using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using StreamCipher.Annotations;
using StreamCipher.Infrastructure;

namespace StreamCipher.Controls
{
    public partial class FilesView
    {
        public static DependencyProperty FilesViewModelProperty = DependencyProperty.Register("ViewModel", typeof(FilesViewModel), typeof(FilesView));
        public FilesViewModel ViewModel
        {
            get { return (FilesViewModel)GetValue(FilesViewModelProperty); } 
            set { SetValue(FilesViewModelProperty, value);}
        }

        public FilesView()
        {
            InitializeComponent();
        }

        private void setInputFileOnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true) 
                return;

            ViewModel?.SetInputFileName(openFileDialog.FileName);
        }

        private void setOutputFileOnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() != true) 
                return;

            ViewModel?.SetOutputFileName(saveFileDialog.FileName);
        }
    }

    public class FilesViewModel : INotifyPropertyChanged
    {
        private string _inputFileEntropy;
        public string InputFileEntropy
        {
            get { return _inputFileEntropy; }
            private set
            {
                _inputFileEntropy = value;
                OnPropertyChanged();
            }
        }

        private string _outputFileEntropy;
        public string OutputFileEntropy
        {
            get { return _outputFileEntropy; }
            private set
            {
                _outputFileEntropy = value;
                OnPropertyChanged();
            }
        }

        private string _inputFileName;
        public string InputFileName 
        {
            get { return _inputFileName; }
            private set
            {
                _inputFileName = value; 
                OnPropertyChanged();
            }
        }

        private string _outputFileName;
        public string OutputFileName
        {
            get { return _outputFileName; }
            private set
            {
                _outputFileName = value;
                OnPropertyChanged();
            }
        }

        private string _inputFileSize;
        public string InputFileSize
        {
            get { return _inputFileSize; }
            private set
            {
                _inputFileSize = value;
                OnPropertyChanged();
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

        public async void RecalcInputFileEntropy()
        {
            InputFileEntropy = await calc(InputFileName, progress => InputFileEntropy = progress);
        }
        public async void RecalcOutputFileEntropy()
        {
            OutputFileEntropy = await calc(OutputFileName, progress => OutputFileEntropy = progress);
        }
        private static Task<string> calc(string fileName, Action<string> progress)
        {
            return Task<string>.Factory.StartNew(() =>
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
            });
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
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}