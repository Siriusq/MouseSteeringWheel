using MouseSteeringWheel.Properties;
using MouseSteeringWheel.Services;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            //读取设置并设置UI状态
            ReadSettings();
        }

        // 点击关闭和取消按钮时隐藏设置窗口
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void Pause_HotKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Setting_HotKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Reset_Joystick_HotKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Button_Binding_HotKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        #region 语言切换
        private string _currLanguage;
        private void EnglishRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("en-US");
        private void ChineseSimplifiedRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("zh-CN");
        private void ChineseTraditionalRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("zh-Hant");
        private void SetLanguage(string cultureName) => _currLanguage = cultureName;
        #endregion

        #region UI
        private int _currUIStyle;
        private double _uiScaleFactor;
        private int _uiYAxisOffset;
        private void SteeringWheelButtonChecked(object sender, RoutedEventArgs e) => _currUIStyle = 1;
        private void JoystickButtonChecked(object sender, RoutedEventArgs e) => _currUIStyle = 2;
        #endregion

        #region X轴

        #endregion

        #region Y轴
        #endregion

        //读取设置并恢复对应UI显示文本
        private void ReadSettings()
        {
            //通用设置
            //vJoy设备ID
            vJoyDeviceID.Text = Settings.Default.vJoyDeviceId.ToString();
            //语言设置
            _currLanguage = Settings.Default.language;
            switch (_currLanguage)
            {
                case "en-US":
                    EnglishRadioButton.IsChecked = true;
                    break;
                case "zh-CN":
                    ChineseSimplifiedButton.IsChecked = true;
                    break;
                case "zh-Hant":
                    ChineseTraditionalButton.IsChecked = true;
                    break;
                default:
                    EnglishRadioButton.IsChecked = true;
                    break;
            }
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(_currLanguage);

            //UI设置
            //UI样式
            _currUIStyle = Settings.Default.UIStyle;
            switch (_currUIStyle)
            {
                case 1:
                    SteeringWheelButton.IsChecked = true;
                    break;
                default:
                    JoystickButton.IsChecked = true;
                    break;
            }

            //UI缩放系数
            _uiScaleFactor = Settings.Default.UIScaleFactor;
            UIScaleFactor.Text = _uiScaleFactor.ToString();

            // UI Y 轴偏移像素
            _uiYAxisOffset = Settings.Default.UIYAxisOffset;
            UIYAxisOffset.Text = _uiYAxisOffset.ToString();


            // X 轴设置
            XAxisEnableButton.IsChecked = Settings.Default.XAxisEnable;
            XAxisSensitivity.Text = Settings.Default.XAxisSensitivity.ToString();
            XAxisNonlinearFactor.Text = Settings.Default.XAxisNonlinear.ToString();
            XAxisDeadzone.Text = Settings.Default.XAxisDeadzone.ToString();

            // Y 轴设置
            YAxisEnableButton.IsChecked = Settings.Default.YAxisEnable;
            YAxisSensitivity.Text = Settings.Default.YAxisSensitivity.ToString();
            YAxisNonlinearFactor.Text = Settings.Default.YAxisNonlinear.ToString();
            YAxisDeadzone.Text = Settings.Default.YAxisDeadzone.ToString();
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            var messageBoxService = new MessageBoxService();

            // 保存通用设置，需重启后生效！！！
            // 保存设备ID
            try
            {
                int newvJoyDeviceID = int.Parse(vJoyDeviceID.Text);
                if (newvJoyDeviceID != Settings.Default.vJoyDeviceId)
                {
                    if (newvJoyDeviceID > 16 || newvJoyDeviceID <= 0)
                    {
                        messageBoxService.ShowMessage("vJoy 设备 ID 超出范围", "保存失败");
                        return;
                    }
                    else
                    {
                        messageBoxService.ShowMessage("vJoy 设备 ID 已修改，请重启程序以使更改生效", "程序需要重启");
                        Settings.Default.vJoyDeviceId = newvJoyDeviceID;
                    }
                }
            }
            catch
            {
                messageBoxService.ShowMessage("vJoy 设备 ID 数值无效", "vJoy 设备 ID 数值无效");
                return;
            }

            //保存语言
            if (Settings.Default.language != _currLanguage)
                Settings.Default.language = _currLanguage;


            //保存UI设置
            //保存UI样式
            if (_currUIStyle != Settings.Default.UIStyle)
            {
                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.SwitchUIStyle(_currUIStyle);
                }
                Settings.Default.UIStyle = _currUIStyle;
            }
            //保存UI缩放系数
            try
            {
                double newUIScaleFactor = double.Parse(UIScaleFactor.Text);
                if (newUIScaleFactor != Settings.Default.UIScaleFactor)
                {
                    Settings.Default.UIScaleFactor = newUIScaleFactor;
                    if (Owner is MainWindow mainWindow)
                    {
                        mainWindow.SetUIScale();
                    }
                }
            }
            catch
            {
                messageBoxService.ShowMessage("UI 缩放数值无效", "UI 缩放数值无效");
                return;
            }

            // 保存 UI Y轴偏移像素
            try
            {
                int newUIYAxisOffset = int.Parse(UIYAxisOffset.Text);
                if (newUIYAxisOffset != Settings.Default.UIYAxisOffset)
                {
                    Settings.Default.UIYAxisOffset = newUIYAxisOffset;
                    if (Owner is MainWindow mainWindow)
                    {
                        mainWindow.SetUIYAxisOffset();
                    }
                }
            }
            catch
            {
                messageBoxService.ShowMessage("UI Y轴偏移像素数值无效", "UI Y轴偏移像素数值无效");
                return;
            }


            // 保存XY轴设置
            try
            {
                Settings.Default.XAxisEnable = (bool)XAxisEnableButton.IsChecked;
                Settings.Default.XAxisSensitivity = int.Parse(XAxisSensitivity.Text);
                Settings.Default.XAxisNonlinear = float.Parse(XAxisNonlinearFactor.Text);
                Settings.Default.XAxisDeadzone = double.Parse(XAxisDeadzone.Text);
                Settings.Default.YAxisEnable = (bool)YAxisEnableButton.IsChecked;
                Settings.Default.YAxisSensitivity = int.Parse(YAxisSensitivity.Text);
                Settings.Default.YAxisNonlinear = float.Parse(YAxisNonlinearFactor.Text);
                Settings.Default.YAxisDeadzone = double.Parse(YAxisDeadzone.Text);
                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.UpdateXYAxisParameters();
                }
            }
            catch
            {
                messageBoxService.ShowMessage("XY轴数值无效", "XY轴数值无效");
                return;
            }


            //保存更改到Settings.settings并关闭窗口
            Settings.Default.Save();
            this.Hide();
        }
    }
}
