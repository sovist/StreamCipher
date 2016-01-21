using System.Windows;

namespace UsbHidDevice
{
    public partial class MainWindow
    {
        public MainWindowViewModel Model { get; }
        public MainWindow()
        {
            Model = new MainWindowViewModel();
            InitializeComponent();
        }

        private void clearSendOnClick(object sender, RoutedEventArgs e)
        {
            Model?.ClearSend();
        }

        private void clearRecieveOnClick(object sender, RoutedEventArgs e)
        {
            Model?.ClearRecieve();
        }

        private void sendOnClick(object sender, RoutedEventArgs e)
        {
            Model?.Send();
        }

        private void connectOnClick(object sender, RoutedEventArgs e)
        {
            Model?.Connected();
        }
    }
}