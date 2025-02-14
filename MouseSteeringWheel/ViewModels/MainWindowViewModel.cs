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

            // 初始化vJoy
            InitializevJoy();
        }

        // 初始化vJoy
        public void InitializevJoy()
        {
            if (!_vJoyService.InitializevJoy())
            {
                Console.WriteLine("Initialize Failed!");
            }
        }

        // 获取摇杆X轴值
        public double GetJoystickX()
        {
            return _vJoyService.GetJoystickX();
        }
    }
}
