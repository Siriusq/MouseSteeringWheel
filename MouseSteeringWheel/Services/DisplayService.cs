using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                NativeInterop.GetSystemMetrics(NativeInterop.SM_CXSCREEN),
                NativeInterop.GetSystemMetrics(NativeInterop.SM_CYSCREEN)
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
                NativeInterop.GetSystemMetrics(NativeInterop.SM_CXVIRTUALSCREEN),
                NativeInterop.GetSystemMetrics(NativeInterop.SM_CYVIRTUALSCREEN)
            );
        }

        /// <summary>
        /// 获取当前系统缩放比例
        /// </summary>
        public static double GetScalingFactor()
        {
            var logicalWidth = SystemParameters.PrimaryScreenWidth;
            var physicalWidth = NativeInterop.GetSystemMetrics(NativeInterop.SM_CXSCREEN);
            return physicalWidth / logicalWidth;
        }
    }
}
