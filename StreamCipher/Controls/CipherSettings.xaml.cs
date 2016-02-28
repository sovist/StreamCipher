using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void sboxOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var sbox = (sender as ListView)?.SelectedItem as Sbox;
            if (sbox == null)
                return;

            var graph = new Graph(sbox.ArrayBytes, sbox.SigmaValue, sbox.RValue) { FormName = "Графік залежності" };
            graph.Show();
            graph.Top = 0;
            graph.Left = 0;
        }
    }
}