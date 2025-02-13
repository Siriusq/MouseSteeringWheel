using MouseSteeringWheel.Services;
using System;
using vJoyInterfaceWrap;

namespace MouseSteeringWheel.ViewModels
{
    public class MainWindowViewModel
    {
        private readonly IMessageBoxService _messageBoxService;
        public vJoy joyStick;

        public MainWindowViewModel(IMessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService;
        }

        public void InitializevJoy()
        {
            joyStick = new vJoy();

            if (!joyStick.vJoyEnabled())
            {
                _messageBoxService.ShowMessage("vJoy 驱动未安装或未正确初始化，请确保系统安装了 vJoy 驱动程序。", "初始化失败");
                //throw new InvalidOperationException("vJoy driver not enabled: Failed Getting vJoy attributes");
            }
            else
                _messageBoxService.ShowMessage($"Vendor: {joyStick.GetvJoyManufacturerString()}\nProduct :{joyStick.GetvJoyProductString()}\nVersion Number:{joyStick.GetvJoySerialNumberString()}\n", "初始化成功");
        }
    }
}
