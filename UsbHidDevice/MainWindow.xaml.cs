using System;
using System.Windows;

namespace UsbHidDevice
{
    public partial class MainWindow
    {
        public MainWindowViewModel Model { get; }

        public MainWindow()
        {
            try
            {
                Model = new MainWindowViewModel();
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void mainWindowOnClosed(object sender, EventArgs e)
        {
            Model?.Dispose();
        }
    }
}