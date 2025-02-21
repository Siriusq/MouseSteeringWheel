using MouseSteeringWheel.Services;
using MouseSteeringWheel.Helper;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApplicationStateService _stateService = new ApplicationStateService();
        private readonly vJoyService _vJoyService;
        private readonly KeyboardHookService _keyboardHook;
        private readonly HotkeyProcessor _hotkeyProcessor;
        private int[] _vJoyBtnHotKeyIds = new int[21];
        private int _settingHotKey;
        private int _pauseHotKey;
        private readonly MouseHookService _mouseService;
        private readonly MouseProcessor _mouseProcessor;
        private BottomSteeringWheel _bottomSteeringWheel;
        private BottomJoystick _bottomJoystick;
        private int uiId;
        private int _bottomJoystickYPos; // 摇杆UI到屏幕底部的距离

        public MainWindow()
        {
            InitializeComponent();

            // 读取设置
            ReadSettings();

            // 初始化
            var messageBoxService = new MessageBoxService();
            _vJoyService = new vJoyService(messageBoxService);

            // 键盘勾子
            _hotkeyProcessor = new HotkeyProcessor(_stateService);
            _keyboardHook = new KeyboardHookService(_vJoyService, keys, modifierKeys, _stateService);
            Closed += (s, e) => _keyboardHook.Dispose();

            // 鼠标勾子
            _mouseService = new MouseHookService(_stateService);
            _mouseProcessor = new MouseProcessor(_mouseService, _vJoyService);

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;

            // UI切换
            if (uiId == 1)
            {
                _bottomSteeringWheel = new BottomSteeringWheel(_vJoyService);
                UIContainer.Content = _bottomSteeringWheel;
                // 监听Rendering事件，确保每一帧更新UI
                CompositionTarget.Rendering += BottomSteeringWheelOnRendering;
            }
            else
            {
                _bottomJoystick = new BottomJoystick(_vJoyService);
                _bottomJoystick.UIPosition.Y = -_bottomJoystickYPos;
                UIContainer.Content = _bottomJoystick;
                // 监听Rendering事件，确保每一帧更新UI
                CompositionTarget.Rendering += BottomJoystickOnRendering;
            }

            // 设置窗口大小和位置
            InitializeWindow();
        }

        // 初始化窗口大小和位置
        private void InitializeWindow()
        {
            // 获取屏幕分辨率
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // 设置窗口大小
            this.Width = screenWidth;
            this.Height = screenHeight;

            // 设置窗口位置（屏幕底部居中）
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = screenHeight - this.Height;
        }

        // 渲染事件的回调方法
        private void BottomSteeringWheelOnRendering(object sender, EventArgs e)
        {
            // 调用 CircleUI 的 UpdateJoystickPosition 方法
            _bottomSteeringWheel.UpdateJoystickPosition(sender, e);
        }

        private void BottomJoystickOnRendering(object sender, EventArgs e)
        {
            // 调用 CircleUI 的 UpdateJoystickPosition 方法
            _bottomJoystick.UpdateJoystickPosition(sender, e);
        }

        // 注册快捷键
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化热键服务
            var windowHandle = new WindowInteropHelper(this).Handle;
            _hotkeyProcessor.Initialize(windowHandle);

            // 注册热键
            // 使用立即计算的局部变量解决闭包变量捕获问题
            for (int index = 0; index < keys.Length; index++)
            {
                int buttonId = index + 1; // 按钮ID从1开始
                Key currentKey = keys[index];

                _vJoyBtnHotKeyIds[index] = _hotkeyProcessor.RegisterHotkeyWithPauseCheck(
                    id: buttonId,
                    modifiers: User32API.MOD_NONE,
                    keyCode: (uint)KeyInterop.VirtualKeyFromKey(currentKey),
                    callback: () => Dispatcher.Invoke(() =>
                        _vJoyService.MapKeyToButton(buttonId)) // 这里使用局部变量
                );
            }

            // 特殊热键：暂停摇杆响应和打开设置
            _pauseHotKey = _hotkeyProcessor.RegisterHotkey(
                    id: 98,
                    modifiers: User32API.MOD_NONE,
                    keyCode: (uint)KeyInterop.VirtualKeyFromKey(Key.NumPad8),
                    callback: () => Dispatcher.Invoke(() =>
                        _stateService.TogglePauseState())
                );

            _settingHotKey = _hotkeyProcessor.RegisterHotkey(
                    id: 99,
                    modifiers: User32API.MOD_NONE,
                    keyCode: (uint)KeyInterop.VirtualKeyFromKey(Key.Add),
                    callback: () => Dispatcher.Invoke(() =>
                        Console.WriteLine("Open Settings"))
                );
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            foreach (var id in _vJoyBtnHotKeyIds.Where(id => id != 0))
            {
                _hotkeyProcessor.UnregisterHotkey(id);
            }
            _hotkeyProcessor.UnregisterHotkey(_settingHotKey);
            _hotkeyProcessor.UnregisterHotkey(_pauseHotKey);
            _hotkeyProcessor.Dispose();
            _mouseService.Dispose();
            CompositionTarget.Rendering -= BottomSteeringWheelOnRendering;
            // 关闭时重置摇杆位置
            _vJoyService.ResetJoystickPos();
        }

        // 读取设置选项
        private Key[] keys;
        private ModifierKeys[] modifierKeys;
        private void ReadSettings()
        {
            keys = new Key[] { Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad5,
                Key.NumPad7, Key.NumPad4,Key.NumPad9,Key.NumPad6,
            Key.Home,Key.End,Key.Delete,Key.PageDown,Key.PageUp,Key.Insert,
            Key.Up,Key.Down,Key.Left,Key.Right,
            Key.Divide,Key.Multiply,Key.Subtract,
            };
            modifierKeys = new ModifierKeys[] { ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, ModifierKeys.None, };
            uiId = 2;
            _bottomJoystickYPos = 100;
        }
    }
}
