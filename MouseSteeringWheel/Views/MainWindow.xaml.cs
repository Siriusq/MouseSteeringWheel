using MouseSteeringWheel.Services;
using MouseSteeringWheel.ViewModels;
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
        private readonly MainWindowViewModel _viewModel;
        private double _centerX;
        private double _radius;

        public MainWindow()
        {
            InitializeComponent();

            // 初始化 ViewModel 并绑定到 DataContext
            var messageBoxService = new MessageBoxService();
            var vJoyService = new vJoyService(messageBoxService);
            _viewModel = new MainWindowViewModel(vJoyService);
            this.DataContext = _viewModel;

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

            // 设置圆环的半径和中心点
            _radius = this.Width / 2;  // 半径为窗口宽度的一半
            _centerX = this.Width / 2; // 中心点X坐标
            Console.WriteLine($"{_radius},{_centerX}");
        }

        // 更新摇杆位置
        private void UpdateJoystickPosition()
        {
            // 获取摇杆的 X 轴值
            double joystickX = _viewModel.GetJoystickX();
            Console.WriteLine(joystickX.ToString());

            // 设置摇杆的最大值范围
            double maxRangeX = 32767; // 最大的 X 值
            double angle = (joystickX / maxRangeX) * 90; // 将X值映射到-90度到+90度范围
            Console.WriteLine(angle);

            // 创建 RotateTransform 来旋转摇杆指示器
            if (JoystickPosition.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle = angle; // 设置旋转角度
            }
        }
    }
}
