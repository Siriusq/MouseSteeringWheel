using MouseSteeringWheel.Services;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// BottomSteeringWheel.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class BottomSteeringWheel : UserControl
    {
        private readonly vJoyService _vJoyService;
        private int _lastJoystickX;
        private int _lastJoystickY;

        public BottomSteeringWheel(vJoyService vJoyService)
        {
            InitializeComponent();

            this._vJoyService = vJoyService;
            _lastJoystickX = 16383;
            _lastJoystickY = 16383;
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
                if (joystickX != _lastJoystickX)
                {
                    // 设置摇杆的最大值范围
                    double maxRangeX = 32767;
                    double angle = (joystickX / maxRangeX) * 180;

                    // 旋转摇杆指示器
                    Indicator.Angle = angle;// 设置旋转角度

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
    }
}
