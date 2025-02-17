using MouseSteeringWheel.Services;
using System;
using System.Windows;
using System.Windows.Controls;
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

        public MainWindow()
        {
            InitializeComponent();

            // 初始化vJoyService
            var messageBoxService = new MessageBoxService();
            _vJoyService = new vJoyService(messageBoxService);

            // 创建 MouseInputService，并传入 vJoyService
            _mouseInputService = new MouseInputService(_vJoyService);

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
