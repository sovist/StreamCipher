using System;
using System.Runtime.InteropServices;

namespace StreamCipherCoder
{
    public enum CoderSatateSize
    {
        Size2 = 2,
        Size3,
        Size4,
        Size5,
        Size6,
        Size7,
        Size8
    }

    public interface ICoder : IDisposable
    {
        void Coded(byte[] arr);
        void Decoded(byte[] arr);
    }

    public interface ICoderWithSettings : ICoder
    {
        byte[] CurrentSatate { get; set; }
        byte[][] Sboxes { get; set; }
    }

    public unsafe class Coder : ICoderWithSettings
    {
        private ulong* _currentSatate;
        private byte* _sbox0, _sbox1, _sbox2, _sbox3, _sbox4, _sbox5, _sbox6, _sbox7;
        private byte* _index0, _index1, _index2, _index3, _index4, _index5, _index6, _index7;

        private readonly Action<byte[]> _coder; 
        /// <summary>
        /// текущее состояние шифратора
        /// </summary>
        public byte[] CurrentSatate
        {
            get
            {
                if (_currentSatate == null)
                    return null;

                switch (CoderSatateSize)
                {
                    case CoderSatateSize.Size2:
                        return new[] {                                                             *_index1, *_index0 };

                    case CoderSatateSize.Size3:
                        return new[] {                                                   *_index2, *_index1, *_index0 };

                    case CoderSatateSize.Size4:
                        return new[] {                                         *_index3, *_index2, *_index1, *_index0 };

                    case CoderSatateSize.Size5:
                        return new[] {                               *_index4, *_index3, *_index2, *_index1, *_index0 };

                    case CoderSatateSize.Size6:
                        return new[] {                     *_index5, *_index4, *_index3, *_index2, *_index1, *_index0 };

                    case CoderSatateSize.Size7:
                        return new[] {           *_index6, *_index5, *_index4, *_index3, *_index2, *_index1, *_index0 };

                    case CoderSatateSize.Size8:
                        return new[] { *_index7, *_index6, *_index5, *_index4, *_index3, *_index2, *_index1, *_index0 };
                }
                return new byte[0];
            }
            set
            {
                if (value.Length != (int)CoderSatateSize)
                    throw new ArgumentException();

                allocMemoryForCurrentSatate(value);
            }
        }

        private byte[][] _sboxes;

        /// <summary>
        /// S-Блоки
        /// </summary>
        public byte[][] Sboxes
        {
            get { return _sboxes; }
            set
            {
                if (value.Length != (int)CoderSatateSize)
                    throw new ArgumentException();

                _sboxes = value;
                allocMemoryForSboxes(_sboxes);
            }
        }
        public CoderSatateSize CoderSatateSize { get; }

        public Coder(CoderSatateSize coderSatateSize)
        {
            CoderSatateSize = coderSatateSize;
            switch (CoderSatateSize)
            {
                case CoderSatateSize.Size2: _coder = coded2; break;
                case CoderSatateSize.Size3: _coder = coded3; break;
                case CoderSatateSize.Size4: _coder = coded4; break;
                case CoderSatateSize.Size5: _coder = coded5; break;
                case CoderSatateSize.Size6: _coder = coded6; break;
                case CoderSatateSize.Size7: _coder = coded7; break;
                case CoderSatateSize.Size8: _coder = coded8; break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void allocMemoryForCurrentSatate(byte[] currentState)
        {
            freeMemoryForCurrentSatate();

            _currentSatate = (ulong*) Marshal.AllocHGlobal(sizeof (ulong));

            *_currentSatate = 0;
            foreach (var t in currentState)
            {
                *_currentSatate <<= 8;
                *_currentSatate |= t;
            }

            //*_currentSatate = (ulong)(currentState[0] << 24 | currentState[1] << 16 | currentState[2] << 8 | currentState[3]);
            //*_currentSatate = (ulong) (currentState[0] << 40 | currentState[1] << 32 | currentState[2] << 24 | currentState[3] << 16 | currentState[4] << 8 | currentState[5]);

            _index0 = (byte*) _currentSatate;

            if (CoderSatateSize >= CoderSatateSize.Size2)
                _index1 = _index0 + 1;

            if (CoderSatateSize >= CoderSatateSize.Size3)
                _index2 = _index0 + 2;

            if (CoderSatateSize >= CoderSatateSize.Size4)
                _index3 = _index0 + 3;

            if (CoderSatateSize >= CoderSatateSize.Size5)
                _index4 = _index0 + 4;

            if (CoderSatateSize >= CoderSatateSize.Size6)
                _index5 = _index0 + 5;

            if (CoderSatateSize >= CoderSatateSize.Size7)
                _index6 = _index0 + 6;

            if (CoderSatateSize >= CoderSatateSize.Size8)
                _index7 = _index0 + 7;
        }

        private void freeMemoryForCurrentSatate()
        {
            if (_currentSatate != null)
                Marshal.FreeHGlobal((IntPtr) _currentSatate);
        }

        private void allocMemoryForSboxes(byte[][] sboxes)
        {
            freeMemoryForSboxes();

            var len = sboxes[0].Length;

            _sbox0 = (byte*) Marshal.AllocHGlobal(len);
            Marshal.Copy(sboxes[0], 0, (IntPtr) _sbox0, len);

            if (CoderSatateSize >= CoderSatateSize.Size2)
            {
                _sbox1 = (byte*) Marshal.AllocHGlobal(len);
                Marshal.Copy(sboxes[1], 0, (IntPtr) _sbox1, len);
            }

            if (CoderSatateSize >= CoderSatateSize.Size3)
            {
                _sbox2 = (byte*) Marshal.AllocHGlobal(len);
                Marshal.Copy(sboxes[2], 0, (IntPtr) _sbox2, len);
            }

            if (CoderSatateSize >= CoderSatateSize.Size4)
            {
                _sbox3 = (byte*) Marshal.AllocHGlobal(len);
                Marshal.Copy(sboxes[3], 0, (IntPtr) _sbox3, len);
            }

            if (CoderSatateSize >= CoderSatateSize.Size5)
            {
                _sbox4 = (byte*) Marshal.AllocHGlobal(len);
                Marshal.Copy(sboxes[4], 0, (IntPtr) _sbox4, len);
            }

            if (CoderSatateSize >= CoderSatateSize.Size6)
            {
                _sbox5 = (byte*) Marshal.AllocHGlobal(len);
                Marshal.Copy(sboxes[5], 0, (IntPtr) _sbox5, len);
            }

            if (CoderSatateSize >= CoderSatateSize.Size7)
            {
                _sbox6 = (byte*) Marshal.AllocHGlobal(len);
                Marshal.Copy(sboxes[6], 0, (IntPtr) _sbox6, len);
            }

            if (CoderSatateSize >= CoderSatateSize.Size8)
            {
                _sbox7 = (byte*) Marshal.AllocHGlobal(len);
                Marshal.Copy(sboxes[7], 0, (IntPtr) _sbox7, len);
            }
            /*Marshal.Copy(sboxes[0], 0, (IntPtr) _sbox0, len);
            Marshal.Copy(sboxes[1], 0, (IntPtr) _sbox1, len);
            Marshal.Copy(sboxes[2], 0, (IntPtr) _sbox2, len);
            Marshal.Copy(sboxes[3], 0, (IntPtr) _sbox3, len);
            Marshal.Copy(sboxes[4], 0, (IntPtr) _sbox4, len);
            Marshal.Copy(sboxes[5], 0, (IntPtr) _sbox5, len);*/
        }

        private void freeMemoryForSboxes()
        {
            if (_sbox0 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox0);

            if (_sbox1 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox1);

            if (_sbox2 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox2);

            if (_sbox3 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox3);

            if (_sbox4 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox4);

            if (_sbox5 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox5);

            if (_sbox6 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox6);

            if (_sbox7 != null)
                Marshal.FreeHGlobal((IntPtr) _sbox7);
        }

        private void coded8(byte[] array)
        {
            fixed (byte* bytes = array)
                for (byte* bytePtr = bytes, end = bytes + array.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
                {
                    var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3) ^ *(_sbox4 + *_index4) ^ *(_sbox5 + *_index5) ^ *(_sbox6 + *_index6) ^ *(_sbox7 + *_index7);
                    var b1 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index4) ^ *(_sbox4 + *_index5) ^ *(_sbox5 + *_index6) ^ *(_sbox6 + *_index7) ^ *(_sbox7 + *_index0);
                    var b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index4) ^ *(_sbox3 + *_index5) ^ *(_sbox4 + *_index6) ^ *(_sbox5 + *_index7) ^ *(_sbox6 + *_index0) ^ *(_sbox7 + *_index1);
                    var b3 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index4) ^ *(_sbox2 + *_index5) ^ *(_sbox3 + *_index6) ^ *(_sbox4 + *_index7) ^ *(_sbox5 + *_index0) ^ *(_sbox6 + *_index1) ^ *(_sbox7 + *_index2);
                    var b4 = *(_sbox0 + *_index4) ^ *(_sbox1 + *_index5) ^ *(_sbox2 + *_index6) ^ *(_sbox3 + *_index7) ^ *(_sbox4 + *_index0) ^ *(_sbox5 + *_index1) ^ *(_sbox6 + *_index2) ^ *(_sbox7 + *_index3);
                    var b5 = *(_sbox0 + *_index5) ^ *(_sbox1 + *_index6) ^ *(_sbox2 + *_index7) ^ *(_sbox3 + *_index0) ^ *(_sbox4 + *_index1) ^ *(_sbox5 + *_index2) ^ *(_sbox6 + *_index3) ^ *(_sbox7 + *_index4);
                    var b6 = *(_sbox0 + *_index6) ^ *(_sbox1 + *_index7) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1) ^ *(_sbox4 + *_index2) ^ *(_sbox5 + *_index3) ^ *(_sbox6 + *_index4) ^ *(_sbox7 + *_index5);
                    var b7 = *(_sbox0 + *_index7) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2) ^ *(_sbox4 + *_index3) ^ *(_sbox5 + *_index4) ^ *(_sbox6 + *_index5) ^ *(_sbox7 + *_index6);

                    *bytePtr ^= (byte) (*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3) ^ *(_sbox4 + b4) ^ *(_sbox5 + b5) ^ *(_sbox6 + b6) ^ *(_sbox7 + b7));
                }
        }
        private void coded7(byte[] array)
        {
            fixed (byte* bytes = array)
                for (byte* bytePtr = bytes, end = bytes + array.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
            {
                var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3) ^ *(_sbox4 + *_index4) ^ *(_sbox5 + *_index5) ^ *(_sbox6 + *_index6);
                var b1 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index4) ^ *(_sbox4 + *_index5) ^ *(_sbox5 + *_index6) ^ *(_sbox6 + *_index0);
                var b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index4) ^ *(_sbox3 + *_index5) ^ *(_sbox4 + *_index6) ^ *(_sbox5 + *_index0) ^ *(_sbox6 + *_index1);
                var b3 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index4) ^ *(_sbox2 + *_index5) ^ *(_sbox3 + *_index6) ^ *(_sbox4 + *_index0) ^ *(_sbox5 + *_index1) ^ *(_sbox6 + *_index2);
                var b4 = *(_sbox0 + *_index4) ^ *(_sbox1 + *_index5) ^ *(_sbox2 + *_index6) ^ *(_sbox3 + *_index0) ^ *(_sbox4 + *_index1) ^ *(_sbox5 + *_index2) ^ *(_sbox6 + *_index3);
                var b5 = *(_sbox0 + *_index5) ^ *(_sbox1 + *_index6) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1) ^ *(_sbox4 + *_index2) ^ *(_sbox5 + *_index3) ^ *(_sbox6 + *_index4);
                var b6 = *(_sbox0 + *_index6) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2) ^ *(_sbox4 + *_index3) ^ *(_sbox5 + *_index4) ^ *(_sbox6 + *_index5);

                *bytePtr ^= (byte)(*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3) ^ *(_sbox4 + b4) ^ *(_sbox5 + b5) ^ *(_sbox6 + b6));
            }
        }
        private void coded6(byte[] array)
        {
            fixed (byte* bytes = array)
                for (byte* bytePtr = bytes, end = bytes + array.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
                {
                    var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3) ^ *(_sbox4 + *_index4) ^ *(_sbox5 + *_index5);
                    var b1 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index4) ^ *(_sbox4 + *_index5) ^ *(_sbox5 + *_index0);
                    var b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index4) ^ *(_sbox3 + *_index5) ^ *(_sbox4 + *_index0) ^ *(_sbox5 + *_index1);
                    var b3 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index4) ^ *(_sbox2 + *_index5) ^ *(_sbox3 + *_index0) ^ *(_sbox4 + *_index1) ^ *(_sbox5 + *_index2);
                    var b4 = *(_sbox0 + *_index4) ^ *(_sbox1 + *_index5) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1) ^ *(_sbox4 + *_index2) ^ *(_sbox5 + *_index3);
                    var b5 = *(_sbox0 + *_index5) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2) ^ *(_sbox4 + *_index3) ^ *(_sbox5 + *_index4);

                    *bytePtr ^= (byte) (*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3) ^ *(_sbox4 + b4) ^ *(_sbox5 + b5));
                }
        }
        private void coded5(byte[] array)
        {
            fixed (byte* bytes = array)
                for (byte* bytePtr = bytes, end = bytes + array.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
            {
                var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3) ^ *(_sbox4 + *_index4);
                var b1 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index4) ^ *(_sbox4 + *_index0);
                var b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index4) ^ *(_sbox3 + *_index0) ^ *(_sbox4 + *_index1);
                var b3 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index4) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1) ^ *(_sbox4 + *_index2);
                var b4 = *(_sbox0 + *_index4) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2) ^ *(_sbox4 + *_index3);

                *bytePtr ^= (byte)(*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3) ^ *(_sbox4 + b4));
            }
        }
        private void coded4(byte[] array)
        {
            fixed (byte* bytes = array)
                for (byte* bytePtr = bytes, end = bytes + array.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
            {
                var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2) ^ *(_sbox3 + *_index3);
                var b1 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index3) ^ *(_sbox3 + *_index0);
                var b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index3) ^ *(_sbox2 + *_index0) ^ *(_sbox3 + *_index1);
                var b3 = *(_sbox0 + *_index3) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1) ^ *(_sbox3 + *_index2);

                *bytePtr ^= (byte)(*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2) ^ *(_sbox3 + b3));
            }
        }
        private void coded3(byte[] array)
        {
            fixed (byte* bytes = array)
                for (byte* bytePtr = bytes, end = bytes + array.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
            {
                var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1) ^ *(_sbox2 + *_index2);
                var b1 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index2) ^ *(_sbox2 + *_index0);
                var b2 = *(_sbox0 + *_index2) ^ *(_sbox1 + *_index0) ^ *(_sbox2 + *_index1);

                *bytePtr ^= (byte)(*(_sbox0 + b0) ^ *(_sbox1 + b1) ^ *(_sbox2 + b2));
            }
        }
        private void coded2(byte[] array)
        {
            fixed (byte* bytes = array)
                for (byte* bytePtr = bytes, end = bytes + array.Length; bytePtr < end; bytePtr++, (*_currentSatate)++)
            {
                var b0 = *(_sbox0 + *_index0) ^ *(_sbox1 + *_index1);
                var b1 = *(_sbox0 + *_index1) ^ *(_sbox1 + *_index0);

                *bytePtr ^= (byte)(*(_sbox0 + b0) ^ *(_sbox1 + b1));
            }
        }
        /// <summary>
        /// шифровать
        /// </summary>
        /// <param name="array">шифруемый масив байт</param>
        public void Coded(byte[] array)
        {
            _coder(array);
        }

        /// <summary>
        /// расшифровать
        /// </summary>
        /// <param name="array">масив байт который нужно расшифровать</param>
        public void Decoded(byte[] array)
        {
            _coder(array);
        }

        public void Dispose()
        {
            freeMemoryForCurrentSatate();
            freeMemoryForSboxes();
        }
    }
}