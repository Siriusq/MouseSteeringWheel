using MouseSteeringWheel.Tests;
using MouseSteeringWheel.Services;
using MouseSteeringWheel.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MouseSteeringWheel.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var messageBoxService = new MessageBoxService();
            var vJoyService = new vJoyService(messageBoxService);
            var viewModel = new MainWindowViewModel(vJoyService);

            this.DataContext = viewModel;

            //初始化vJoy
            viewModel.InitializevJoy();
        }
    }
}
