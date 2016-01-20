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

        private const int UpdateDeviceStatusInterval = 400;
        private const int ReadBufferInterval = 5;
        private const int SendInterval = 5;

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

        public int SendBlockSize { get; private set; }

        public HidDeviceViewModel()
        {
            Task.Factory.StartNew(deviceStatusMonitor);
            Task.Factory.StartNew(recieveDataMonitor);
        }

        private void deviceStatusMonitor()
        {
            while (true)
            {
                Thread.Sleep(UpdateDeviceStatusInterval);

                var isOnline = AtUsbHid.FindHidDevice(_vendorId, _productIds[SelectedProductId]);
                var currentDeviceStatus = isOnline ? DeviceStatus.Online : DeviceStatus.Offline;
                if (currentDeviceStatus == Status)
                    continue;

                if (currentDeviceStatus == DeviceStatus.Online)                   
                    SendBlockSize = AtUsbHid.GetInputReportLength();
                    
                Status = currentDeviceStatus;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DeviceCoderDecorator
    {
        public Action<string> ReceiveText;
        public CipherSettingsViewModel CipherSettings { get; }

        private readonly object _deCoderSync = new object();
        private readonly Coder _decoder = new Coder();
        private readonly Coder _coder = new Coder();        
        private readonly HidDeviceViewModel _device;

        private int PayLoadLenInBytes => 1;
        private int CodedBlockLen => _device.SendBlockSize;
        private int CoderStateLen => _coder.CurrentSatate.Length;

        public DeviceCoderDecorator(HidDeviceViewModel device)
        {
            CipherSettings = new CipherSettingsViewModel();
            _decoder.Sboxes = CipherSettings.SboxesArray;
            _coder.Sboxes = CipherSettings.SboxesArray;           
            _coder.CurrentSatate = CipherSettings.InitBytesRegister;
            CipherSettings.PropertyChanged += cipherSettingsViewModelOnPropertyChanged;

            _device = device;
            _device.ReceiveBytes += hidDeviceOnReceiveBytes;
        }

        private void cipherSettingsViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var cipherSettingsViewModel = sender as CipherSettingsViewModel;
            if (cipherSettingsViewModel == null)
                return;

            switch (propertyChangedEventArgs.PropertyName)
            {
                case nameof(cipherSettingsViewModel.InitBytesRegister):
                    _coder.CurrentSatate = cipherSettingsViewModel.InitBytesRegister;
                    break;

                case nameof(cipherSettingsViewModel.Sboxes):
                    _coder.Sboxes = cipherSettingsViewModel.SboxesArray;
                    _decoder.Sboxes = cipherSettingsViewModel.SboxesArray;
                    break;
            }
        }

        #region recieve and Decoded
        private void hidDeviceOnReceiveBytes(byte[] bytes)
        {
            var coderState = new byte[CoderStateLen];
            Array.Copy(bytes, 0, coderState, 0, CoderStateLen);

            var payloadLen = bytes.Length - CoderStateLen;

            var payLoad = new byte[payloadLen];
            Array.Copy(bytes, CoderStateLen, payLoad, 0, payloadLen);

            var decodedPayload = decoded(coderState, payLoad);
            var text = payLoadToString(decodedPayload);
            ReceiveText?.Invoke(text);
        }

        private byte[] decoded(byte[] coderState, byte[] payLoad)
        {
            lock (_deCoderSync)
            {
                _decoder.CurrentSatate = coderState;
                _decoder.Decoded(payLoad);
                return payLoad;
            }
        }

        private string payLoadToString(byte[] payLoad)
        {
            var realpayLoadLen = payLoad[0];
            var maxPayLoad = CodedBlockLen - CoderStateLen - PayLoadLenInBytes;
            if (realpayLoadLen > maxPayLoad)
                return " !err! ";

            var realpayLoad = new byte[realpayLoadLen];

            Array.Copy(payLoad, PayLoadLenInBytes, realpayLoad, 0, realpayLoadLen);
            return StringToByteConverter.GetString(realpayLoad);
        }
        #endregion

        #region Coded and Send
        public void Send(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var sendBytes = StringToByteConverter.GetBytes(text);
            var codedByteBlocks = codedBytes(sendBytes);

            foreach (var bytes in codedByteBlocks)              
                    _device.Send(bytes);
                
            CipherSettings.InitBytesRegister = _coder.CurrentSatate;         
        }
        private IEnumerable<byte[]> codedBytes(byte[] arr)
        {
            var codedBlocks = new List<byte[]>();
            if (arr == null || arr.Length == 0 || CodedBlockLen <= 0)
                return codedBlocks;

            for (var i = 0; i < arr.Length; i += CodedBlockLen)
                codedBlocks.Add(codedBlock(arr, i, CodedBlockLen));

            return codedBlocks;
        }
        private byte[] codedBlock(byte[] arr, int offset, int blockLength)
        {
            var payloadLen = blockLength - CoderStateLen;
            var payLoad = new byte[payloadLen];

            payloadLen -= PayLoadLenInBytes;
            var copyLen = arr.Length - offset > payloadLen ? payloadLen : arr.Length - offset;

            payLoad[0] = (byte)copyLen; //add real payloadLen
            Array.Copy(arr, offset, payLoad, PayLoadLenInBytes, copyLen);

            //coded
            var startCoderStateState = _coder.CurrentSatate;
            _coder.Coded(payLoad);

            //codedBlock format - [coder state(4 bytes), payloadLen(1 bytes), payLoad]
            var codedBlock = new byte[blockLength];
            
            //add coder state
            Array.Copy(startCoderStateState, 0, codedBlock, 0, startCoderStateState.Length);
            //add payload
            Array.Copy(payLoad, 0, codedBlock, CoderStateLen, payLoad.Length);

            return codedBlock;
        }
        #endregion
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly DeviceCoderDecorator _deviceCoderDecorator;
        public CipherSettingsViewModel CipherSettings => _deviceCoderDecorator.CipherSettings;
        public HidDeviceViewModel Device { get; }
      
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

        private string _sendText;
        public string SendText
        {
            get { return _sendText; }
            set
            {
                _sendText = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {          
            var atUsbHidFilepath = $"{Environment.CurrentDirectory}\\AtUsbHid.dll";
            if (!File.Exists(atUsbHidFilepath))
                File.WriteAllBytes(atUsbHidFilepath, Properties.Resources.AtUsbHid);
            AtUsbHid.LoadUnmanagedDll(atUsbHidFilepath);

            ClearSend();
            ClearRecieve();

            Device = new HidDeviceViewModel();
            _deviceCoderDecorator = new DeviceCoderDecorator(Device);
            _deviceCoderDecorator.ReceiveText += receiveText;
        }

        private void receiveText(string text)
        {
            ReceiveText += text;
        }

        public void ClearSend()
        {
            SendText = string.Empty;
        }
        public void ClearRecieve()
        {
            ReceiveText = string.Empty;
        }
        public void Send()
        {
            if(string.IsNullOrEmpty(SendText))
                return;

            _deviceCoderDecorator.Send(SendText);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class StringToByteConverter
    {
        public static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length*sizeof (char)];
            var strArr = str.ToCharArray();
            Buffer.BlockCopy(strArr, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            try
            {
                var len = (int)Math.Round((double) bytes.Length/sizeof (char));
                var chars = new char[len];
                Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
                return new string(chars);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
