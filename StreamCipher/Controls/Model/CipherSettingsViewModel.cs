using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using StreamCipher.Annotations;
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
            return string.Format("r = {0}, σ = {1}", RValue.ToString("e6"), SigmaValue.ToString("e6"));
        }
    }
    public class CipherSettingsViewModel : INotifyPropertyChanged
    {
        public List<string> RValues { get; private set; }
        public List<string> SigmaValues { get; private set; }
        public ObservableCollection<Sbox> Sboxes { get; private set; }

        private byte[] _initBytesShift;
        private byte[] _initBytesRegister;
        private int _randBytesIndex;
        public byte[] InitBytesShift
        {
            get { return _initBytesShift; }
            set
            {
                _initBytesShift = value;
                App.EventAggregator.GetEvent<Events.InitShiftBytesIsChenged>().Publish(_initBytesShift);
                OnPropertyChanged();
            }
        }

        public byte[] InitBytesRegister
        {
            get { return _initBytesRegister; }
            set
            {
                _initBytesRegister = value;
                App.EventAggregator.GetEvent<Events.InitRegisterBytesIsChenged>().Publish(_initBytesRegister);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public CipherSettingsViewModel()
        {
            if(App.EventAggregator == null)
                return;

            RValues = new List<string> {"0.1", "0.01", "0.001", "0.0001", "0.00001", "0.000001"};
            SigmaValues = new List<string> {"6", "5", "4", "3", "0"};

            Sboxes = new ObservableCollection<Sbox>(new Sbox[8]);
            for (int i = 0; i < Sboxes.Count; i++)            
                GenerateNewSbox(6, 0.1);                          
            
            GenereteNewBytesForRegister();
            GenereteNewBytesForShift();
        }

        public void GenereteNewBytesForRegister()
        {
            InitBytesRegister = GenerateBytesSequense.Get(4);
            App.EventAggregator.GetEvent<Events.InitRegisterBytesIsChenged>().Publish(_initBytesRegister);
        }
        public void GenereteNewBytesForShift()
        {
            InitBytesShift = GenerateBytesSequense.Get(4);
            App.EventAggregator.GetEvent<Events.InitShiftBytesIsChenged>().Publish(_initBytesShift);
        }
       
        public void GenerateNewSbox(double sigma, double r)
        {
            _randBytesIndex++;
            for (int i = Sboxes.Count - 1; i > 0; i--)
                Sboxes[i] = Sboxes[i - 1];

            byte[] sbox;
            GenerateSubstitution.Generate256(out sbox, ref _randBytesIndex, sigma, r);
            Sboxes[0] = new Sbox
            {
                ArrayBytes = sbox,
                RValue = Math.Abs(Stat.CorrelationCoefficient(sbox)),
                SigmaValue = Stat.Sigma256(sbox)
            };
            App.EventAggregator.GetEvent<Events.SboxesIsChenged>().Publish(Sboxes.ToList());
        }
    }
}