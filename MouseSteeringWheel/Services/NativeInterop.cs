using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace MouseSteeringWheel.Services
{
    public static class NativeInterop
    {
        // 通用常量
        public const int WH_MOUSE_LL = 14;
        public const int WH_KEYBOARD_LL = 13;

        // 鼠标相关
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MOUSEWHEEL = 0x020A;

        // 键盘相关
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_HOTKEY = 0x0312;

        // 修饰键标志
        public const uint MOD_NONE = 0x0000;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        //屏幕分辨率相关
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        public const int SM_CXVIRTUALSCREEN = 78;
        public const int SM_CYVIRTUALSCREEN = 79;

        #region 鼠标API函数
        // API 函数
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelMouseProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        // 委托
        public delegate IntPtr LowLevelMouseProc(
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        // 结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT p)
            {
                return new Point(p.X, p.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        #endregion

        #region 键盘相关
        // 注册热键API
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        // 注销热键API
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        public delegate IntPtr LowLevelKeyboardProc(
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        #endregion

        #region 屏幕分辨率

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);

        #endregion

        // 勾子相关
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
