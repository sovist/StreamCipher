using System;
using System.Collections.Generic;
using System.ComponentModel;
using StreamCipherCoder;
using UsbHidDevice.Controls.Model;

namespace UsbHidDevice
{
    public class CommunicationProtocol : ICommunicationProtocol
    {
        /* 
            protocol blok format:    
            0. | ---------------------- blok 32 bytes --------------------------- |            
            1. |------------------------------------------------------- | payLoad |
            2. |------------------------------------|payloadLen(1 bytes)| payLoad |
            3. |---------------------|hash (4 bytes)|payloadLen(1 bytes)| payLoad |
            4. |---------------------|                coded data                  |
            5. |coder state(4 bytes) |--------------- coded data -----------------|
        */

        private readonly object _deCoderSync = new object();
        private readonly IHash _coderHash;
        private readonly IHash _decoderHash;
        private readonly ICoderWithSettings _coder;
        private readonly ICoderWithSettings _decoder;
        private byte[] _sboxesBytes;

        /// <summary>
        /// размер хеша
        /// </summary>
        private static int HashLenInBytes => 4;

        /// <summary>
        /// количество байтов указывающих количество информационных байтов в закодированом блоке
        /// </summary>
        private static int PayLoadLenInBytes => 1;

        /// <summary>
        /// –азмер состо€ни€ кодера
        /// </summary>
        private int CoderStateLen => _coder.CurrentSatate.Length;

        public ValidationKey ValidationKey { get; }

        public CipherSettingsViewModel CipherSettings { get; }
      
        public CommunicationProtocol(ICoderWithSettings coder, IHash coderHash, ICoderWithSettings decoder, IHash decoderHash)
        {
            _coder = coder;
            _decoder = decoder;
            _coderHash = coderHash;
            _decoderHash = decoderHash;

            ValidationKey = new ValidationKey();
            CipherSettings = new CipherSettingsViewModel();

            _coder.CurrentSatate = CipherSettings.InitBytesRegister;
            var sboxesArray = CipherSettings.SboxesArray;
            _coder.Sboxes = sboxesArray;
            _decoder.Sboxes = sboxesArray;
            getSboxesBytes(sboxesArray);
            CipherSettings.PropertyChanged += cipherSettingsViewModelOnPropertyChanged;
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
                    var sboxesArray = cipherSettingsViewModel.SboxesArray;
                    _coder.Sboxes = sboxesArray;
                    _decoder.Sboxes = sboxesArray;
                    getSboxesBytes(sboxesArray);
                    break;
            }
        }
        private void getSboxesBytes(IEnumerable<byte[]> sboxes)
        {
            var sboxesBytes = new List<byte>();
            foreach (var bytes in sboxes)
                sboxesBytes.AddRange(bytes);

            _sboxesBytes = sboxesBytes.ToArray();
        }

        private byte[] calcHash(IHash hash, byte[] data, byte[] coderState)
        {
            return hash.Compute(HashLenInBytes, ValidationKey.Key, _sboxesBytes, coderState, data);
        }

        #region Decoded
        public byte[] GetData(byte[] recieveBytes, out bool isAuthenticated)
        {
            byte[] coderState, codedData;
            splitArrays(recieveBytes, out coderState, CoderStateLen, out codedData);
            var dataWithHash = decoded(coderState, codedData);

            byte[] hash, dataWithLen;
            splitArrays(dataWithHash, out hash, HashLenInBytes, out dataWithLen);
            var calcHash = this.calcHash(_decoderHash, dataWithLen, coderState);

            isAuthenticated = isArraysEquivalent(hash, calcHash);
            return getDataFromDataWithLen(dataWithLen);
        }
        private static byte[] getDataFromDataWithLen(byte[] dataWithPayLoadLen)
        {
            var realpayLoadLen = dataWithPayLoadLen[0];
            var maxPayLoad = dataWithPayLoadLen.Length - PayLoadLenInBytes;
            if (realpayLoadLen > maxPayLoad)
                realpayLoadLen = (byte)(dataWithPayLoadLen.Length - 1);

            var realpayLoad = new byte[realpayLoadLen];
            Array.Copy(dataWithPayLoadLen, PayLoadLenInBytes, realpayLoad, 0, realpayLoad.Length);
            return realpayLoad;
        }
        private static bool isArraysEquivalent(byte[] arr1, byte[] arr2)
        {
            if (arr1.Length != arr2.Length)
                return false;

            for (int i = 0; i < arr1.Length; i++)           
                if (arr1[i] != arr2[i])
                    return false;

            return true;
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
        #endregion

        #region Coded
        public IEnumerable<byte[]> WrapData(byte[] data, int sendBlockSize)
        {
            var codedBlocks = new List<byte[]>();
            if (data == null || data.Length == 0 || sendBlockSize <= 0)
                return codedBlocks;

            int codedBytesCount;
            for (var offset = 0; offset < data.Length; offset += codedBytesCount)
            {
                var dataLenWithData = getDataLenWithData(data, offset, sendBlockSize, out codedBytesCount);
                var codedData = codedBlock(dataLenWithData);
                codedBlocks.Add(codedData);
            }

            CipherSettings.InitBytesRegister = _coder.CurrentSatate;
            return codedBlocks;           
        }
        private byte[] getDataLenWithData(byte[] allData, int allDataOffset, int sendBlockSize, out int getedDataCount)
        {
            var dataLen = sendBlockSize - CoderStateLen - HashLenInBytes - PayLoadLenInBytes;
            getedDataCount = allData.Length - allDataOffset > dataLen ? dataLen : allData.Length - allDataOffset;
            getedDataCount -= getedDataCount % 2;

            var payLoadWithLen = new byte[dataLen + PayLoadLenInBytes];
            payLoadWithLen[0] = (byte)getedDataCount; //add real payloadLen
            Array.Copy(allData, allDataOffset, payLoadWithLen, PayLoadLenInBytes, getedDataCount);
            return payLoadWithLen;
        }
        private byte[] codedBlock(byte[] data)
        {
            var coderState = _coder.CurrentSatate;
            var hash = calcHash(_coderHash, data, coderState);
            var hashWithData = combineArrays(hash, data);
            _coder.Coded(hashWithData);
            return combineArrays(coderState, hashWithData);
        }
        #endregion

        private static void splitArrays(byte[] array, out byte[] first, int firstLen, out byte[] second)
        {
            first = new byte[firstLen];
            Array.Copy(array, 0, first, 0, first.Length);

            var secondLen = array.Length - first.Length;
            second = new byte[secondLen];
            Array.Copy(array, first.Length, second, 0, second.Length);
        }

        private static byte[] combineArrays(byte[] firstArr, byte[] secondArr)
        {
            var result = new byte[firstArr.Length + secondArr.Length];
            Array.Copy(firstArr, 0, result, 0, firstArr.Length);
            Array.Copy(secondArr, 0, result, firstArr.Length, secondArr.Length);
            return result;
        }

        public void Dispose()
        {
            _coder.Dispose();
            _decoder.Dispose();
        }
    }
}