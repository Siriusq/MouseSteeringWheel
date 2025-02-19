// 处理键位抬起时，vJoy无法自动复原对应按钮状态的问题

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MouseSteeringWheel.Services
{
    public class KeyboardHookService : IDisposable
    {
        private readonly vJoyService _vJoyService;
        private IntPtr _hookId = IntPtr.Zero;
        private NativeInterop.LowLevelKeyboardProc _proc;

        public event Action<Key, ModifierKeys> KeyDown;
        public event Action<Key, ModifierKeys> KeyUp;

        private Key[] keysArray;
        private ModifierKeys[] modifierKeysArray;

        public KeyboardHookService(vJoyService _vJoyService, Key[] keys, ModifierKeys[] modifierKey)
        {
            this._vJoyService = _vJoyService;
            this.keysArray = keys;
            this.modifierKeysArray = modifierKey;
            _proc = HookCallback;
            InstallHook();

            //this.KeyDown += OnGlobalKeyDown;
            this.KeyUp += OnGlobalKeyUp;
        }

        private void OnGlobalKeyDown(Key key, ModifierKeys modifierKeys)
        {
            if (key == Key.N && modifierKeys == ModifierKeys.Control)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    Console.WriteLine("Ctrl + N键已按下（通过服务触发）"));
            }
        }

        private void OnGlobalKeyUp(Key key, ModifierKeys modifierKeys)
        {
            for (int i = 0; i < 21; i++)
            {
                if (key == keysArray[i] && modifierKeys == modifierKeysArray[i])
                {
                    Console.WriteLine(i + 1);
                    Application.Current.Dispatcher.Invoke(() =>
                        _vJoyService.ResetButtonStatus(i + 1));
                }
            }
        }

        private void InstallHook()
        {
            using (var curModule = System.Diagnostics.Process.GetCurrentProcess().MainModule)
            {
                _hookId = NativeInterop.SetWindowsHookEx(
                    NativeInterop.WH_KEYBOARD_LL,
                    _proc,
                    NativeInterop.GetModuleHandle(curModule.ModuleName),
                    0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var kbStruct = Marshal.PtrToStructure<NativeInterop.KBDLLHOOKSTRUCT>(lParam);
                var key = KeyInterop.KeyFromVirtualKey(kbStruct.vkCode);
                var modifierKey = Keyboard.Modifiers;

                if (wParam == (IntPtr)NativeInterop.WM_KEYDOWN)
                {
                    KeyDown?.Invoke(key, modifierKey);
                }
                else if (wParam == (IntPtr)NativeInterop.WM_KEYUP)
                {
                    KeyUp?.Invoke(key, modifierKey);
                }
            }
            return NativeInterop.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            NativeInterop.UnhookWindowsHookEx(_hookId);
        }
    }
}
