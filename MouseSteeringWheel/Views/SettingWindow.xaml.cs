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
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.language);

            //Properties.Settings.Default.vJoyDeviceId = 3;
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
        private string currLanguage;
        private void EnglishRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("en-US");
        private void ChineseSimplifiedRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("zh-CN");
        private void ChineseTraditionalRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("zh-Hant");
        private void SetLanguage(string cultureName) => currLanguage = cultureName;
        #endregion

        #region UI切换
        private int currUIStyle;
        private void SteeringWheelButtonChecked(object sender, RoutedEventArgs e) => currUIStyle = 1;
        private void JoystickButtonChecked(object sender, RoutedEventArgs e) => currUIStyle = 2;
        #endregion

        //读取设置并恢复对应UI显示文本
        private void ReadSettings()
        {
            //通用设置
            //vJoy设备ID
            vJoyDeviceID.Text = Settings.Default.vJoyDeviceId.ToString();
            //语言设置
            currLanguage = Settings.Default.language;
            switch (currLanguage)
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
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currLanguage);

            Console.WriteLine($"currLanguage: {currLanguage}, SettingLanguage:{Settings.Default.language}");

            //UI设置
            //UI样式
            currUIStyle = Settings.Default.UIStyle;
            switch (currUIStyle)
            {
                case 1:
                    SteeringWheelButton.IsChecked = true;
                    break;
                default:
                    JoystickButton.IsChecked = true;
                    break;
            }
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            var messageBoxService = new MessageBoxService();

            // 保存通用设置，需重启后生效！！！
            // 保存设备ID
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

            //保存语言            
            Settings.Default.language = currLanguage;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currLanguage);
            Console.WriteLine($"currLanguage: {currLanguage}, SettingLanguage:{Settings.Default.language}");

            //保存UI设置
            //保存UI样式
            if (currUIStyle != Settings.Default.UIStyle)
            {
                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.SwitchUIStyle(currUIStyle);
                }
                Settings.Default.UIStyle = currUIStyle;
            }

            //保存更改到Settings.settings并关闭窗口
            Settings.Default.Save();
            this.Hide();
        }
    }
}
