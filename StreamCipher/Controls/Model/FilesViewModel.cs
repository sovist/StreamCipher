using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using StreamCipher.Annotations;
using StreamCipher.Infrastructure;

namespace StreamCipher.Controls.Model
{
    public class FilesViewModel : INotifyPropertyChanged
    {
        private string _inputFileEntropy;
        public string InputFileEntropy
        {
            get { return _inputFileEntropy; }
            private set
            {
                _inputFileEntropy = value;
                OnPropertyChanged(nameof(InputFileEntropy));
            }
        }

        private string _outputFileEntropy;
        public string OutputFileEntropy
        {
            get { return _outputFileEntropy; }
            private set
            {
                _outputFileEntropy = value;
                OnPropertyChanged(nameof(OutputFileEntropy));
            }
        }

        private string _inputFileName;
        public string InputFileName 
        {
            get { return _inputFileName; }
            private set
            {
                _inputFileName = value; 
                OnPropertyChanged(nameof(InputFileName));
            }
        }

        private string _outputFileName;
        public string OutputFileName
        {
            get { return _outputFileName; }
            private set
            {
                _outputFileName = value;
                OnPropertyChanged(nameof(OutputFileName));
            }
        }

        private string _inputFileSize;
        public string InputFileSize
        {
            get { return _inputFileSize; }
            private set
            {
                _inputFileSize = value;
                OnPropertyChanged(nameof(InputFileSize));
            }
        }

        public void SetOutputFileName(string fileName)
        {
            OutputFileName = fileName;
        }

        public void SetInputFileName(string fileName)
        {
            InputFileName = fileName;
            InputFileSize = FileSizeInfo.Info(fileName).ShortForm;
            RecalcInputFileEntropy();
        }

        public void RecalcInputFileEntropy()
        {
            Task.Factory.StartNew(() => InputFileEntropy = calc(InputFileName, progress => InputFileEntropy = progress));
        }
        public void RecalcOutputFileEntropy()
        {
            Task.Factory.StartNew(() => OutputFileEntropy = calc(OutputFileName, progress => OutputFileEntropy = progress));
        }

        private static string calc(string fileName, Action<string> progress)
        {
            try
            {
                return Entropy.Value(fileName, i => progress($"{i}%...")).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                // ignored
            }
            return string.Empty;
        }

        public bool FilesIsValid()
        {
            if (string.IsNullOrEmpty(InputFileName))
            {
                MessageBox.Show("Необходимо указать входной файл.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (string.IsNullOrEmpty(OutputFileName))
            {
                MessageBox.Show("Необходимо указать выходной файл.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}