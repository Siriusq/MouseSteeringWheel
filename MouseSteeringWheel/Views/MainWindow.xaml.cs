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

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly vJoyService _vJoyService;
        private int _lastJoystickX;
        private int _lastJoystickY;
        private readonly KeyboardHookService _keyboardHook;
        private readonly HotkeyProcessor _hotkeyProcessor = new HotkeyProcessor();
        private int[] _hotKeyIds = new int[21];
        private readonly MouseHookService _mouseService;
        private readonly MouseProcessor _processor;

        public MainWindow()
        {
            InitializeComponent();

            ReadSettings();

            // 初始化vJoyService
            var messageBoxService = new MessageBoxService();
            _vJoyService = new vJoyService(messageBoxService);

            // 创建底层键盘勾子
            _keyboardHook = new KeyboardHookService(_vJoyService, keys, modifierKeys);
            Closed += (s, e) => _keyboardHook.Dispose();

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;

            // 鼠标勾子
            _mouseService = new MouseHookService();
            _processor = new MouseProcessor(_mouseService, _vJoyService);

            Closed += (s, e) => _mouseService.Dispose();

            // 设置窗口大小和位置
            InitializeWindow();

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
                    if (joystickY > 16383)
                    {
                        double throttleVal = (16383 - joystickY) * 150 / 16383;
                        ThrottleIndicator.Y = throttleVal;
                        BreakIndicator.Y = 0;
                    }
                    // 设置刹车
                    else if (joystickY < 16383)
                    {
                        double breakVal = (16383 - joystickY) * 150 / 16383;
                        BreakIndicator.Y = breakVal;
                        ThrottleIndicator.Y = 0;
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
            _hotkeyProcessor.Initialize(windowHandle);

            // 注册热键
            // 使用立即计算的局部变量解决闭包变量捕获问题
            for (int index = 0; index < keys.Length; index++)
            {
                int buttonId = index + 1; // 按钮ID从1开始
                Key currentKey = keys[index];

                _hotKeyIds[index] = _hotkeyProcessor.RegisterHotkey(
                    modifiers: User32API.MOD_NONE,
                    keyCode: (uint)KeyInterop.VirtualKeyFromKey(currentKey),
                    callback: () => Dispatcher.Invoke(() =>
                        _vJoyService.MapKeyToButton(buttonId)) // 这里使用局部变量
                );
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            foreach (var id in _hotKeyIds.Where(id => id != 0))
            {
                _hotkeyProcessor.UnregisterHotkey(id);
            }
            _hotkeyProcessor.Dispose();
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
            CompositionTarget.Rendering -= UpdateJoystickPosition;
            Console.WriteLine("Released");
            _vJoyService.ResetJoystickPos();
        }
    }
}
