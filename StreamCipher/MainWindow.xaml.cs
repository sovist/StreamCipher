using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using StreamCipher.Infrastructure;

namespace StreamCipher
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel Model { get; private set; }

        public MainWindow()
        {
            Model = new MainWindowViewModel();
            App.EventAggregator.GetEvent<Events.InputFileIsChenged>().Subscribe(inputFileIsChenged);
            App.EventAggregator.GetEvent<Events.OutputFileIsChenged>().Subscribe(fileName => Model.OutputFileName = fileName);
            App.EventAggregator.GetEvent<Events.InitRegisterBytesIsChenged>().Subscribe(bytes => Model.InitBytesRegister = bytes);
            App.EventAggregator.GetEvent<Events.InitShiftBytesIsChenged>().Subscribe(bytes => Model.InitBytesShift = bytes);
            App.EventAggregator.GetEvent<Events.SboxesIsChenged>().Subscribe(list => Model.Sboxes = list);

            InitializeComponent();           
        }

        private void inputFileIsChenged(string fileName)
        {
            Model.InputFileName = fileName;
            calc<Events.InputFileEntropyIsCalculated>(fileName);
        }

        private static void calc<TEventType>(string fileName) where TEventType : CompositePresentationEvent<string>, new()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    Action<int> progress = i => App.EventAggregator.GetEvent<TEventType>().Publish(string.Format("{0}%...", i));
                    var entropy = Entropy.Value(fileName, progress).ToString(CultureInfo.InvariantCulture);
                    App.EventAggregator.GetEvent<TEventType>().Publish(entropy);
                });
            }
            catch (Exception)
            {

            }
        }

        private void codedOnClick(object sender, RoutedEventArgs e)
        {
            Model.Coded();
            calc<Events.OutputFileEntropyIsCalculated>(Model.OutputFileName);
        }

        private void deCodedOnClick(object sender, RoutedEventArgs e)
        {
            Model.Decoded();
        }
    }
}