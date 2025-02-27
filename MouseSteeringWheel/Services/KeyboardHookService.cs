// 处理键位抬起时，vJoy无法自动复原对应按钮状态的问题

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace MouseSteeringWheel.Services
{
    public class KeyboardHookService : IDisposable
    {
        private readonly ApplicationStateService _stateService;
        private readonly vJoyService _vJoyService;
        private IntPtr _hookId = IntPtr.Zero;
        private User32API.LowLevelKeyboardProc _proc;

        public event Action<Key, ModifierKeys> KeyDown;
        public event Action<Key, ModifierKeys> KeyUp;

        private Key[] keysArray;
        private ModifierKeys[] modifierKeysArray;

        public KeyboardHookService(vJoyService _vJoyService, Key[] keys, ModifierKeys[] modifierKey, ApplicationStateService stateService)
        {
            this._stateService = stateService;
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
            for (int i = 0; i < 21; i++)
            {
                if (key == keysArray[i] && modifierKeys == modifierKeysArray[i])
                {
                    Console.WriteLine(i + 1);
                    Application.Current.Dispatcher.Invoke(() =>
                        _vJoyService.MapKeyToButton(i + 1));
                }
            }
        }

        private void OnGlobalKeyUp(Key key, ModifierKeys modifierKeys)
        {
            for (int i = 1; i <= 128; i++)
            {
                if (key == keysArray[i] && modifierKeys == modifierKeysArray[i])
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        _vJoyService.ResetButtonStatus(i));
                }
            }
        }

        private void InstallHook()
        {
            using (var curModule = System.Diagnostics.Process.GetCurrentProcess().MainModule)
            {
                _hookId = User32API.SetWindowsHookEx(
                    User32API.WH_KEYBOARD_LL,
                    _proc,
                    User32API.GetModuleHandle(curModule.ModuleName),
                    0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && !_stateService.IsPaused)
            {
                var kbStruct = Marshal.PtrToStructure<User32API.KBDLLHOOKSTRUCT>(lParam);
                var key = KeyInterop.KeyFromVirtualKey(kbStruct.vkCode);
                var modifierKey = Keyboard.Modifiers;

                if (wParam == (IntPtr)User32API.WM_KEYDOWN)
                {
                    KeyDown?.Invoke(key, modifierKey);
                }
                else if (wParam == (IntPtr)User32API.WM_KEYUP)
                {
                    KeyUp?.Invoke(key, modifierKey);
                }
            }
            return User32API.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            User32API.UnhookWindowsHookEx(_hookId);
        }
    }
}
