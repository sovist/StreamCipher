using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using StreamCipher.Converters;
using StreamCipher.Infrastructure;

namespace StreamCipher.Controls.Model
{
    public class Sbox
    {
        public byte[] ArrayBytes { get; set; }
        public double SigmaValue { get; set; }
        public double RValue { get; set; }
        public override string ToString()
        {
            return $"r = {RValue.ToString("e6")}, σ = {SigmaValue.ToString("e6")}";
        }
    }
    public class CipherSettingsViewModel : INotifyPropertyChanged
    {
        public int SelectedSigmaValueIndex { get; set; }
        public int SelectedRValueIndex { get; set; }

        public List<double> RValues { get; }
        public List<double> SigmaValues { get; }
        public ObservableCollection<Sbox> Sboxes { get; }

        public byte[][] SboxesArray
        {
            get
            {
                return Sboxes.Select(_ => _.ArrayBytes).ToArray();
            }
        }

        private byte[] _initBytesRegister;
        private int _randBytesIndex;

        public byte[] InitBytesRegister
        {
            get { return _initBytesRegister; }
            set
            {
                _initBytesRegister = value;
                OnPropertyChanged(nameof(InitBytesRegister));
            }
        }

        public void SetInitBytesRegister(string str)
        {
            var converter = new ByteArrayToStringConverter();
            var array = converter.ConvertBack(str, typeof(string), null, CultureInfo.InvariantCulture) as byte[];

            if (array == null || InitBytesRegister.Length != array.Length)
                return;

            InitBytesRegister = array;
        }

        public CipherSettingsViewModel()
        {
            RValues = new List<double> {0.1, 0.01, 0.001, 0.0001, 0.00001, 0.000001};
            SigmaValues = new List<double> {6, 5, 4, 3, 0};

            Sboxes = new ObservableCollection<Sbox>(new Sbox[4]);
            for (int i = 0; i < Sboxes.Count; i++)            
                GenerateNewSbox();                          
            
            GenereteNewBytesForRegister();
        }

        public void GenereteNewBytesForRegister()
        {
            InitBytesRegister = GenerateBytesSequense.Get(4);
        }
       
        public void GenerateNewSbox()
        {
            _randBytesIndex++;
            for (int i = Sboxes.Count - 1; i > 0; i--)
                Sboxes[i] = Sboxes[i - 1];

            byte[] sbox;
            GenerateSubstitution.Generate256(out sbox, ref _randBytesIndex, SigmaValues[SelectedSigmaValueIndex], RValues[SelectedRValueIndex]);
            Sboxes[0] = new Sbox
            {
                ArrayBytes = sbox,
                RValue = Math.Abs(Stat.CorrelationCoefficient(sbox)),
                SigmaValue = Stat.Sigma256(sbox)
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}