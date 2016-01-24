using System;
using System.Collections.Generic;
using UsbHidDevice.Controls.Model;

namespace UsbHidDevice
{
    public interface ICommunicationProtocol : IDisposable
    {
        ValidationKey ValidationKey { get; }
        CipherSettingsViewModel CipherSettings { get; }

        /// <summary>
        /// получить данные
        /// </summary>
        byte[] GetData(byte[] recieveBytes);

        /// <summary>
        /// данные для отправки
        /// </summary>
        IEnumerable<byte[]> WrapData(byte[] data, int sendBlockSize);
    }
}