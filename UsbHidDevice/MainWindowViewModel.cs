﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using StreamCipherCoder;
using UsbHidDevice.Controls.Model;
using UsbHidDevice.Infrastructure;
using Timer = System.Timers.Timer;

namespace UsbHidDevice
{
    public enum DeviceStatus { Offline, Online}

    public class HidDeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        public event Action<byte[]> ReceiveBytes;

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

        public void Connect()
        {
            var isOnline = AtUsbHid.FindHidDevice(_vendorId, _productIds[SelectedProductId]);
            var currentDeviceStatus = isOnline ? DeviceStatus.Online : DeviceStatus.Offline;
            if (currentDeviceStatus == Status)
                return;

            if (currentDeviceStatus == DeviceStatus.Online)
                SendBlockSize = AtUsbHid.GetInputReportLength();

            Status = currentDeviceStatus;
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

    public class DeviceCoderDecorator : IDisposable
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

        /// <summary>
        /// кодировать отправку
        /// </summary>
        public bool CodedSend { get; set; }
        /// <summary>
        /// декодировать при приеме
        /// </summary>
        public bool DecodedRecieve { get; set; }

        public DeviceCoderDecorator(HidDeviceViewModel device)
        {
            CodedSend = true;
            DecodedRecieve = true;

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
                if (DecodedRecieve)
                {
                    _decoder.CurrentSatate = coderState;
                    _decoder.Decoded(payLoad);
                }
                return payLoad;
            }
        }

        private string payLoadToString(byte[] payLoad)
        {
            var realpayLoadLen = payLoad[0];
            var maxPayLoad = CodedBlockLen - CoderStateLen - PayLoadLenInBytes;
            if (realpayLoadLen > maxPayLoad)
                realpayLoadLen = (byte) (payLoad.Length - 1);

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

            int codedBytes;
            for (var i = 0; i < arr.Length; i += codedBytes)
                codedBlocks.Add(codedBlock(arr, i, CodedBlockLen, out codedBytes));

            return codedBlocks;
        }
        private byte[] codedBlock(byte[] arr, int offset, int blockLength, out int codedBytes)
        {
            var payloadLen = blockLength - CoderStateLen;
            var payLoad = new byte[payloadLen];

            payloadLen -= PayLoadLenInBytes;
            var copyLen = arr.Length - offset > payloadLen ? payloadLen : arr.Length - offset;

            copyLen = copyLen - copyLen%2;

            codedBytes = copyLen;
            payLoad[0] = (byte)copyLen; //add real payloadLen
            Array.Copy(arr, offset, payLoad, PayLoadLenInBytes, copyLen);

            //coded
            var startCoderStateState = _coder.CurrentSatate;
            _coder.Coded(CodedSend ? payLoad : new byte[payLoad.Length]);

            //codedBlock format - [coder state(4 bytes), payloadLen(1 bytes), payLoad]
            var codedBlock = new byte[blockLength];
            
            //add coder state
            Array.Copy(startCoderStateState, 0, codedBlock, 0, startCoderStateState.Length);
            //add payload
            Array.Copy(payLoad, 0, codedBlock, CoderStateLen, payLoad.Length);

            return codedBlock;
        }
        #endregion

        public void Dispose()
        {
            _coder.Dispose();
            _decoder.Dispose();
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public DeviceCoderDecorator DeviceCoderDecorator { get; }
        public CipherSettingsViewModel CipherSettings => DeviceCoderDecorator.CipherSettings;
        public HidDeviceViewModel Device { get; }

        public MainWindowViewModel()
        {          
            ClearSend();
            ClearRecieve();

            Device = new HidDeviceViewModel();
            DeviceCoderDecorator = new DeviceCoderDecorator(Device);
            DeviceCoderDecorator.ReceiveText += receiveText;

            _updateReceiveText = new Timer
            {
                AutoReset = true,
                Interval = UpdateReceiveTextInterval
            };
            _updateReceiveText.Elapsed += updateReceiveTextOnElapsed;
        }

        #region Receive
        private readonly Timer _updateReceiveText;
        private const int UpdateReceiveTextInterval = 500;
        private DateTime _lastReceiveTimeUtc = DateTime.UtcNow;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        public string ReceiveText => _stringBuilder.ToString();
        public string ReceiveTextSizeBytes
        {
            get
            {
                if (_stringBuilder.Length == 0)
                    return ByteSizeInfo.Get(0);

                return ByteSizeInfo.Get(_stringBuilder.Length * sizeof(char));
            }
        }
        private void updateReceiveTextOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnPropertyChanged(nameof(ReceiveTextSizeBytes));

            if ((DateTime.UtcNow - _lastReceiveTimeUtc).TotalMilliseconds > UpdateReceiveTextInterval)
            {
                OnPropertyChanged(nameof(ReceiveText));
                _updateReceiveText.Enabled = false;
            }
        }
        private void receiveText(string text)
        {
            _updateReceiveText.Enabled = true;
            _lastReceiveTimeUtc = DateTime.UtcNow;
            _stringBuilder.Append(text);
        }
        public void ClearRecieve()
        {
            _stringBuilder.Clear();
            OnPropertyChanged(nameof(ReceiveTextSizeBytes));
            OnPropertyChanged(nameof(ReceiveText));
        }
        #endregion

        #region Send
        private string _sendText;
        public string SendText
        {
            get { return _sendText; }
            set
            {
                _sendText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SendTextSizeBytes));
            }
        }

        public string SendTextSizeBytes
        {
            get
            {
                if (string.IsNullOrEmpty(SendText))
                    return ByteSizeInfo.Get(0);

                return ByteSizeInfo.Get(SendText.Length * sizeof(char));
            }
        }
        public void ClearSend()
        {
            SendText = string.Empty;
        }

        public void Send()
        {
            if(string.IsNullOrEmpty(SendText))
                return;

            DeviceCoderDecorator.Send(SendText);
        }
        public void Send(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            DeviceCoderDecorator.Send(str);
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Connected()
        {
            Device.Connect();
        }

        public void Dispose()
        {  
            Device.Dispose();   
            DeviceCoderDecorator.Dispose();
        }
    }
}