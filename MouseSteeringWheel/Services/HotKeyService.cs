using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MouseSteeringWheel.Services
{
    public class HotKeyService
    {
        private readonly vJoyService _vJoyService;
        private readonly Window _window;
        public HotKeyService(vJoyService vJoyService, Window window)
        {
            _vJoyService = vJoyService;
            _window = window;

            BindHotKeyToButton(ModifierKeys.None, Key.N, 2);
        }

        private void BindHotKeyToButton(ModifierKeys fsModifiers, Key key, int buttonId)
        {
            Regist(_window, fsModifiers, key, () =>
            {
                _vJoyService.MapKeyToButton(buttonId);
            });
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int WM_HOTKEY = 0x312;
        public delegate void HotKeyCallBackHanlder();
        public const int HOTKEY_ID = 9000;
        //Commands that need to be executed by the hot key
        public static HotKeyCallBackHanlder hotKeyCallBackHanlder = null;

        /// <summary>
        /// Register a new hotkey
        /// </summary>
        /// <param name="window">The window that holds the shortcut keys</param>
        /// <param name="fsModifiers">Modifier Keys like Control, Alt and Shift</param>
        /// <param name="key">Key like ABCDEFG</param>
        /// <param name="callBack">callback function</param>
        public static void Regist(Window window, ModifierKeys fsModifiers, Key key, HotKeyCallBackHanlder callBack)
        {
            //获取窗口的句柄
            var hwnd = new WindowInteropHelper(window).Handle;
            //使用窗口句柄创建一个 HwndSource 对象
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            //将消息处理委托 WndProc 添加到窗口的消息处理链中，窗口收到消息后会先传递给 WndProc 方法进行处理
            _hwndSource.AddHook(WndProc);

            //将传入的Key转化为虚拟键码vk
            var vk = KeyInterop.VirtualKeyFromKey(key);

            //如果注册失败则弹窗报错
            if (!RegisterHotKey(hwnd, HOTKEY_ID, fsModifiers, (uint)vk))
                MessageBox.Show("Failed to register global hotkey!");

            //将回调函数赋值给了 hotKeyCallBackHanlder 变量。这个回调函数就是在注册时由主窗口传入的 callBack 参数
            hotKeyCallBackHanlder = callBack;
        }

        /// <summary> 
        /// Hot Key Message Handling 
        /// </summary> 
        static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //当收到 WM_HOTKEY 消息并且热键 ID 匹配时，会调用 hotKeyCallBackHanlder()，即执行了预先设置的回调函数。
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                hotKeyCallBackHanlder();
            }
            return IntPtr.Zero;
        }


        /// <summary> 
        /// Unregister a exist hot key
        /// </summary> 
        public static void UnRegist(WindowInteropHelper windowInteropHelper)
        {
            UnregisterHotKey(windowInteropHelper.Handle, HOTKEY_ID);
        }

        /// <summary>
        /// Modify a exist hotkey, including unregister the old one and register a new one
        /// </summary>
        /// <param name="windowInteropHelper">Handle of the window holding the hotkey</param>
        /// <param name="key">Key like ABCDEFG</param>
        /// <param name="modifiers">Modifier Keys like Control, Alt and Shift</param>
        public static void ModifyGlobalHotKey(WindowInteropHelper windowInteropHelper, Key key, ModifierKeys modifiers)
        {
            //注销先前的热键
            UnregisterHotKey(windowInteropHelper.Handle, HOTKEY_ID);
            //尝试注册新的热键
            if (!RegisterHotKey(windowInteropHelper.Handle, HOTKEY_ID, modifiers, (uint)KeyInterop.VirtualKeyFromKey(key)))
                MessageBox.Show("Failed to register global hotkey!");
        }

    }
}
