using System.Windows;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// ErrorMessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorMessageWindow : Window
    {
        public ErrorMessageWindow(string message)
        {
            InitializeComponent();
            ErrorMessageText.Text = message;
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
