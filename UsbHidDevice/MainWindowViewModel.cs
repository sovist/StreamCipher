using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using StreamCipherCoder;
using UsbHidDevice.Controls.Model;

namespace UsbHidDevice
{
    public enum DeviceStatus { Offline, Online}

    public class HidDeviceViewModel : INotifyPropertyChanged
    {
        public event Action<byte[]> ReceiveBytes;

        private int _outputBufferLength;
        private int _inputBufferLength;

        private readonly int _vendorId = 0x03EB;
        public string VendorId => _vendorId.ToString("X4");

        private readonly int[] _productIds = { 0, 1 };
        public List<string> ProductIds { get { return _productIds.Select(_ => _.ToString("X4")).ToList(); } }
        public int SelectedProductId { get; set; }

        private DeviceStatus _status;
        public DeviceStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public int SendBlockSize => _inputBufferLength;

        public HidDeviceViewModel()
        {
            Task.Factory.StartNew(deviceStatusMonitor);
            Task.Factory.StartNew(recieveDataMonitor);
        }

        private void deviceStatusMonitor()
        {
            while (true)
            {
                var isOnline = AtUsbHid.FindHidDevice(_vendorId, _productIds[SelectedProductId]);
                var currentDeviceStatus = isOnline ? DeviceStatus.Online : DeviceStatus.Offline;
                if (currentDeviceStatus == Status)
                    continue;

                if (currentDeviceStatus == DeviceStatus.Online)
                {
                    _outputBufferLength = AtUsbHid.GetOutputReportLength();
                    _inputBufferLength = AtUsbHid.GetInputReportLength();
                }
                Status = currentDeviceStatus;

                Thread.Sleep(400);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void recieveDataMonitor()
        {
            while (true)
            {

                if (Status != DeviceStatus.Online)
                    continue;

                var buff = new byte[_outputBufferLength];
                if (AtUsbHid.ReadData(buff))
                    Task.Factory.StartNew(() => ReceiveBytes?.Invoke(buff));

                Thread.Sleep(10);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public bool Send(byte[] bytes)
        {            
            if (Status == DeviceStatus.Online && bytes?.Length > 0 && bytes.Length == SendBlockSize)
                return AtUsbHid.WriteData(bytes);
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public CipherSettingsViewModel CipherSettings { get; }
        public HidDeviceViewModel Device { get; }

        private readonly Coder _coder = new Coder();

        private string _receiveText;
        public string ReceiveText
        {
            get { return _receiveText; }
            private set
            {
                _receiveText = value;
                OnPropertyChanged();
            }
        }
        public MainWindowViewModel()
        {          
            var atUsbHidpath = $"{Environment.CurrentDirectory}\\AtUsbHid.dll";
            File.WriteAllBytes(atUsbHidpath, Properties.Resources.AtUsbHid);
            AtUsbHid.LoadUnmanagedDll(atUsbHidpath);

            ReceiveText = string.Empty;

            CipherSettings = new CipherSettingsViewModel();
            _coder.Sboxes = CipherSettings.SboxesArray;
            _coder.CurrentSatate = CipherSettings.InitBytesRegister;
            CipherSettings.PropertyChanged += cipherSettingsViewModelOnPropertyChanged;

            Device = new HidDeviceViewModel();
            Device.ReceiveBytes += onReceiveBytes;
        }

        private void onReceiveBytes(byte[] receiveBytes)
        {
            Task.Factory.StartNew(() =>
            {
                ReceiveText += System.Text.Encoding.Default.GetString(receiveBytes) + "-\n";
            });
        }

        private void cipherSettingsViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var cipherSettingsViewModel = sender as CipherSettingsViewModel;
            if(cipherSettingsViewModel == null)
                return;

            switch (propertyChangedEventArgs.PropertyName)
            {
                case "InitBytesRegister":
                    _coder.CurrentSatate = cipherSettingsViewModel.InitBytesRegister;
                    break;

                case "Sboxes":
                    _coder.Sboxes = cipherSettingsViewModel.SboxesArray;
                    break;
            }          
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
