using System;
using UsbHidDevice.Converters;

namespace UsbHidDevice
{
    public class HidDeviceCommunicationProtocol : IDisposable
    {
        public Action<string> ReceiveText;
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
            var data = CommunicationProtocol.GetData(bytes);
            if(data == null)
                return;

            var text = BytesConverter.GetString(data);
            ReceiveText?.Invoke(text);
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