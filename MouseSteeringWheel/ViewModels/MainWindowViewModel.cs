using MouseSteeringWheel.Services;
using System;
using vJoyInterfaceWrap;

namespace MouseSteeringWheel.ViewModels
{
    public class MainWindowViewModel
    {
        private readonly vJoyService _vJoyService;

        public MainWindowViewModel(vJoyService vJoyService)
        {
            _vJoyService = vJoyService;
        }

        public void InitializevJoy()
        {
            if (!_vJoyService.InitializevJoy())
            {
                Console.WriteLine("Initialize Failed!");
            }
        }
    }
}
