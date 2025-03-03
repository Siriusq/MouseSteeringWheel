using MouseSteeringWheel.Properties;
using MouseSteeringWheel.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        private MessageBoxService _messageBoxService;
        public SettingWindow()
        {
            InitializeComponent();
            //读取设置并设置UI状态
            ReadSettings();
            _messageBoxService = new MessageBoxService();
        }

        /// <summary>
        /// 由主窗口注册全部快捷键
        /// </summary>
        public void RegistAllHotKey()
        {
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.RegistAllHotKey(_hotKeyArray, _modifierKeyArray);
            }
        }

        /// <summary>
        /// 点击关闭按钮时，隐藏设置窗口，并取消所做的更改
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ReadSettings();
            this.Hide();
            e.Cancel = true;
        }

        #region 语言切换
        private string _currLanguage;
        private void EnglishRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("en-US");
        private void ChineseSimplifiedRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("zh-CN");
        private void ChineseTraditionalRadioButtonChecked(object sender, RoutedEventArgs e) => SetLanguage("zh-Hant");
        private void SetLanguage(string cultureName) => _currLanguage = cultureName;
        #endregion

        #region UI 设置
        private int _currUIStyle;
        private double _uiScaleFactor;
        private int _uiYAxisOffset;
        private void SteeringWheelButtonChecked(object sender, RoutedEventArgs e) => _currUIStyle = 1;
        private void JoystickButtonChecked(object sender, RoutedEventArgs e) => _currUIStyle = 2;
        #endregion

        #region 快捷键处理
        private readonly HashSet<Key> _pressedKeys = new HashSet<Key>();
        private int _currKeyID = 0;
        private Key _currHotKey;
        private ModifierKeys _currHotKeyModifiers;
        private Key[] _hotKeyArray = new Key[132];
        private ModifierKeys[] _modifierKeyArray = new ModifierKeys[132];
        private HashSet<int> _changedHotKeyID = new HashSet<int>();

        /// <summary>
        /// 快捷键预处理，检测按下的按钮，并显示预览
        /// </summary>
        private void HotKeyTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //判断快捷键对应的文本框编号
            var textBox = (TextBox)sender;
            if (textBox.Name == "ButtonHotKeyTextBox")
            {
                //判断ID是否合法
                try
                {
                    _currKeyID = int.Parse(vJoyButtonIDTextBox.Text);
                    if (_currKeyID <= 0 || _currKeyID > 128)
                    {
                        _messageBoxService.ShowMessage(Properties.Resources.vJoyButtonIDInvalidContent, Properties.Resources.vJoyButtonIDInvalidTitle);
                        e.Handled = true;
                        return;
                    }
                }
                catch
                {
                    _messageBoxService.ShowMessage(Properties.Resources.vJoyButtonIDInvalidContent, Properties.Resources.vJoyButtonIDInvalidTitle);
                    e.Handled = true;
                    return;
                }
            }
            else if (textBox.Name == "PauseHotKeyTextBox") _currKeyID = 129;
            else if (textBox.Name == "SettingHotKeyTextBox") _currKeyID = 130;
            else if (textBox.Name == "ResetJoystickHotKeyTextBox") _currKeyID = 131;
            else
            {
                _currKeyID = 0;
                e.Handled = true;
                return;
            }

            try
            {
                //判断Key是否合法
                if (IsForbiddenKey(e.Key))
                {
                    // 如果按键非法则阻止按键事件继续传播
                    e.Handled = true;
                    return;
                }
                //将用户输入的键位赋值到变量中
                _currHotKey = e.Key;
                _currHotKeyModifiers = Keyboard.Modifiers;
                textBox.Text = $"{_currHotKeyModifiers} + {_currHotKey}";
                _pressedKeys.Add(_currHotKey);
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowMessage(ex.ToString(), "Error!");
                return;
            }
        }

        /// <summary>
        /// 检测按键是否合法，排除修饰键
        /// </summary>
        private bool IsForbiddenKey(Key key)
        {
            return Enum.IsDefined(typeof(ForbiddenKeys), key.ToString());
        }

        /// <summary>
        /// 非法按键枚举
        /// </summary>
        private enum ForbiddenKeys
        {
            Capital, CapsLock,
            Enter,
            LeftAlt, LeftCtrl, LeftShift, LWin,
            ImeProcessed,
            NumLock, None,
            RightAlt, RightCtrl, RightShift, RWin,
            System,
            Tab,
            VolumeDown, VolumeMute, VolumeUp
        };

        /// <summary>
        /// 按键抬起时，快捷键预存储
        /// </summary>
        private void HotKeyTextBoxPreviewKeyUp(object sender, KeyEventArgs e)
        {
            // 判断 Key 是否合法
            if (IsForbiddenKey(e.Key))
            {
                // 如果按键非法则阻止按键事件继续传播
                e.Handled = true;
                return;
            }
            _pressedKeys.Remove(e.Key);

            if (_pressedKeys.Count == 0)
            {
                _hotKeyArray[_currKeyID] = _currHotKey;
                _modifierKeyArray[_currKeyID] = _currHotKeyModifiers;
                _changedHotKeyID.Add(_currKeyID);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 按键 ID 两侧的加减按钮点击事件处理
        /// </summary>
        private void AddIDButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int currID = int.Parse(vJoyButtonIDTextBox.Text);
                if (currID >= 128) return;
                currID++;
                vJoyButtonIDTextBox.Text = currID.ToString();
            }
            catch
            {
                _messageBoxService.ShowMessage(Properties.Resources.vJoyButtonIDInvalidContent, Properties.Resources.vJoyButtonIDInvalidTitle);
            }
        }

        private void SubtractIDButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int currID = int.Parse(vJoyButtonIDTextBox.Text);
                if (currID <= 1) return;
                currID--;
                vJoyButtonIDTextBox.Text = currID.ToString();
            }
            catch
            {
                _messageBoxService.ShowMessage(Properties.Resources.vJoyButtonIDInvalidContent, Properties.Resources.vJoyButtonIDInvalidTitle);
            }
        }

        /// <summary>
        /// 直接通过文本框输入的按键 ID 检测
        /// </summary>
        private void vJoyButtonIDTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int currID = int.Parse(vJoyButtonIDTextBox.Text);
                if (currID <= 0 || currID > 128)
                {
                    _messageBoxService.ShowMessage(Properties.Resources.vJoyButtonIDInvalidContent, Properties.Resources.vJoyButtonIDInvalidTitle);
                    vJoyButtonIDTextBox.Text = "1";
                    return;
                }
                TextBox buttonHotKeyTextBox = ButtonHotKeyTextBox;
                if (buttonHotKeyTextBox != null)
                    buttonHotKeyTextBox.Text = $"{_modifierKeyArray[currID]} + {_hotKeyArray[currID]}";
            }
            catch
            {
                _messageBoxService.ShowMessage(Properties.Resources.vJoyButtonIDInvalidContent, Properties.Resources.vJoyButtonIDInvalidTitle);
            }
        }

        #endregion


        /// <summary>
        /// 读取设置并恢复对应 UI 显示文本
        /// </summary>
        private void ReadSettings()
        {
            // 通用设置
            // vJoy设备ID
            vJoyDeviceID.Text = Settings.Default.vJoyDeviceId.ToString();
            // 语言设置
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

            // UI 设置
            // UI 样式
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

            // UI 缩放系数
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


            // 快捷键数组 <-> 字符串转换
            // 读取热键数组
            string hotKeyStr = Settings.Default.HotKeyArrayString;
            if (!string.IsNullOrEmpty(hotKeyStr))
            {
                string[] parts = hotKeyStr.Split(';');
                for (int i = 0; i < Math.Min(_hotKeyArray.Length, parts.Length); i++)
                {
                    if (Enum.TryParse(parts[i], out Key parsedKey))
                    {
                        _hotKeyArray[i] = parsedKey;
                    }
                    else
                    {
                        // 处理解析失败，设为默认值
                        _hotKeyArray[i] = Key.None;
                    }
                }
            }

            // 读取修饰键数组（类似逻辑）
            string modifierKeyStr = Settings.Default.ModifierKeyArrayString;
            if (!string.IsNullOrEmpty(modifierKeyStr))
            {
                string[] parts = modifierKeyStr.Split(';');
                for (int i = 0; i < Math.Min(_modifierKeyArray.Length, parts.Length); i++)
                {
                    if (Enum.TryParse(parts[i], out ModifierKeys parsedModifier))
                    {
                        _modifierKeyArray[i] = parsedModifier;
                    }
                    else
                    {
                        _modifierKeyArray[i] = ModifierKeys.None;
                    }
                }
            }

            // 输出快捷键数组
            //for (int i = 1; i < 132; i++)
            //{
            //    Console.WriteLine($"{i},{_hotKeyArray[i]},{_modifierKeyArray[i]}");
            //}

            // 将快捷键还原至对应文本框
            PauseHotKeyTextBox.Text = $"{_modifierKeyArray[129]} + {_hotKeyArray[129]}";
            SettingHotKeyTextBox.Text = $"{_modifierKeyArray[130]} + {_hotKeyArray[130]}";
            ResetJoystickHotKeyTextBox.Text = $"{_modifierKeyArray[131]} + {_hotKeyArray[131]}";
            ButtonHotKeyTextBox.Text = $"{_modifierKeyArray[1]} + {_hotKeyArray[1]}";
            vJoyButtonIDTextBox.Text = "1";
        }

        /// <summary>
        /// 点击保存按钮后保存设置
        /// </summary>
        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            // 保存通用设置，需重启后生效
            // 保存设备 ID
            try
            {
                int newvJoyDeviceID = int.Parse(vJoyDeviceID.Text);
                if (newvJoyDeviceID != Settings.Default.vJoyDeviceId)
                {
                    if (newvJoyDeviceID > 16 || newvJoyDeviceID <= 0)
                    {
                        _messageBoxService.ShowMessage(Properties.Resources.vJoyDeviceIDInvalidContent, Properties.Resources.vJoyDeviceIDInvalidTitle);
                        return;
                    }
                    else
                    {
                        _messageBoxService.ShowMessage(Properties.Resources.vJoyDeviceIDChangedContent, Properties.Resources.vJoyDeviceIDChangedTitle);
                        Settings.Default.vJoyDeviceId = newvJoyDeviceID;
                    }
                }
            }
            catch
            {
                _messageBoxService.ShowMessage(Properties.Resources.vJoyDeviceIDInvalidContent, Properties.Resources.vJoyDeviceIDInvalidTitle);
                return;
            }

            // 保存语言设置
            if (Settings.Default.language != _currLanguage)
                Settings.Default.language = _currLanguage;


            // 保存 UI 设置
            // 保存 UI 样式
            if (_currUIStyle != Settings.Default.UIStyle)
            {
                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.SwitchUIStyle(_currUIStyle);
                }
                Settings.Default.UIStyle = _currUIStyle;
            }
            // 保存 UI 缩放系数
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
                _messageBoxService.ShowMessage(Properties.Resources.UIScaleFactorInvalidContent, Properties.Resources.UIScaleFactorInvalidTitle);
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
                _messageBoxService.ShowMessage(Properties.Resources.UIYAxisOffsetInvalidContent, Properties.Resources.UIYAxisOffsetInvalidTitle);
                return;
            }


            // 保存 XY 轴相关设置
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
                _messageBoxService.ShowMessage(Properties.Resources.XYAxisParameterInvalidContent, Properties.Resources.XYAxisParameterInvalidTitle);
                return;
            }

            // 输出快捷键数组
            //for (int i = 1; i < 132; i++)
            //{
            //    Console.WriteLine($"{i},{_hotKeyArray[i]},{_modifierKeyArray[i]}");
            //}

            // 更新修改过的快捷键
            if (Owner is MainWindow _mainWindow)
            {
                _mainWindow.UpdateHotKeys(_hotKeyArray, _modifierKeyArray, _changedHotKeyID);
            }

            // 保存快捷键设置
            // 将数组转换为以分号分隔的字符串
            Settings.Default.HotKeyArrayString = string.Join(";", _hotKeyArray.Select(k => k.ToString()));
            Settings.Default.ModifierKeyArrayString = string.Join(";", _modifierKeyArray.Select(m => m.ToString()));

            //Console.WriteLine(Settings.Default.HotKeyArrayString);
            //Console.WriteLine(Settings.Default.ModifierKeyArrayString);

            // 保存更改到Settings.settings并关闭窗口
            Settings.Default.Save();
            this.Hide();
        }

        /// <summary>
        /// 点击取消按钮后，隐藏设置窗口并取消所做的更改
        /// </summary>
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            ReadSettings();
            this.Hide();
        }
    }
}
