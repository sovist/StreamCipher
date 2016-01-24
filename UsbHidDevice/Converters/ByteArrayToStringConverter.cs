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
            if (bytes == null || bytes.Length == 0)
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
            if (string.IsNullOrEmpty(str))
                return null;

            str = getHexString(str);

            var bytes = new byte[str.Length / 2];
            for (int i = 0, j = 0; i < bytes.Length; i++, j += 2)
                bytes[i] = System.Convert.ToByte(str.Substring(j, 2), 16);

            return bytes;
        }

        private static string getHexString(string str)
        {
            str = str.Replace("-", "");
            if (str.Length % 2 != 0)
                str += "0";

            var stringBuilder = new StringBuilder(str);
            for (var i = 0; i < stringBuilder.Length; )          
                if (isHex(stringBuilder[i]))
                    i++;
                else
                    stringBuilder.Remove(i, 1);
            
            return stringBuilder.ToString();
        }

        private static bool isHex(char value)
        {
            if ('a' <= value && value <= 'f')
                return true;

            if ('A' <= value && value <= 'F')
                return true;

            if ('0' <= value && value <= '9')
                return true;

            return false;
        }
    }
}