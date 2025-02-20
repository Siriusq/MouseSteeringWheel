using System.Windows;

namespace MouseSteeringWheel.Services
{
    public static class DisplayService
    {
        /// <summary>
        /// 获取主显示器的物理分辨率
        /// </summary>
        public static Size GetPrimaryScreenPhysicalSize()
        {
            return new Size(
                User32API.GetSystemMetrics(User32API.SM_CXSCREEN),
                User32API.GetSystemMetrics(User32API.SM_CYSCREEN)
            );
        }

        /// <summary>
        /// 获取虚拟屏幕的物理尺寸（多显示器）
        /// </summary>
        public static Rect GetVirtualScreenPhysicalBounds()
        {
            return new Rect(
                0,
                0,
                User32API.GetSystemMetrics(User32API.SM_CXVIRTUALSCREEN),
                User32API.GetSystemMetrics(User32API.SM_CYVIRTUALSCREEN)
            );
        }

        /// <summary>
        /// 获取当前系统缩放比例
        /// </summary>
        public static double GetScalingFactor()
        {
            var logicalWidth = SystemParameters.PrimaryScreenWidth;
            var physicalWidth = User32API.GetSystemMetrics(User32API.SM_CXSCREEN);
            return physicalWidth / logicalWidth;
        }
    }
}
