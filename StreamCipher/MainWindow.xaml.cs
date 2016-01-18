using System.Windows;

namespace StreamCipher
{
    public partial class MainWindow 
    {
        public MainWindowViewModel Model { get; }

        public MainWindow()
        {
            Model = new MainWindowViewModel();
            InitializeComponent();           
        }
        private void codedOnClick(object sender, RoutedEventArgs e)
        {
            Model.CodedAsync();
        }
    }
}