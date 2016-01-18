using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StreamCipherCoder
{
    public unsafe class Coder : IDisposable
    {
        private uint* _currentSatate;
        private byte* _sbox0, _sbox1, _sbox2, _sbox3;
        private byte* _index0, _index1, _index2, _index3;

        public byte[] CurrentSatate
        {
            get
            {
                return _currentSatate == null ? null : new [] { *_index3, *_index2, *_index1, *_index0};
            }
            set { allocMemoryForCurrentSatate(value); }
        }

        public byte[][] Sboxes
        {
            get
            {
                return _sbox0 == null ? null : new[]
                {
                    new[] {*_sbox0},
                    new[] {*_sbox1},
                    new[] {*_index2},
                    new[] {*_sbox3}
                };
            }
            set { allocMemoryForSboxes(value); }
        }

        private void allocMemoryForCurrentSatate(IReadOnlyList<byte> currentState)
        {
            freeMemoryForCurrentSatate();

            _currentSatate = (uint*)Marshal.AllocHGlobal(sizeof(uint));
            *_currentSatate = (uint)(currentState[0] << 24 | currentState[1] << 16 | currentState[2] << 8 | currentState[3]);

            _index0 = (byte*)_currentSatate;
            _index1 = _index0 + 1;
            _index2 = _index0 + 2;
            _index3 = _index0 + 3;
        }

        private void freeMemoryForCurrentSatate()
        {
            if(_currentSatate != null)
                Marshal.FreeHGlobal((IntPtr)_currentSatate);
        }

        private void allocMemoryForSboxes(IReadOnlyList<byte[]> sboxes)
        {
            freeMemoryForSboxes();

            var len = sboxes[0].Length;
            _sbox0 = (byte*)Marshal.AllocHGlobal(len);
            _sbox1 = (byte*)Marshal.AllocHGlobal(len);
            _sbox2 = (byte*)Marshal.AllocHGlobal(len);
            _sbox3 = (byte*)Marshal.AllocHGlobal(len);

            Marshal.Copy(sboxes[0], 0, (IntPtr)_sbox0, len);
            Marshal.Copy(sboxes[1], 0, (IntPtr)_sbox1, len);
            Marshal.Copy(sboxes[2], 0, (IntPtr)_sbox2, len);
            Marshal.Copy(sboxes[3], 0, (IntPtr)_sbox3, len);
        }
        private void freeMemoryForSboxes()
        {
            if (_sbox0 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox0);

            if (_sbox1 != null)
                Marshal.FreeHGlobal((IntPtr)_sbox1);

            if (_sbox2 != null)
                Marshal.FreeHGlobal((IntPtr)_sbox2);

            if (_sbox3 != null)
                Marshal.FreeHGlobal((IntPtr)_sbox3);
        }

        public void Coded(byte[] arr)
        {
            fixed (byte* bytes = arr)
                for (byte* bytePtr = bytes, end = bytes + arr.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
            {
                //var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3);
                //var b1 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2);
                //var b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1);
                //var b3 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index0);

                //*bytePtr ^= (byte)(*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3));

                var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3);
                var b1 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2);
                var b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1);
                var b3 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index0);

                var b10 = *(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3);
                var b11 = *(_sbox0 + b3) ^ *(_sbox1 + b0) ^ *(_sbox2 + b1) ^ *(_sbox3 + b2);
                var b12 = *(_sbox0 + b2) ^ *(_sbox1 + b3) ^ *(_sbox2 + b0) ^ *(_sbox3 + b1);
                var b13 = *(_sbox0 + b1) ^ *(_sbox1 + b2) ^ *(_sbox2 + b3) ^ *(_sbox3 + b0);

                *bytePtr ^= (byte)(*(_sbox0 + b10) ^ *(_sbox1 + b11) ^ *(_sbox2 + b12) ^ *(_sbox3 + b13));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            freeMemoryForCurrentSatate();
            freeMemoryForSboxes();
        }
    }
}
