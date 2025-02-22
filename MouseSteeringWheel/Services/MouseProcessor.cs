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
        // 非线性映射
        private float _nonlinearCoefficient = 5.0f;
        // 设置死区百分比
        private double _deadZoneXPercentage = 0.01;
        private double _deadZoneYPercentage = 0.01;
        // 常量
        private const int Center = 16383;
        private const int MaxRange = 32767;

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

        // 将鼠标的坐标映射为摇杆位置，dx为鼠标X轴横向坐标，dy为鼠标Y轴竖向坐标，坐标轴圆点为屏幕中心。
        // dx和dy的范围与屏幕分辨率相关，若屏幕分辨率为2560*1440，则dx的范围是-1280至+1280，dy的范围是-720至+720
        // 计算出的映射至newJoystickX和newJoystickY的范围都是0至32767
        // 两个常量：Center = 16383, MaxRange = 32767
        // _sensitivityX 和 _sensitivityY 为 X和Y轴的灵敏度
        // _deadZoneXPercentage 和 _deadZoneYPercentage为 X和Y轴的死区占比，范围是0到1，也就是0%到100%
        private void UpdateJoystickXY(int dx, int dy)
        {
            // 设置灵敏度
            dx *= _sensitivityX;
            dy *= _sensitivityY;

            // ===== 非线性映射核心算法 =====
            // 将坐标转换为相对中心的比例（范围[-1,1]）
            double dxNormalized = (double)dx / Center;
            double dyNormalized = (double)dy / Center;

            //// 将鼠标移动的XY值转为vJoy设备的XY轴范围，假设最大范围是32767
            //int newJoystickX = Center + dx;
            //int newJoystickY = Center + dy;
            Console.WriteLine($"OVal:{Center + dx}");

            // 计算极坐标半径（范围[0,1]）
            double r = Math.Sqrt(dxNormalized * dxNormalized + dyNormalized * dyNormalized);
            r = Math.Min(r, 1.0); // 强制限制在单位圆内

            // 应用非线性变换公式：r' = r^k
            double rPrime = Math.Pow(r, _nonlinearCoefficient);
            double scale = r > 0.0001 ? rPrime / r : 0; // 避免除零错误

            // 计算调整后的坐标偏移量
            int adjustedDx = (int)(dxNormalized * scale * Center);
            int adjustedDy = (int)(dyNormalized * scale * Center);

            // ===== 坐标转换 =====
            int newJoystickX = Center + adjustedDx;
            int newJoystickY = Center + adjustedDy;

            Console.WriteLine($"NVal:{newJoystickX}");
            Console.WriteLine($"diff:{newJoystickX - dx - Center}");

            // ===== 死区处理 =====
            // 计算死区边界（矩形死区）
            int deadZoneX = (int)(Center * _deadZoneXPercentage);
            int deadZoneY = (int)(Center * _deadZoneYPercentage);

            // X轴死区处理
            if (newJoystickX > (Center - deadZoneX) &&
                newJoystickX < (Center + deadZoneX))
            {
                newJoystickX = Center;
            }
            else // 边界限制
            {
                newJoystickX = Math.Max(0, Math.Min(newJoystickX, MaxRange));
            }

            // Y轴死区处理
            if (newJoystickY > (Center - deadZoneY) &&
                newJoystickY < (Center + deadZoneY))
            {
                newJoystickY = Center;
            }
            else // 边界限制
            {
                newJoystickY = Math.Max(0, Math.Min(newJoystickY, MaxRange));
            }

            // 更新摇杆XY位置
            _vJoyService.SetJoystickX(newJoystickX);
            _vJoyService.SetJoystickY(newJoystickY);
        }
    }
}