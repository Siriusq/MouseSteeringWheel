using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;


namespace MouseSteeringWheel.Services
{
    public class MouseInputService
    {
        private readonly vJoyService _vJoyService;

        public MouseInputService(vJoyService vJoyService)
        {
            _vJoyService = vJoyService;
            Console.WriteLine(SystemParameters.PrimaryScreenWidth / 2);
            Console.WriteLine(Screen.PrimaryScreen.Bounds.Width / 2);
        }

        // 鼠标移动处理
        // 设置屏幕中央为原点
        private int OriginX = Screen.PrimaryScreen.Bounds.Width / 2;
        private int OriginY = Screen.PrimaryScreen.Bounds.Height / 2;
        // 设置灵敏度
        private int sensitivityX = 30;
        private int sensitivityY = 30;
        // 设置死区百分比
        private double deadZoneXPercentage = 0.1;
        private double deadZoneYPercentage = 0.1;

        public void OnMouseMove(object sender, EventArgs e)
        {
            var point = Control.MousePosition;
            // 计算鼠标的X轴偏移量
            int deltaX = point.X - OriginX;
            int deltaY = point.Y - OriginY;

            // 灵敏度调整
            int x = deltaX * sensitivityX;
            int y = deltaY * sensitivityY;
            // 根据鼠标移动更新vJoy的XY值
            UpdateJoystickX(x, y);
        }

        private void UpdateJoystickX(int dx, int dy)
        {
            // 将鼠标移动的XY值转为vJoy设备的XY轴范围，假设最大范围是32767
            int newJoystickX = 16383 + dx;
            int newJoystickY = 16383 + dy;

            // 设置死区
            int deadZoneX = (int)(16383 * deadZoneXPercentage);
            int deadZoneY = (int)(16383 * deadZoneYPercentage);
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
            Console.WriteLine($"{newJoystickX} {newJoystickY}");
        }
    }
}
