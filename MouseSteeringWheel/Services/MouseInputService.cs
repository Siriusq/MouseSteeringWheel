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
        private int lastX = Screen.PrimaryScreen.Bounds.Width / 2;

        private int OriginX = Screen.PrimaryScreen.Bounds.Width / 2;
        private int OriginY = Screen.PrimaryScreen.Bounds.Height / 2;
        private int sensitivity = 60;

        public void OnMouseMove(object sender, EventArgs e)
        {
            var point = Control.MousePosition;
            // 计算鼠标的X轴偏移量
            var deltaX = point.X - OriginX;

            // 更新之前的位置
            lastX = point.X;
            Console.WriteLine($"{deltaX} {OriginX}");

            // 根据鼠标移动的X轴更新vJoy的X值
            UpdateJoystickX(deltaX);

        }

        private void UpdateJoystickX(double deltaX)
        {
            // 将鼠标移动的X值转为vJoy设备的X轴范围，假设最大范围是32767
            int newJoystickX = 16383 + (int)(deltaX * sensitivity); // 调整灵敏度
            if (newJoystickX < 0) newJoystickX = 0;
            if (newJoystickX > 32767) newJoystickX = 32767;
            _vJoyService.SetJoystickX(newJoystickX);
        }
    }
}
