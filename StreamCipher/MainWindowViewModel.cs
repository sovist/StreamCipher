using System.Collections.Generic;
using System.IO;
using System.Threading;
using StreamCipher.Controls.Model;

namespace StreamCipher
{
    public class MainWindowViewModel
    {
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
        public byte[] InitBytesShift { get; set; }
        public byte[] InitBytesRegister { get; set; }
        public List<Sbox> Sboxes { get; set; }

        private uint _externIndexInc;
        public void Coded()
        {
            try
            {
                _externIndexInc = (uint)(InitBytesShift[0] << 24 | InitBytesShift[1] << 16 | InitBytesShift[2] << 8 | InitBytesShift[3]);
                File.Create(OutputFileName).Close();
                using (BinaryWriter fileWrite = new BinaryWriter(File.Open(OutputFileName, FileMode.Open, FileAccess.Write)))
                {
                    using (BinaryReader fileRead = new BinaryReader(File.Open(InputFileName, FileMode.Open, FileAccess.Read)))
                    {
                        int readBufer = 3200000;
                        while (fileRead.BaseStream.Position != fileRead.BaseStream.Length)
                        {
                            byte[] temp = fileRead.ReadBytes(readBufer);

                            //_substitution.ForwardSub(ref temp);
                            //_progress((int)(((double)fileRead.BaseStream.Position / fileRead.BaseStream.Length) * 100));
                            forwardSub(ref temp);
                            fileWrite.Write(temp);
                        }
                        //_progress(100);//оновити останній раз
                        Thread.Sleep(300);
                    }
                }
            }
            catch (IOException) { }
        }
        private byte getIndex(byte index0, byte index1, byte index2, byte index3)
        {
            /*byte t = _forwardSubArr[0][index3];
            t ^= _forwardSubArr[1 % _tableCount][index2];
            t ^= _forwardSubArr[2 % _tableCount][index1];
            t ^= _forwardSubArr[3 % _tableCount][index0];

            return t;*/
            return (byte)(Sboxes[0].ArrayBytes[index0 ^ InitBytesRegister[0]] ^ 
                          Sboxes[1].ArrayBytes[index1 ^ InitBytesRegister[1]] ^ 
                          Sboxes[2].ArrayBytes[index2 ^ InitBytesRegister[2]] ^ 
                          Sboxes[3].ArrayBytes[index3 ^ InitBytesRegister[3]]);
        }

        private unsafe void forwardSub(ref byte[] arr)
        {
            uint indexInc = _externIndexInc;
            byte* index = (byte*)(&indexInc);
            byte* index0 = index, index1 = index + 1, index2 = index + 2, index3 = index + 3;

            for (uint i = 0; i < arr.Length; i++, indexInc++)
            {
                arr[i] ^= getIndex(*index0, *index1, *index2, *index3);
            }

            _externIndexInc = indexInc;
        }

        public void Decoded()
        {

        }
    }
}
