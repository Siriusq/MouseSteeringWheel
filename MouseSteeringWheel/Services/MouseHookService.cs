using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace MouseSteeringWheel.Services
{
    public enum MouseEventType
    {
        Move,
        LeftButtonDown,
        LeftButtonUp,
        RightButtonDown,
        RightButtonUp,
        Wheel
    }

    public class MouseHookEventArgs : EventArgs
    {
        /// <summary>
        /// 物理坐标（实际像素）
        /// </summary>
        public Point PhysicalPosition { get; set; }

        /// <summary>
        /// 逻辑坐标（考虑缩放后的坐标）
        /// </summary>
        public Point LogicalPosition { get; set; }

        /// <summary>
        /// 相对于屏幕中心的偏移量
        /// </summary>
        public Vector RelativeOffset { get; set; }

        public MouseEventType EventType { get; set; }
    }

    public class MouseHookService : IDisposable
    {
        private readonly Point _screenCenter;
        private IntPtr _hookId = IntPtr.Zero;
        private NativeInterop.LowLevelMouseProc _proc;
        private readonly Point _physicalScreenCenter;
        private readonly double _scalingFactor;

        public Point ScreenCenter => _screenCenter;

        public event EventHandler<MouseHookEventArgs> MouseAction;
        public MouseHookService()
        {
            _screenCenter = new Point(
                SystemParameters.PrimaryScreenWidth / 2,
                SystemParameters.PrimaryScreenHeight / 2);

            // 获取物理分辨率
            var physicalSize = DisplayService.GetPrimaryScreenPhysicalSize();
            _physicalScreenCenter = new Point(
                physicalSize.Width / 2,
                physicalSize.Height / 2
            );

            // 计算缩放因子
            _scalingFactor = DisplayService.GetScalingFactor();

            _proc = HookCallback;
            InstallHook();
        }

        private void InstallHook()
        {
            using (var processModule = System.Diagnostics.Process.GetCurrentProcess().MainModule)
            {
                _hookId = NativeInterop.SetWindowsHookEx(
                    NativeInterop.WH_MOUSE_LL,
                    _proc,
                    NativeInterop.GetModuleHandle(processModule.ModuleName),
                    0);
            }

            if (_hookId == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = Marshal.PtrToStructure<NativeInterop.MSLLHOOKSTRUCT>(lParam);
                var eventType = ParseEventType(wParam);

                // 转换为物理坐标
                var physicalPoint = new Point(
                    hookStruct.pt.X,
                    hookStruct.pt.Y
                );

                // 计算相对坐标（基于物理中心点）
                var relativeOffset = new Vector(
                    physicalPoint.X - _physicalScreenCenter.X,
                    _physicalScreenCenter.Y - physicalPoint.Y
                );

                // 转换为逻辑坐标（如果需要与WPF界面交互）
                var logicalPoint = new Point(
                    physicalPoint.X / _scalingFactor,
                    physicalPoint.Y / _scalingFactor
                );

                var args = new MouseHookEventArgs
                {
                    EventType = eventType,
                    PhysicalPosition = physicalPoint,
                    LogicalPosition = logicalPoint,
                    RelativeOffset = relativeOffset
                };

                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action(() => MouseAction?.Invoke(this, args)));
            }
            return NativeInterop.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private MouseEventType ParseEventType(IntPtr wParam)
        {
            switch ((int)wParam)
            {
                case NativeInterop.WM_LBUTTONDOWN: return MouseEventType.LeftButtonDown;
                case NativeInterop.WM_LBUTTONUP: return MouseEventType.LeftButtonUp;
                case NativeInterop.WM_RBUTTONDOWN: return MouseEventType.RightButtonDown;
                case NativeInterop.WM_RBUTTONUP: return MouseEventType.RightButtonUp;
                case NativeInterop.WM_MOUSEWHEEL: return MouseEventType.Wheel;
                default: return MouseEventType.Move;
            }
        }

        public void Dispose()
        {
            NativeInterop.UnhookWindowsHookEx(_hookId);
            GC.SuppressFinalize(this);
        }
    }
}