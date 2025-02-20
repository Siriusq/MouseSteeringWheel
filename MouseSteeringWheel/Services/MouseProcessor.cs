using MouseSteeringWheel.Services;
using System;

namespace MouseSteeringWheel.Helper
{
    public class MouseProcessor
    {
        private readonly MouseHookService _hookService;
        private readonly vJoyService _vJoyService;
        // 鼠标移动处理
        // 设置灵敏度
        private int _sensitivityX = 30;
        private int _sensitivityY = 30;
        // 设置死区百分比
        private double _deadZoneXPercentage = 0.1;
        private double _deadZoneYPercentage = 0.1;

        public MouseProcessor(MouseHookService hookService, vJoyService vJoyService)
        {
            _vJoyService = vJoyService;
            _hookService = hookService;
            _hookService.MouseAction += OnMouseAction;
        }

        private void OnMouseAction(object sender, MouseHookEventArgs e)
        {
            switch (e.EventType)
            {
                case MouseEventType.Move:
                    UpdateJoystickXY((int)e.RelativeOffset.X, (int)e.RelativeOffset.Y);
                    Console.WriteLine($"相对坐标: X={e.RelativeOffset.X}, Y={e.RelativeOffset.Y}");
                    break;

                case MouseEventType.LeftButtonDown:
                    Console.WriteLine("左键按下");
                    break;

                case MouseEventType.LeftButtonUp:
                    Console.WriteLine("左键抬起");
                    break;
            }
        }

        private void UpdateJoystickXY(int dx, int dy)
        {
            dx *= _sensitivityX;
            dy *= _sensitivityY;
            // 将鼠标移动的XY值转为vJoy设备的XY轴范围，假设最大范围是32767
            int newJoystickX = 16383 + dx;
            int newJoystickY = 16383 + dy;

            // 设置死区
            int deadZoneX = (int)(16383 * _deadZoneXPercentage);
            int deadZoneY = (int)(16383 * _deadZoneYPercentage);
            int deadZoneLeft = 16383 - deadZoneX;
            int deadZoneRight = 16383 + deadZoneX;
            int deadZoneUp = 16383 - deadZoneY;
            int deadZoneDown = 16383 + deadZoneY;

            // 设置阈值
            if (newJoystickX > deadZoneLeft && newJoystickX < deadZoneRight) newJoystickX = 16383;
            else if (newJoystickX < 0) newJoystickX = 0;
            else if (newJoystickX > 32767) newJoystickX = 32767;

            if (newJoystickY > deadZoneUp && newJoystickY < deadZoneDown) newJoystickY = 16383;
            else if (newJoystickY < 0) newJoystickY = 0;
            else if (newJoystickY > 32767) newJoystickY = 32767;

            // 更新摇杆XY位置
            _vJoyService.SetJoystickX(newJoystickX);
            _vJoyService.SetJoystickY(newJoystickY);
        }
    }
}