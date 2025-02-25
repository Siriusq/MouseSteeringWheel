using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            //设置语言单选框状态
            SetLanguageRadioButton();
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
        private void English_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetLanguage("en-US");
        }

        private void Chinese_Simplified_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetLanguage("zh-CN");

        }

        private void Chinese_Traditional_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetLanguage("zh-Hant");
        }

        private void SetLanguage(string cultureName)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
        }

        // 自动勾选当前语言对应的 RadioButton
        private void SetLanguageRadioButton()
        {
            // 获取当前 UI 语言
            string currentCulture = CultureInfo.CurrentUICulture.Name;

            switch (currentCulture)
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
        }

        #endregion
    }
}
