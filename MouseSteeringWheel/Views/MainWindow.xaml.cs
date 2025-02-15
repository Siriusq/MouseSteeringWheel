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
        private int _lastJoystickX;

        public MainWindow()
        {
            InitializeComponent();

            // 初始化vJoyService
            var messageBoxService = new MessageBoxService();
            _vJoyService = new vJoyService(messageBoxService);

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
        }

        // 更新摇杆位置
        private void UpdateJoystickPosition(object sender, EventArgs e)
        {
            // 获取当前摇杆的 X 值
            int joystickX = _vJoyService.GetJoystickX();

            // Test旋转测试TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest
            _vJoyService.SetJoystickX(joystickX + 10);

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
            });
        }
    }
}
