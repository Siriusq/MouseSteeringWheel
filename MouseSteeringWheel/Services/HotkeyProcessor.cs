// 注册全局快捷键，阻断其他程序对已注册快捷键的响应，在快捷键按下时设置vJoy相关按钮状态为按下

using MouseSteeringWheel.Properties;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MouseSteeringWheel.Services
{
    public class HotkeyProcessor : IDisposable
    {
        private readonly Dictionary<int, Action> _hotkeyActions = new Dictionary<int, Action>();
        private HwndSource _hwndSource;
        private readonly ApplicationStateService _stateService;

        public HotkeyProcessor(ApplicationStateService stateService)
        {
            _stateService = stateService;
        }

        public void Initialize(IntPtr windowHandle)
        {
            var hwndSource = HwndSource.FromHwnd(windowHandle);
            hwndSource?.AddHook(WndProc);
            _hwndSource = hwndSource;
        }

        public int RegisterHotkey(int id, ModifierKeys modifiers, uint keyCode, Action callback)
        {
            if (User32API.RegisterHotKey(_hwndSource.Handle, id, modifiers, keyCode))
            {
                _hotkeyActions[id] = callback;
                return id;
            }
            throw new InvalidOperationException(Resources.RegistHotKeyFailedPop);
        }

        // vJoy按键对应快捷键注册，支持暂停功能
        public int RegisterHotkeyWithPauseCheck(
            int id,
            ModifierKeys modifiers,
            uint keyCode,
            Action callback)
        {
            return RegisterHotkey(id, modifiers, keyCode, () =>
            {
                if (!_stateService.IsPaused)
                {
                    callback?.Invoke();
                }
            });
        }

        public void UnregisterHotkey(int hotkeyId)
        {
            if (_hotkeyActions.ContainsKey(hotkeyId))
            {
                User32API.UnregisterHotKey(_hwndSource.Handle, hotkeyId);
                _hotkeyActions.Remove(hotkeyId);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == User32API.WM_HOTKEY)
            {
                var hotkeyId = wParam.ToInt32();
                if (_hotkeyActions.TryGetValue(hotkeyId, out var action))
                {
                    action?.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            foreach (var id in _hotkeyActions.Keys)
            {
                User32API.UnregisterHotKey(_hwndSource.Handle, id);
            }
            _hwndSource?.RemoveHook(WndProc);
        }
    }
}
