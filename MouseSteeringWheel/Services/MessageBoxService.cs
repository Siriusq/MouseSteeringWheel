﻿using System.Windows;

namespace MouseSteeringWheel.Services
{
    public class MessageBoxService
    {
        public void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
