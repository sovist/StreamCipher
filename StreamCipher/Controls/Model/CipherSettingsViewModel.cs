using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StreamCipher.Annotations;
using StreamCipher.Infrastructure;

namespace StreamCipher.Controls.Model
{
    public class Sbox
    {
        public string Name { get; set; }
    }
    public class CipherSettingsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Sbox> Sboxes { get; private set; }

        private byte[] _initBytesShift;
        private byte[] _initBytesRegister;
        public byte[] InitBytesShift
        {
            get { return _initBytesShift; }
            set
            {
                _initBytesShift = value;
                OnPropertyChanged();
            }
        }

        public byte[] InitBytesRegister
        {
            get { return _initBytesRegister; }
            set
            {
                _initBytesRegister = value;
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
            
            List<Sbox> sboxes = new List<Sbox>();
            for (int i = 0; i < 4; i++)
            {
                sboxes.Add(new Sbox{Name = i.ToString()});
            }
            Sboxes = new ObservableCollection<Sbox>(sboxes);
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
    }
}