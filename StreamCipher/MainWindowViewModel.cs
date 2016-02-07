using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using StreamCipher.Controls.Model;
using StreamCipherCoder;

namespace StreamCipher
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _workTime;
        public string WorkTime
        {
            get { return _workTime; }
            set
            {
                _workTime = value;
                OnPropertyChangedWithName(nameof(WorkTime));
            }
        }

        private DateTime _startDateTime;
        private int _progress;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                WorkTime = (DateTime.UtcNow - _startDateTime).ToString();
                OnPropertyChangedWithName(nameof(Progress));
            }
        }

        private Visibility _progressVisibility;
        public Visibility ProgressVisibility
        {
            get { return _progressVisibility; }
            set
            {
                _progressVisibility = value;
                OnPropertyChangedWithName(nameof(ProgressVisibility));
            }
        }

        public CipherSettingsViewModel CipherSettingsViewModel { get; }
        public FilesViewModel FilesViewModel { get; }

        public MainWindowViewModel()
        {
            FilesViewModel = new FilesViewModel();
            CipherSettingsViewModel = new CipherSettingsViewModel();
            ProgressVisibility = Visibility.Collapsed;
        }
      
        public void CodedAsync()
        {
            Task.Factory.StartNew(Coded);
        }
                
        public void Coded()
        {
            if (!FilesViewModel.FilesIsValid())
                return;

            _startDateTime = DateTime.UtcNow;
            Progress = 0;
            ProgressVisibility = Visibility.Visible;

            var coder = new Coder
            {
                CurrentSatate = CipherSettingsViewModel.InitBytesRegister,
                Sboxes = CipherSettingsViewModel.SboxesArray
            };
            using (coder)
            {
                FileCoder.Coded(coder, FilesViewModel.InputFileName, FilesViewModel.OutputFileName, _ => Progress = _);
                //CipherSettingsViewModel.InitBytesRegister = coder.CurrentSatate;
            }

            ProgressVisibility = Visibility.Collapsed;
            FilesViewModel.RecalcOutputFileEntropy();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChangedWithName(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}