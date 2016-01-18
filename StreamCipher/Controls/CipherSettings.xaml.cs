using System.Windows;
using System.Windows.Controls;
using StreamCipher.Controls.Model;

namespace StreamCipher.Controls
{
    public partial class CipherSettings
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(CipherSettingsViewModel), typeof(CipherSettings));
        public CipherSettingsViewModel ViewModel
        {
            get { return (CipherSettingsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value);}
        }

        public CipherSettings()
        {            
            InitializeComponent();          
        }

        private void genereteBytesForRegisterOnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GenereteNewBytesForRegister();
        }

        private void registerOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if(textBox == null)
                return;

            ViewModel.SetInitBytesRegister(textBox.Text);
        }

        private void generateNewSboxOnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GenerateNewSbox();
        }
    }
}