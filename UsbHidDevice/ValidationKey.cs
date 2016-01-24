using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UsbHidDevice.Infrastructure;

namespace UsbHidDevice
{
    public class ValidationKey : INotifyPropertyChanged
    {
        private byte[] _key = new byte[0];
        public byte[] Key
        {
            get { return _key; }
            set
            {
                if(value == null)
                    return;

                for (var i = 0; i < value.Length && i < _key.Length; i++)                
                    _key[i] = value[i];
                
                OnPropertyChanged();
            }
        }

        private int _selectedLengthIndex;
        public int SelectedLengthIndex
        {
            get { return _selectedLengthIndex; }
            set
            {
                _selectedLengthIndex = value;
                _key = new byte[Lengths[SelectedLengthIndex]];
                Generate();

                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(Key));
            }
        }

        public int[] Lengths => Enumerable.Range(8, 25).ToArray();

        public ValidationKey()
        {
            SelectedLengthIndex = Lengths.Length - 1;
            Key = new byte[]
            {
                0xE2, 0x9A, 0x4F, 0x32, 0x4E, 0x91, 0x5C, 0x2C, 0xD5, 0x26, 0x59, 0xB0, 0xC7, 0x8B, 0xA5, 0x5E,
                0x99, 0x02, 0xB1, 0xD6, 0x16, 0xF8, 0xE8, 0x8E, 0x92, 0x12, 0xE4, 0x2B, 0x6E, 0x7A, 0xEB, 0x2D
            };
        }

        /// <summary>
        /// Генерировать новый ключ
        /// </summary>
        public void Generate()
        {
            Key = GenerateBytesSequense.Get(Lengths[SelectedLengthIndex]);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}