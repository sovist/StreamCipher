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
            catch (IOException) { }
        }
        private byte getIndex(byte index0, byte index1, byte index2, byte index3)
        {
            return (byte)(Sboxes[0].ArrayBytes[index0/* ^ InitBytesRegister[0]*/] ^ 
                          Sboxes[1].ArrayBytes[index1/* ^ InitBytesRegister[1]*/] ^ 
                          Sboxes[2].ArrayBytes[index2/* ^ InitBytesRegister[2]*/] ^ 
                          Sboxes[3].ArrayBytes[index3/* ^ InitBytesRegister[3]*/] ^
                          Sboxes[4].ArrayBytes[index0 ^ index1/* ^ InitBytesRegister[0]*/] ^ 
                          Sboxes[5].ArrayBytes[index1 ^ index2/* ^ InitBytesRegister[1]*/] ^ 
                          Sboxes[6].ArrayBytes[index2 ^ index3/* ^ InitBytesRegister[2]*/] ^ 
                          Sboxes[7].ArrayBytes[index3 ^ index0/* ^ InitBytesRegister[3]*/]);
        }

        private unsafe void forwardSub(ref byte[] arr)
        {
            uint indexInc = _externIndexInc;
            byte* index = (byte*)(&indexInc);
            byte* index0 = index, index1 = index + 1, index2 = index + 2, index3 = index + 3;

            for (uint i = 0; i < arr.Length; i++, indexInc++)
            {
                var t1 = getIndex(*index0, *index1, *index2, *index3);
                var t2 = getIndex(*index3, *index0, *index1, *index2);
                var t3 = getIndex(*index2, *index3, *index0, *index1);
                var t4 = getIndex(*index1, *index2, *index3, *index0);
                arr[i] ^= (byte)(getIndex(t1, t2, t3, t4) /*^ 
                                 getIndex(*index3, *index0, *index1, *index2) ^ 
                                 getIndex(*index2, *index3, *index0, *index1) ^ 
                                 getIndex(*index1, *index2, *index3, *index0)*/);
            }

            _externIndexInc = indexInc;
        }

        public void Decoded()
        {

        }
    }
}
