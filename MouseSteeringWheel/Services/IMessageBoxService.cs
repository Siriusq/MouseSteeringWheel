using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseSteeringWheel.Services
{
    public interface IMessageBoxService
    {
        void ShowMessage(string message, string title);
    }
}
