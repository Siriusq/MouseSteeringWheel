using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace MouseSteeringWheel.Tests
{
    public class VJoyTest
    {
        public void JoyStickEnabledTest()
        {
            vJoy joyStick = new vJoy();
            if (!joyStick.vJoyEnabled())
            {
                throw new InvalidOperationException("vJoy driver not enabled: Failed Getting vJoy attributes");
            }
            else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n",
                joyStick.GetvJoyManufacturerString(),
                joyStick.GetvJoyProductString(),
                joyStick.GetvJoySerialNumberString());

        }
        

    }
}
