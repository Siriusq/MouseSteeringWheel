using MouseSteeringWheel.Views;

namespace MouseSteeringWheel.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        public void ShowMessage(string message)
        {
            var errorWindow = new ErrorMessageWindow(message);
            errorWindow.ShowDialog();
        }

        public void ShowMessage(string message, string title)
        {
            var errorWindow = new ErrorMessageWindow(message) { 
                Title = title
            };
            errorWindow.ShowDialog();
        }
    }
}
