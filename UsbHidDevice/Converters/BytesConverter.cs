using System;

namespace UsbHidDevice.Converters
{
    public static class BytesConverter
    {
        public static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length*sizeof (char)];
            var strArr = str.ToCharArray();
            Buffer.BlockCopy(strArr, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            try
            {
                var len = (int)Math.Round((double) bytes.Length/sizeof (char));
                var chars = new char[len];
                Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
                return new string(chars);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}