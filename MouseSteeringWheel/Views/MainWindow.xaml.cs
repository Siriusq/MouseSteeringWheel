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

        public MainWindow()
        {
            InitializeComponent();

            // 初始化vJoyService
            var messageBoxService = new MessageBoxService();
            _vJoyService = new vJoyService(messageBoxService);

            // 设置窗口大小和位置
            InitializeWindow();

            // 启动定时器更新摇杆位置
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += (sender, args) => UpdateJoystickPosition();
            timer.Interval = TimeSpan.FromMilliseconds(100); // 每 100 毫秒更新一次
            timer.Start();
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
        private void UpdateJoystickPosition()
        {
            // 获取摇杆的 X 轴值
            int joystickX = _vJoyService.GetJoystickX();
            Console.WriteLine($"X: {joystickX}");

            // Test旋转测试TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest
            _vJoyService.SetJoystickX(joystickX + 100);

            // 设置摇杆的最大值范围
            double maxRangeX = 32767; // 最大的 X 值
            double angle = (joystickX / maxRangeX) * 180; // 将X值映射到0度到180度范围
            Console.WriteLine($"Angel: {angle}");

            // 旋转摇杆指示器
            RotateTransform rotateTransform = Indicator;
            if (rotateTransform != null)
            {
                rotateTransform.Angle = angle; // 设置旋转角度
            }
        }
    }
}
