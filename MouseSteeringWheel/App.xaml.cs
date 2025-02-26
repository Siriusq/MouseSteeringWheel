using MouseSteeringWheel.Properties;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace MouseSteeringWheel
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //语言设置
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.language);

        }

    }
}
