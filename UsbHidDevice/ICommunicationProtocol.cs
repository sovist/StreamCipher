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
        /// �������� ������
        /// </summary>
        byte[] GetData(byte[] recieveBytes);

        /// <summary>
        /// ������ ��� ��������
        /// </summary>
        IEnumerable<byte[]> WrapData(byte[] data, int sendBlockSize);
    }
}