using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using StreamCipher.Controls.Model;

namespace StreamCipher
{
    public unsafe class MainWindowViewModel
    {
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
        public byte[] InitBytesShift { get; set; }
        public byte[] InitBytesRegister { get; set; }
        public List<Sbox> Sboxes { get; set; }

        private uint* _currentSatate;
        private byte* _sbox0, _sbox1, _sbox2, _sbox3;//, _sbox4, _sbox5, _sbox6, _sbox7;
        byte* _index0, _index1, _index2, _index3;
        public void Coded(Action<int> progress)
        {
            try
            {
                allocMemoryAndInitCoder();

                File.Create(OutputFileName).Close();
                using (var fileWrite = new BinaryWriter(File.Open(OutputFileName, FileMode.Open, FileAccess.Write)))
                using (var fileRead = new BinaryReader(File.Open(InputFileName, FileMode.Open, FileAccess.Read)))
                {
                    int readBufer = 3200000;
                    while (fileRead.BaseStream.Position != fileRead.BaseStream.Length)
                    {
                        byte[] temp = fileRead.ReadBytes(readBufer);
                        progress((int) (((double) fileRead.BaseStream.Position/fileRead.BaseStream.Length)*100));

                        codedBytes(ref temp);
                        fileWrite.Write(temp);
                    }
                }               
            }
            catch (IOException)
            {
            }
            finally
            {
                freeMemory();
            }
        }

        private void allocMemoryAndInitCoder()
        {
            int len = Sboxes[0].ArrayBytes.Length;
            _sbox0 = (byte*)Marshal.AllocHGlobal(len);
            _sbox1 = (byte*)Marshal.AllocHGlobal(len);
            _sbox2 = (byte*)Marshal.AllocHGlobal(len);
            _sbox3 = (byte*)Marshal.AllocHGlobal(len);
            /*_sbox4 = (byte*)Marshal.AllocHGlobal(len);
            _sbox5 = (byte*)Marshal.AllocHGlobal(len);
            _sbox6 = (byte*)Marshal.AllocHGlobal(len);
            _sbox7 = (byte*)Marshal.AllocHGlobal(len);*/

            Marshal.Copy(Sboxes[0].ArrayBytes, 0, (IntPtr)_sbox0, len);
            Marshal.Copy(Sboxes[1].ArrayBytes, 0, (IntPtr)_sbox1, len);
            Marshal.Copy(Sboxes[2].ArrayBytes, 0, (IntPtr)_sbox2, len);
            Marshal.Copy(Sboxes[3].ArrayBytes, 0, (IntPtr)_sbox3, len);
            /*Marshal.Copy(Sboxes[4].ArrayBytes, 0, (IntPtr)_sbox4, len);
            Marshal.Copy(Sboxes[5].ArrayBytes, 0, (IntPtr)_sbox5, len);
            Marshal.Copy(Sboxes[6].ArrayBytes, 0, (IntPtr)_sbox6, len);
            Marshal.Copy(Sboxes[7].ArrayBytes, 0, (IntPtr)_sbox7, len);*/

            _currentSatate = (uint*)Marshal.AllocHGlobal(sizeof(uint));
            *_currentSatate = (uint)(InitBytesRegister[3] << 24 | InitBytesRegister[2] << 16 | InitBytesRegister[1] << 8 | InitBytesRegister[0]);
            _index0 = (byte*)_currentSatate;
            _index1 = _index0 + 1;
            _index2 = _index0 + 2;
            _index3 = _index0 + 3;
        }
        private void freeMemory()
        {
            Marshal.FreeHGlobal((IntPtr)_sbox0);
            Marshal.FreeHGlobal((IntPtr)_sbox1);
            Marshal.FreeHGlobal((IntPtr)_sbox2);
            Marshal.FreeHGlobal((IntPtr)_sbox3);
            /*Marshal.FreeHGlobal((IntPtr)_sbox4);
            Marshal.FreeHGlobal((IntPtr)_sbox5);
            Marshal.FreeHGlobal((IntPtr)_sbox6);
            Marshal.FreeHGlobal((IntPtr)_sbox7);*/
            Marshal.FreeHGlobal((IntPtr)_currentSatate);
        }

        private void codedBytes(ref byte[] arr)
        {
            fixed (byte* bytes = arr)
                for (byte* bytePtr = bytes, end = bytes + arr.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
                {
                    //byte i0 = *_index0, i1 = *_index1, i2 = *_index2, i3 = *_index3;
                    //byte t1 = getIndex(i0, i1, i2, i3);
                    //byte t2 = getIndex(i3, i0, i1, i2);
                    //byte t3 = getIndex(i2, i3, i0, i1);
                    //byte t4 = getIndex(i1, i2, i3, i0);
                    //*bytePtr ^= getIndex(b1, b2, b3, b4);

                    /*//hardVersion
                    int  t0 = *_index0 ^ *_index1, 
                         t1 = *_index1 ^ *_index2, 
                         t2 = *_index2 ^ *_index3, 
                         t3 = *_index3 ^ *_index0;

                    int b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3) ^ *(_sbox4 + t0) ^ *(_sbox5 + t1) ^ *(_sbox6 + t2) ^ *(_sbox7 + t3);
                    int b1 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2) ^ *(_sbox4 + t3) ^ *(_sbox5 + t0) ^ *(_sbox6 + t1) ^ *(_sbox7 + t2);
                    int b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1) ^ *(_sbox4 + t2) ^ *(_sbox5 + t3) ^ *(_sbox6 + t0) ^ *(_sbox7 + t1);
                    int b3 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index0) ^ *(_sbox4 + t1) ^ *(_sbox5 + t2) ^ *(_sbox6 + t3) ^ *(_sbox7 + t0);

                    *bytePtr ^= (byte)(*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3) ^ *(_sbox4 + (b0 ^ b1)) ^ *(_sbox5 + (b1 ^ b2)) ^ *(_sbox6 + (b2 ^ b3)) ^ *(_sbox7 + (b3 ^ b0)));
                     */
                    /*int t0 = *_index0,// ^ *_index1,
                        t1 = *_index1,// ^ *_index2,
                        t2 = *_index2,// ^ *_index3,
                        t3 = *_index3;// ^ *_index0;
                    
                    int b0 = *(_sbox0 + t0) ^ *(_sbox1 + t1) ^ *(_sbox2 + t2) ^ *(_sbox3 + t3);
                    int b1 = *(_sbox0 + t3) ^ *(_sbox1 + t0) ^ *(_sbox2 + t1) ^ *(_sbox3 + t2);
                    int b2 = *(_sbox0 + t2) ^ *(_sbox1 + t3) ^ *(_sbox2 + t0) ^ *(_sbox3 + t1);
                    int b3 = *(_sbox0 + t1) ^ *(_sbox1 + t2) ^ *(_sbox2 + t3) ^ *(_sbox3 + t0);*/

                    //easyVersion
                    *bytePtr ^= (byte)(*(_sbox0 + (*_index0 ^ *_index1)) ^ *(_sbox1 + (*_index1 ^ *_index2)) ^ *(_sbox2 + (*_index2 ^ *_index3)) ^ *(_sbox3 + (*_index3 ^ *_index0)));

                    /*
                    int b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3);
                    int b1 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2);
                    int b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1);
                    int b3 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index0);

                    *bytePtr ^= (byte)(*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3));*/
                }
        }
        
        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte getIndex(byte index0, byte index1, byte index2, byte index3)
        {
            byte rez  = *(_sbox0 + index0);
                 rez ^= *(_sbox1 + index1);
                 rez ^= *(_sbox2 + index2);
                 rez ^= *(_sbox3 + index3);
                 rez ^= *(_sbox4 + (index0 ^ index1));
                 rez ^= *(_sbox5 + (index1 ^ index2));
                 rez ^= *(_sbox6 + (index2 ^ index3));
                 rez ^= *(_sbox7 + (index3 ^ index0));
            return rez;
        }*/
    }
}