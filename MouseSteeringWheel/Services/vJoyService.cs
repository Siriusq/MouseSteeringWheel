using System;
using vJoyInterfaceWrap;

namespace MouseSteeringWheel.Services
{
    public class vJoyService
    {
        private readonly IMessageBoxService _messageBoxService;
        public vJoy _joyStick;

        public vJoyService(IMessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService;
        }

        // 初始化vJoy并返回是否成功
        public bool InitializevJoy()
        {
            _joyStick = new vJoy();

            if (!_joyStick.vJoyEnabled())
            {
                _messageBoxService.ShowMessage("vJoy 驱动未安装或未正确初始化，请确保系统安装了 vJoy 驱动程序。", "初始化失败");
                return false;
            }

            // 设备初始化成功
            Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n",
                _joyStick.GetvJoyManufacturerString(),
                _joyStick.GetvJoyProductString(),
                _joyStick.GetvJoySerialNumberString());
            return true;
        }
    }
}
