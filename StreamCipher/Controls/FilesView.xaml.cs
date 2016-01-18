using System.Windows;
using Microsoft.Win32;
using StreamCipher.Controls.Model;

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
}