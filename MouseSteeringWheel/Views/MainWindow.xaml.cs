using MouseSteeringWheel.Services;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly vJoyService _vJoyService;
        private readonly MouseInputService _mouseInputService;
        private int _lastJoystickX;
        private int _lastJoystickY;
        private readonly KeyboardHookService _keyboardHook;
        private readonly HotkeyService _hotkeyService = new HotkeyService();
        private int _nHotkeyId;
        private int _nHotkeyId1;
        private int[] _hotKeyIds = new int[21];

        public MainWindow()
        {
            InitializeComponent();

            ReadSettings();

            // 初始化vJoyService
            var messageBoxService = new MessageBoxService();
            _vJoyService = new vJoyService(messageBoxService);

            // 创建 MouseInputService，并传入 vJoyService
            _mouseInputService = new MouseInputService(_vJoyService);

            // 创建底层键盘勾子
            _keyboardHook = new KeyboardHookService(_vJoyService, keys, modifierKeys);
            Closed += (s, e) => _keyboardHook.Dispose();

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;

            // 设置窗口大小和位置
            InitializeWindow();

            // 使用 CompositionTarget.Rendering 进行全屏鼠标移动监听
            CompositionTarget.Rendering += _mouseInputService.OnMouseMove;

            // 监听Rendering事件，确保每一帧更新UI
            CompositionTarget.Rendering += UpdateJoystickPosition;
        }

        // 初始化窗口大小和位置
        private void InitializeWindow()
        {
            // 获取屏幕分辨率
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // 计算窗口大小（屏幕大小的五分之一）
            this.Width = screenWidth / 5;
            this.Height = screenHeight / 5;

            // 设置窗口位置（屏幕底部居中）
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = screenHeight - this.Height;

            _lastJoystickX = 16383;
            _lastJoystickY = 16383;
        }

        // 更新指示器位置
        private void UpdateJoystickPosition(object sender, EventArgs e)
        {
            // 获取当前摇杆的 X 值
            int joystickX = _vJoyService.GetJoystickX();
            int joystickY = _vJoyService.GetJoystickY();

            // 使用Dispatcher确保UI更新在主线程上执行
            Dispatcher.Invoke(() =>
            {
                // 如果摇杆的 X 值变化了，更新UI
                if (joystickX != _lastJoystickX)
                {
                    // 设置摇杆的最大值范围
                    double maxRangeX = 32767;
                    double angle = (joystickX / maxRangeX) * 180;

                    // 旋转摇杆指示器
                    RotateTransform rotateTransform = Indicator;
                    if (rotateTransform != null)
                    {
                        rotateTransform.Angle = angle;// 设置旋转角度
                    }

                    // 更新记录的摇杆X值
                    _lastJoystickX = joystickX;
                }

                if (joystickY != _lastJoystickY)
                {
                    // 设置油门
                    if (joystickY < 16383)
                    {
                        double throttleVal = (joystickY - 16383) * 150 / 16383;
                        ThrottleIndicator.Y = throttleVal;
                    }
                    // 设置刹车
                    else if (joystickY > 16383)
                    {
                        double breakVal = (joystickY - 16383) * 150 / 16383;
                        BreakIndicator.Y = breakVal;
                    }
                    // 归零
                    else
                    {
                        ThrottleIndicator.Y = 0;
                        BreakIndicator.Y = 0;
                    }

                    // 更新记录的摇杆Y值
                    _lastJoystickY = joystickY;
                }
            });
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化热键服务
            var windowHandle = new WindowInteropHelper(this).Handle;
            _hotkeyService.Initialize(windowHandle);

            // 注册Ctrl + Alt + N热键
            _nHotkeyId = _hotkeyService.RegisterHotkey(
                modifiers: NativeInterop.MOD_NONE,
                keyCode: (uint)KeyInterop.VirtualKeyFromKey(Key.NumPad1),
                callback: () => Dispatcher.Invoke(() =>
                    _vJoyService.MapKeyToButton(1))
            );
            _nHotkeyId1 = _hotkeyService.RegisterHotkey(
                modifiers: NativeInterop.MOD_NONE,
                keyCode: (uint)KeyInterop.VirtualKeyFromKey(Key.NumPad2),
                callback: () => Dispatcher.Invoke(() =>
                    _vJoyService.MapKeyToButton(2))
            );
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            foreach (int _hotKeys in _hotKeyIds)
            {
                _hotkeyService.UnregisterHotkey(_hotKeys);
            }
            _hotkeyService.Dispose();
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
        }

        // 关闭时重置摇杆位置
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CompositionTarget.Rendering -= _mouseInputService.OnMouseMove;
            CompositionTarget.Rendering -= UpdateJoystickPosition;
            Console.WriteLine("Released");
            _vJoyService.ResetJoystickPos();
        }
    }
}
