using MouseSteeringWheel.Services;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using MouseSteeringWheel.Properties;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// BottomJoystick.xaml 的交互逻辑
    /// </summary>
    public partial class BottomJoystick : UserControl
    {
        private readonly vJoyService _vJoyService;
        private int _lastJoystickX;
        private int _lastJoystickY;

        public BottomJoystick(vJoyService vJoyService)
        {

            InitializeComponent();

            this._vJoyService = vJoyService;
            _lastJoystickX = 16383;
            _lastJoystickY = 16383;

            // 设置UI缩放
            SetUIScale();
        }

        public void SetUIScale()
        {
            UIScale.ScaleX = Settings.Default.UIScaleFactor;
            UIScale.ScaleY = Settings.Default.UIScaleFactor;
        }

        // 更新指示器位置
        public void UpdateJoystickPosition(object sender, EventArgs e)
        {
            // 获取当前摇杆的 X 值
            int joystickX = _vJoyService.GetJoystickX();
            int joystickY = _vJoyService.GetJoystickY();

            // 使用Dispatcher确保UI更新在主线程上执行
            Dispatcher.Invoke(() =>
            {
                // 如果摇杆的 X 值变化了，更新UI
                if (joystickX != _lastJoystickX || joystickY != _lastJoystickY)
                {
                    // 摇杆指示器
                    TranslateTransform translateTransform = JoyStickPosIndicator;

                    double xPos = (joystickX - 16383) * 50 / 16383;
                    double yPos = (16383 - joystickY) * 50 / 16383;

                    // 如果距离超过半径，缩放摇杆位置
                    double distance = Math.Sqrt(xPos * xPos + yPos * yPos);
                    if (distance > 50)
                    {
                        double scale = 50 / distance;
                        xPos *= scale;
                        yPos *= scale;
                    }

                    translateTransform.X = xPos;
                    translateTransform.Y = yPos;

                    // 更新记录的摇杆XY值
                    _lastJoystickX = joystickX;
                    _lastJoystickY = joystickY;
                }
            });
        }
    }
}
