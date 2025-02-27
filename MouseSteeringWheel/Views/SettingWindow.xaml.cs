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

        // 点击关闭和取消按钮时隐藏设置窗口
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
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

        #region UI
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

        // 快捷键预处理
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
                        _messageBoxService.ShowMessage("vJoy 按键 ID 超出范围", "vJoy 按键 ID 超出范围");
                        e.Handled = true;
                        return;
                    }
                }
                catch
                {
                    _messageBoxService.ShowMessage("vJoy 按键 ID 非法", "vJoy 按键 ID 非法");
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

        //非法Key枚举
        private bool IsForbiddenKey(Key key)
        {
            return Enum.IsDefined(typeof(ForbiddenKeys), key.ToString());
        }
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

        // 快捷键预存储
        private void HotKeyTextBoxPreviewKeyUp(object sender, KeyEventArgs e)
        {
            //判断Key是否合法
            if (IsForbiddenKey(e.Key))
            {
                // 如果按键非法则阻止按键事件继续传播
                e.Handled = true;
                return;
            }
            _pressedKeys.Remove(e.Key);
            Console.WriteLine(_pressedKeys.Count);
            if (_pressedKeys.Count == 0)
            {
                _hotKeyArray[_currKeyID] = _currHotKey;
                _modifierKeyArray[_currKeyID] = _currHotKeyModifiers;
                Console.WriteLine($"{_currHotKeyModifiers} + {_currHotKey}");

            }
            e.Handled = true;
        }

        // ID 加减按钮处理
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
                _messageBoxService.ShowMessage("ID 值非法", "ID 值非法");
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
                _messageBoxService.ShowMessage("ID 值非法", "ID 值非法");
            }
        }

        private void vJoyButtonIDTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int currID = int.Parse(vJoyButtonIDTextBox.Text);
                if (currID <= 0 || currID > 128)
                {
                    _messageBoxService.ShowMessage("ID 值超出范围", "ID 值超出范围");
                    vJoyButtonIDTextBox.Text = "1";
                    return;
                }
                TextBox buttonHotKeyTextBox = ButtonHotKeyTextBox;
                if (buttonHotKeyTextBox != null)
                    buttonHotKeyTextBox.Text = $"{_modifierKeyArray[currID]} + {_hotKeyArray[currID]}";
            }
            catch
            {
                _messageBoxService.ShowMessage("ID 值非法", "ID 值非法");
            }
        }

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


            // 快捷键数组 <->字符串转换
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
                        // 处理解析失败（例如设为默认值）
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
            for (int i = 1; i < 132; i++)
            {
                Console.WriteLine($"{i},{_hotKeyArray[i]},{_modifierKeyArray[i]}");
            }

            // 将快捷键还原至对应文本框
            PauseHotKeyTextBox.Text = $"{_modifierKeyArray[129]} + {_hotKeyArray[129]}";
            SettingHotKeyTextBox.Text = $"{_modifierKeyArray[130]} + {_hotKeyArray[130]}";
            ResetJoystickHotKeyTextBox.Text = $"{_modifierKeyArray[131]} + {_hotKeyArray[131]}";
            ButtonHotKeyTextBox.Text = $"{_modifierKeyArray[1]} + {_hotKeyArray[1]}";
            vJoyButtonIDTextBox.Text = "1";
        }

        // 保存设置
        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            // 保存通用设置，需重启后生效！！！
            // 保存设备ID
            try
            {
                int newvJoyDeviceID = int.Parse(vJoyDeviceID.Text);
                if (newvJoyDeviceID != Settings.Default.vJoyDeviceId)
                {
                    if (newvJoyDeviceID > 16 || newvJoyDeviceID <= 0)
                    {
                        _messageBoxService.ShowMessage("vJoy 设备 ID 超出范围", "保存失败");
                        return;
                    }
                    else
                    {
                        _messageBoxService.ShowMessage("vJoy 设备 ID 已修改，请重启程序以使更改生效", "程序需要重启");
                        Settings.Default.vJoyDeviceId = newvJoyDeviceID;
                    }
                }
            }
            catch
            {
                _messageBoxService.ShowMessage("vJoy 设备 ID 数值无效", "vJoy 设备 ID 数值无效");
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
                _messageBoxService.ShowMessage("UI 缩放数值无效", "UI 缩放数值无效");
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
                _messageBoxService.ShowMessage("UI Y轴偏移像素数值无效", "UI Y轴偏移像素数值无效");
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
                _messageBoxService.ShowMessage("XY轴数值无效", "XY轴数值无效");
                return;
            }


            // 输出快捷键数组
            for (int i = 1; i < 132; i++)
            {
                Console.WriteLine($"{i},{_hotKeyArray[i]},{_modifierKeyArray[i]}");
            }


            // 保存快捷键设置
            // 将数组转换为以分号分隔的字符串
            Settings.Default.HotKeyArrayString = string.Join(";", _hotKeyArray.Select(k => k.ToString()));
            Settings.Default.ModifierKeyArrayString = string.Join(";", _modifierKeyArray.Select(m => m.ToString()));


            //保存更改到Settings.settings并关闭窗口
            Settings.Default.Save();
            this.Hide();
        }


    }
}
