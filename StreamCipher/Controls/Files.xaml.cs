using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using StreamCipher.Annotations;
using StreamCipher.Infrastructure;

namespace StreamCipher.Controls
{
    public partial class Files : INotifyPropertyChanged
    {
        private string _inputFileSize;
        public string InputFileSize {
            get { return _inputFileSize; }
            private set
            {
                _inputFileSize = value;
                OnPropertyChanged();
            } 
        }

        public Files()
        {
            InitializeComponent();
            if (App.EventAggregator == null)
               return;           
            App.EventAggregator.GetEvent<Events.InputFileEntropyIsCalculated>().Subscribe(inputFileEntropyIsCalculatedAction);
            App.EventAggregator.GetEvent<Events.OutputFileEntropyIsCalculated>().Subscribe(outputFileEntropyIsCalculatedAction);
        }

        private void inputFileEntropyIsCalculatedAction(string str)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                InputFileEntropy.Text = str;
            }));
        }
        private void outputFileEntropyIsCalculatedAction(string str)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                OutputFileEntropy.Text = str;
            }));
        }
        private void setInputFileOnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true) 
                return;

            InputFileName.Text = openFileDialog.FileName;
            InputFileSize = FileSizeInfo.Info(openFileDialog.FileName).ShortForm;
            App.EventAggregator.GetEvent<Events.InputFileIsChenged>().Publish(openFileDialog.FileName);
        }

        private void setOutputFileOnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new SaveFileDialog();
            if (openFileDialog.ShowDialog() != true) 
                return;

            OutputFileName.Text = openFileDialog.FileName;
            App.EventAggregator.GetEvent<Events.OutputFileIsChenged>().Publish(openFileDialog.FileName);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}