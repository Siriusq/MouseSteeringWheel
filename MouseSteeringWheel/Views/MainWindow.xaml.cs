using MouseSteeringWheel.Properties;
using MouseSteeringWheel.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApplicationStateService _stateService = new ApplicationStateService();
        private SettingWindow settingWindow;
        private readonly vJoyService _vJoyService;
        private KeyboardHookService _keyboardHook;
        private readonly HotkeyProcessor _hotkeyProcessor;
        private readonly MouseHookService _mouseService;
        private readonly MouseProcessor _mouseProcessor;
        private BottomSteeringWheel _bottomSteeringWheel;
        private BottomJoystick _bottomJoystick;

        public MainWindow()
        {
            InitializeComponent();

            // 初始化
            var messageBoxService = new MessageBoxService();
            _vJoyService = new vJoyService(messageBoxService);

            // 键盘
            _hotkeyProcessor = new HotkeyProcessor(_stateService);

            // 鼠标
            _mouseService = new MouseHookService(_stateService);
            _mouseProcessor = new MouseProcessor(_mouseService, _vJoyService);

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;

            // UI切换
            SwitchUIStyle(Settings.Default.UIStyle);

            // 设置窗口大小和位置
            InitializeWindow();
        }

        #region 设置参数传递
        /// <summary>
        /// UI 切换
        /// </summary>
        /// <param name="uiId">用户指定的 UI 风格</param>
        public void SwitchUIStyle(int uiId)
        {
            if (uiId == 1)
            {
                _bottomSteeringWheel = new BottomSteeringWheel(_vJoyService);
                UIContainer.Content = _bottomSteeringWheel;
                // 移除另一个UI
                CompositionTarget.Rendering -= BottomJoystickOnRendering;
                // 监听Rendering事件，确保每一帧更新UI
                CompositionTarget.Rendering += BottomSteeringWheelOnRendering;
            }
            else
            {
                _bottomJoystick = new BottomJoystick(_vJoyService);
                SetUIYAxisOffset();
                UIContainer.Content = _bottomJoystick;
                // 移除另一个UI
                CompositionTarget.Rendering -= BottomSteeringWheelOnRendering;
                // 监听Rendering事件，确保每一帧更新UI
                CompositionTarget.Rendering += BottomJoystickOnRendering;
            }
        }

        /// <summary>
        /// 设置 UI 缩放比例
        /// </summary>
        public void SetUIScale()
        {
            _bottomSteeringWheel?.SetUIScale();
            _bottomJoystick?.SetUIScale();
        }

        /// <summary>
        /// 设置摇杆 UI 的 Y 轴偏移高度
        /// </summary>
        public void SetUIYAxisOffset()
        {
            _bottomJoystick?.SetYAxisOffset();
        }

        /// <summary>
        /// 更新XY轴对应的设置参数
        /// </summary>
        public void UpdateXYAxisParameters()
        {
            _mouseProcessor.UpdateParameters();
        }

        #endregion

        /// <summary>
        /// 初始化窗口，设置窗口大小和位置
        /// </summary>
        private void InitializeWindow()
        {
            // 获取屏幕分辨率
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // 设置窗口大小
            this.Width = screenWidth;
            this.Height = screenHeight;

            // 设置窗口位置（默认屏幕底部居中）
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = screenHeight - this.Height;
        }

        /// <summary>
        /// 渲染事件的回调方法
        /// </summary>
        private void BottomSteeringWheelOnRendering(object sender, EventArgs e)
        {
            // 调用 CircleUI 的 UpdateJoystickPosition 方法
            _bottomSteeringWheel.UpdateJoystickPosition(sender, e);
        }

        private void BottomJoystickOnRendering(object sender, EventArgs e)
        {
            // 调用 CircleUI 的 UpdateJoystickPosition 方法
            _bottomJoystick.UpdateJoystickPosition(sender, e);
        }

        /// <summary>
        /// 主窗口加载完成后，初始化快捷键服务，初始化设置窗口
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化快捷键服务
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(this);
            _hotkeyProcessor.Initialize(windowInteropHelper.Handle);

            // 设置窗口初始化
            settingWindow = new SettingWindow();
            settingWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settingWindow.Owner = this;
            settingWindow.RegistAllHotKey();
        }

        /// <summary>
        /// 初始化时注册全部快捷键
        /// </summary>
        public void RegistAllHotKey(Key[] hotKeyArray, ModifierKeys[] modifierKeyArray)
        {
            //键盘勾子，用于在快捷键抬起时重置vJoy对应按钮状态
            _keyboardHook = new KeyboardHookService(_vJoyService, hotKeyArray, modifierKeyArray, _stateService);

            // 注册 vJoy Button 对应热键
            // 使用立即计算的局部变量解决闭包变量捕获问题
            for (int index = 1; index <= 128; index++)
            {
                int buttonId = index;
                Key currentKey = hotKeyArray[index];
                if (currentKey == Key.None) continue;
                ModifierKeys currentModifiers = modifierKeyArray[index];

                _hotkeyProcessor.RegisterHotkeyWithPauseCheck(
                    id: buttonId,
                    modifiers: currentModifiers,
                    keyCode: (uint)KeyInterop.VirtualKeyFromKey(currentKey),
                    callback: () => Dispatcher.Invoke(() =>
                        _vJoyService.MapKeyToButton(buttonId)) // 这里使用局部变量
                );
            }

            // 特殊热键：暂停摇杆响应
            if (hotKeyArray[129] != Key.None)
                _hotkeyProcessor.RegisterHotkey(
                        id: 129,
                        modifiers: modifierKeyArray[129],
                        keyCode: (uint)KeyInterop.VirtualKeyFromKey(hotKeyArray[129]),
                        callback: () => Dispatcher.Invoke(() =>
                            _stateService.TogglePauseState())
                    );

            // 特殊热键：打开设置
            if (hotKeyArray[130] != Key.None)
                _hotkeyProcessor.RegisterHotkey(
                    id: 130,
                    modifiers: modifierKeyArray[130],
                    keyCode: (uint)KeyInterop.VirtualKeyFromKey(hotKeyArray[130]),
                    callback: () => Dispatcher.Invoke(() =>
                        LoadSettingWindow())
                );

            // 特殊热键：摇杆位置重置
            if (hotKeyArray[131] != Key.None)
                _hotkeyProcessor.RegisterHotkey(
                    id: 131,
                    modifiers: modifierKeyArray[131],
                    keyCode: (uint)KeyInterop.VirtualKeyFromKey(hotKeyArray[131]),
                    callback: () => Dispatcher.Invoke(() =>
                        ResetJoystickPos())
                );
        }

        /// <summary>
        /// 更新快捷键
        /// </summary>
        /// <param name="hotKeyArray"></param>
        /// <param name="modifierKeyArray"></param>
        /// <param name="changedHotKeyID"></param>
        public void UpdateHotKeys(Key[] hotKeyArray, ModifierKeys[] modifierKeyArray, HashSet<int> changedHotKeyID)
        {
            foreach (int id in changedHotKeyID)
            {
                int buttonId = id;
                _hotkeyProcessor.UnregisterHotkey(buttonId);
                Key currentKey = hotKeyArray[id];
                if (currentKey == Key.None) continue;
                ModifierKeys currentModifiers = modifierKeyArray[id];

                if (buttonId <= 128)
                {
                    _hotkeyProcessor.RegisterHotkeyWithPauseCheck(
                                        id: buttonId,
                                        modifiers: currentModifiers,
                                        keyCode: (uint)KeyInterop.VirtualKeyFromKey(currentKey),
                                        callback: () => Dispatcher.Invoke(() =>
                                            _vJoyService.MapKeyToButton(buttonId)) // 这里使用局部变量
                                    );
                }
                else if (buttonId == 129)
                    // 特殊热键：暂停摇杆响应
                    _hotkeyProcessor.RegisterHotkey(
                            id: 129,
                            modifiers: modifierKeyArray[129],
                            keyCode: (uint)KeyInterop.VirtualKeyFromKey(hotKeyArray[129]),
                            callback: () => Dispatcher.Invoke(() =>
                                _stateService.TogglePauseState())
                        );
                else if (buttonId == 130)
                    // 特殊热键：打开设置
                    _hotkeyProcessor.RegisterHotkey(
                            id: 130,
                            modifiers: modifierKeyArray[130],
                            keyCode: (uint)KeyInterop.VirtualKeyFromKey(hotKeyArray[130]),
                            callback: () => Dispatcher.Invoke(() =>
                                LoadSettingWindow())
                        );
                else if (buttonId == 131)
                    // 特殊热键：摇杆位置重置
                    _hotkeyProcessor.RegisterHotkey(
                            id: 131,
                            modifiers: modifierKeyArray[131],
                            keyCode: (uint)KeyInterop.VirtualKeyFromKey(hotKeyArray[131]),
                            callback: () => Dispatcher.Invoke(() =>
                                ResetJoystickPos())
                        );
            }
        }

        /// <summary>
        /// 显示设置窗口
        /// </summary>
        private void LoadSettingWindow()
        {
            //检测窗口状态，然后决定是否显示设置窗口
            if (!settingWindow.IsVisible)
                settingWindow.ShowDialog();
        }

        /// <summary>
        /// 摇杆位置重置
        /// </summary>
        private void ResetJoystickPos()
        {
            _vJoyService.SetJoystickX(16383);
            _vJoyService.SetJoystickY(16383);
            //将光标居中
            _mouseProcessor.ResetMousePos();
        }

        /// <summary>
        /// 窗口关闭时，注销全部快捷键、勾子以及渲染事件
        /// </summary>
        private void OnWindowClosed(object sender, EventArgs e)
        {
            for (int i = 1; i < 132; i++)
            {
                _hotkeyProcessor.UnregisterHotkey(i);
            }

            _hotkeyProcessor.Dispose();
            _mouseService.Dispose();
            _keyboardHook.Dispose();
            CompositionTarget.Rendering -= BottomSteeringWheelOnRendering;
            CompositionTarget.Rendering -= BottomJoystickOnRendering;
            // 关闭时重置摇杆位置
            _vJoyService.ResetJoystickPos();
        }
    }
}
