using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using StreamCipher.Controls.Model;
using StreamCipher.Converters;

namespace StreamCipher.Controls
{
    public partial class CipherSettings
    {
        public CipherSettingsViewModel Model { get; private set; }
        public CipherSettings()
        {
            Model = new CipherSettingsViewModel();
            InitializeComponent();          
        }

        private void genereteBytesForRegisterOnClick(object sender, RoutedEventArgs e)
        {
            Model.GenereteNewBytesForRegister();
        }
        private void genereteBytesForShiftOnClick(object sender, RoutedEventArgs e)
        {
            Model.GenereteNewBytesForShift();
        }

        private void registerOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var array = getBytes(sender, e);
            if (array == null || Model.InitBytesRegister.Length != array.Length)
                return;           

            Model.InitBytesRegister = array;
        }

        private void shiftOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var array = getBytes(sender, e);
            if (array == null || Model.InitBytesShift.Length != array.Length)
                return;            
            Model.InitBytesShift = array;
        }

        private byte[] getBytes(object sender, TextChangedEventArgs e)
        {
            var text = sender as TextBox;
            if (text == null)
                return null;

            var converter = new ByteArrayToStringConverter();
            return converter.ConvertBack(text.Text, sender.GetType(), null, CultureInfo.InvariantCulture) as byte[];
        }

        private void generateNewSboxOnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}