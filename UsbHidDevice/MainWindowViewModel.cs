using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using System.Windows.Input;
using StreamCipherCoder;
using UsbHidDevice.Annotations;
using UsbHidDevice.Infrastructure;
using Timer = System.Timers.Timer;

namespace UsbHidDevice
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        internal class CommandAllowExecute : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private readonly Action<object> _execute;
            public CommandAllowExecute(Action<object> execute)
            {
                _execute = execute;
            }
            public void Execute(object parameter)
            {
                _execute?.Invoke(parameter);
            }
            public bool CanExecute(object parameter)
            {
                return true;
            }
        }

        public ICommand ClearRecieve { get; }
        public ICommand ClearSend { get; }
        public ICommand Send { get; }
        public ICommand UpdateDeviceStatus { get; }
        public ICommand GenerateValidationKey { get; }

        public HidDeviceCommunicationProtocol HidDeviceCommunicationProtocol { get; }
        public HidDeviceViewModel Device { get; }

        private string _sendText;
        public string SendText
        {
            get { return _sendText; }
            set
            {
                _sendText = value;
                OnPropertyChanged();
                OnPropertyChangedWithName(nameof(SendTextSizeBytes));
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

        public MainWindowViewModel()
        {          
            clearSend();
            clearRecieve();

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

            ClearRecieve = new CommandAllowExecute(_ => clearRecieve());
            ClearSend = new CommandAllowExecute(_ => clearSend());
            Send = new CommandAllowExecute(_ => send());
            UpdateDeviceStatus = new CommandAllowExecute(_ => updateDeviceStatus());
            GenerateValidationKey = new CommandAllowExecute(_ => generateValidationKey());
        }

        #region Receive
        private readonly Timer _updateReceiveText;
        private const int UpdateReceiveTextInterval = 100;
        private DateTime _lastReceiveTimeUtc = DateTime.UtcNow;
        private readonly StringBuilder _authenticatedReceiveTextStringBuilder = new StringBuilder();
        private readonly StringBuilder _notAuthenticatedReceiveTextStringBuilder = new StringBuilder();

        private void updateReceiveTextOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnPropertyChangedWithName(nameof(ReceiveTextSizeBytes));

            if ((DateTime.UtcNow - _lastReceiveTimeUtc).TotalMilliseconds > UpdateReceiveTextInterval)
            {
                OnPropertyChangedWithName(nameof(AuthenticatedReceiveText));
                OnPropertyChangedWithName(nameof(NotAuthenticatedReceiveText));
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
        private void clearRecieve()
        {
            _authenticatedReceiveTextStringBuilder.Clear();
            _notAuthenticatedReceiveTextStringBuilder.Clear();
            OnPropertyChangedWithName(nameof(ReceiveTextSizeBytes));
            OnPropertyChangedWithName(nameof(AuthenticatedReceiveText));
            OnPropertyChangedWithName(nameof(NotAuthenticatedReceiveText));
        }
        #endregion

        #region Send
        private void clearSend()
        {
            SendText = string.Empty;
        }

        private void send()
        {
            if(string.IsNullOrEmpty(SendText))
                return;

            HidDeviceCommunicationProtocol.Send(SendText);
        }
        #endregion

        private void generateValidationKey()
        {
            HidDeviceCommunicationProtocol.CommunicationProtocol.ValidationKey?.Generate();
        }

        private void updateDeviceStatus()
        {
            Device.UpdateStatus();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChangedWithName([NotNull] string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void Dispose()
        {  
            Device.Dispose();   
            HidDeviceCommunicationProtocol.Dispose();
        }
    }
}