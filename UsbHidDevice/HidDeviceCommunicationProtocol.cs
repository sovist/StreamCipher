using System;
using UsbHidDevice.Converters;

namespace UsbHidDevice
{
    public enum ReceiveDataAuthenticatedStatus
    {
        None = 0,
        Authenticated,
        NotAuthenticated,
    }

    public class HidDeviceCommunicationProtocol : IDisposable
    {
        public Action<string, ReceiveDataAuthenticatedStatus> ReceiveText;
        private readonly HidDeviceViewModel _device;
        public ICommunicationProtocol CommunicationProtocol { get; }

        public HidDeviceCommunicationProtocol(HidDeviceViewModel device, ICommunicationProtocol communicationProtocol)
        {
            CommunicationProtocol = communicationProtocol;
            _device = device;
            _device.ReceiveBytes += hidDeviceOnReceiveBytes;
        }

        private void hidDeviceOnReceiveBytes(byte[] bytes)
        {
            bool isAuthenticated;
            var data = CommunicationProtocol.GetData(bytes, out isAuthenticated);
            if(data == null)
                return;

            var text = BytesConverter.GetString(data);
            var status = isAuthenticated ? ReceiveDataAuthenticatedStatus.Authenticated : ReceiveDataAuthenticatedStatus.NotAuthenticated;
            ReceiveText?.Invoke(text, status);
        }

        public void Send(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var sendBytes = BytesConverter.GetBytes(text);
            var codedByteBlocks = CommunicationProtocol.WrapData(sendBytes, _device.SendBlockSize);

            foreach (var bytes in codedByteBlocks)            
                _device.Send(bytes);                                       
        }

        public void Dispose()
        {
            CommunicationProtocol.Dispose();
        }
    }
}