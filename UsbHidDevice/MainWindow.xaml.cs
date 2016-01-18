using System;
using System.IO;

namespace UsbHidDevice
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            var atUsbHidpath = $"{Environment.CurrentDirectory}\\AtUsbHid.dll";
            File.WriteAllBytes(atUsbHidpath, Properties.Resources.AtUsbHid);
            AtUsbHid.LoadUnmanagedDll(atUsbHidpath);

            InitializeComponent();
        }
    }
}
