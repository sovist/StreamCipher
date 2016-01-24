using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace UsbHidDevice
{
    public enum DeviceStatus { Offline, Online }

    public class HidDeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        public event Action<byte[]> ReceiveBytes;
        public event Action<HidDeviceViewModel> DeviceChangeStatus;

        private const int UpdateDeviceStatusInterval = 500;
        private const int ReadBufferInterval = 1;
        private const int SendInterval = 4;

        private readonly int _vendorId = 0x03EB;
        public string VendorId => _vendorId.ToString("X4");

        private readonly int[] _productIds = { 0, 1 };
        public List<string> ProductIds { get { return _productIds.Select(_ => _.ToString("X4")).ToList(); } }

        private int _selectedProductId;
        public int SelectedProductId
        {
            get { return _selectedProductId; }
            set
            {
                _selectedProductId = value;
                Status = DeviceStatus.Offline;
            }
        }

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

        public int SendBlockSize { get; private set; }

        public HidDeviceViewModel()
        {
            var atUsbHidFilepath = $"{Environment.CurrentDirectory}\\AtUsbHid.dll";
            if (!File.Exists(atUsbHidFilepath))
                File.WriteAllBytes(atUsbHidFilepath, Properties.Resources.AtUsbHid);
            AtUsbHid.LoadUnmanagedDll(atUsbHidFilepath);

            Task.Factory.StartNew(deviceStatusMonitor);
            Task.Factory.StartNew(recieveDataMonitor);
        }

        private void deviceStatusMonitor()
        {
            while (true)
            {
                Thread.Sleep(UpdateDeviceStatusInterval);

                if(Status == DeviceStatus.Online)
                    continue;

                Connect();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void recieveDataMonitor()
        {
            while (true)
            {
                Thread.Sleep(ReadBufferInterval);

                if (Status != DeviceStatus.Online)
                    continue;

                var buff = AtUsbHid.ReadBuffer();
                if (buff != null && buff.Length != 0)
                    Task.Factory.StartNew(() => ReceiveBytes?.Invoke(buff));
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public bool Send(byte[] bytes)
        {
            Thread.Sleep(SendInterval);
            if (Status == DeviceStatus.Online && bytes?.Length > 0 && bytes.Length == SendBlockSize)
                return AtUsbHid.WriteData(bytes);
            return false;
        }

        public void Connect()
        {
            var isOnline = AtUsbHid.FindHidDevice(_vendorId, _productIds[SelectedProductId]);
            var currentDeviceStatus = isOnline ? DeviceStatus.Online : DeviceStatus.Offline;
            if (currentDeviceStatus == Status)
                return;

            if (currentDeviceStatus == DeviceStatus.Online)
                SendBlockSize = AtUsbHid.GetInputReportLength();

            Status = currentDeviceStatus;
            DeviceChangeStatus?.Invoke(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            AtUsbHid.FreeLibrary();
        }
    }
}