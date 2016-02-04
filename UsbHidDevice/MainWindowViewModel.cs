using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using StreamCipherCoder;
using UsbHidDevice.Infrastructure;
using Timer = System.Timers.Timer;

namespace UsbHidDevice
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public HidDeviceCommunicationProtocol HidDeviceCommunicationProtocol { get; }
        public HidDeviceViewModel Device { get; }

        public MainWindowViewModel()
        {          
            ClearSend();
            ClearRecieve();

            Device = new HidDeviceViewModel();
            var communicationProtocol = new CommunicationProtocol(new Coder(), new Hash(), new Coder(), new Hash());
            HidDeviceCommunicationProtocol = new HidDeviceCommunicationProtocol(Device, communicationProtocol);
            HidDeviceCommunicationProtocol.ReceiveText += receiveText;

            _updateReceiveText = new Timer
            {
                AutoReset = true,
                Interval = UpdateReceiveTextInterval
            };
            _updateReceiveText.Elapsed += updateReceiveTextOnElapsed;
        }

        #region Receive
        private readonly Timer _updateReceiveText;
        private const int UpdateReceiveTextInterval = 100;
        private DateTime _lastReceiveTimeUtc = DateTime.UtcNow;
        private readonly StringBuilder _authenticatedReceiveTextStringBuilder = new StringBuilder();
        private readonly StringBuilder _notAuthenticatedReceiveTextStringBuilder = new StringBuilder();

        public string AuthenticatedReceiveText => _authenticatedReceiveTextStringBuilder.ToString();
        public string NotAuthenticatedReceiveText => _notAuthenticatedReceiveTextStringBuilder.ToString();

        public string ReceiveTextSizeBytes
        {
            get
            {
                if (_authenticatedReceiveTextStringBuilder.Length == 0)
                    return ByteSizeInfo.Get(0);

                return ByteSizeInfo.Get(_authenticatedReceiveTextStringBuilder.Length * sizeof(char));
            }
        }
        private void updateReceiveTextOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnPropertyChanged(nameof(ReceiveTextSizeBytes));

            if ((DateTime.UtcNow - _lastReceiveTimeUtc).TotalMilliseconds > UpdateReceiveTextInterval)
            {
                OnPropertyChanged(nameof(AuthenticatedReceiveText));
                OnPropertyChanged(nameof(NotAuthenticatedReceiveText));
                _updateReceiveText.Enabled = false;
            }
        }
        private void receiveText(string text, ReceiveDataAuthenticatedStatus receiveDataAuthenticatedStatus)
        {
            _updateReceiveText.Enabled = true;
            _lastReceiveTimeUtc = DateTime.UtcNow;

            switch (receiveDataAuthenticatedStatus)
            {
                case ReceiveDataAuthenticatedStatus.NotAuthenticated:
                    _notAuthenticatedReceiveTextStringBuilder.Append(text);
                    return;

                case ReceiveDataAuthenticatedStatus.Authenticated:
                    _authenticatedReceiveTextStringBuilder.Append(text);
                    return;
            }           
        }
        public void ClearRecieve()
        {
            _authenticatedReceiveTextStringBuilder.Clear();
            _notAuthenticatedReceiveTextStringBuilder.Clear();
            OnPropertyChanged(nameof(ReceiveTextSizeBytes));
            OnPropertyChanged(nameof(AuthenticatedReceiveText));
            OnPropertyChanged(nameof(NotAuthenticatedReceiveText));
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

            HidDeviceCommunicationProtocol.Send(SendText);
        }
        public void Send(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            HidDeviceCommunicationProtocol.Send(str);
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
            HidDeviceCommunicationProtocol.Dispose();
        }
    }
}