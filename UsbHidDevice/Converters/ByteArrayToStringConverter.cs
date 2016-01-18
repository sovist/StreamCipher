using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace UsbHidDevice.Converters
{
    internal class ByteArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value as byte[];
            if (bytes == null)
                return string.Empty;

            if (bytes.Length == 0)
                return string.Empty;

            var sb = new StringBuilder(2 * bytes.Length); 
            sb.AppendFormat("{0:X2}", bytes[0]);
            for (int i = 1; i < bytes.Length; i++)
                sb.AppendFormat("-{0:X2}", bytes[i]);
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (str == null)
                return null;

            str = str.Replace("-", "");
            try
            {
                byte[] bytes = new byte[str.Length/2];
                for (int i = 0, j = 0; i < bytes.Length; i++, j += 2)
                    bytes[i] = System.Convert.ToByte(str.Substring(j, 2), 16);
                return bytes;
            }
            catch (Exception)
            {

            }
            return null;
        }
    }
}
