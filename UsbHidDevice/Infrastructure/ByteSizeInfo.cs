namespace UsbHidDevice.Infrastructure
{
    internal static class ByteSizeInfo
    {
        private static readonly string[] Arr = { " ", " K", " M", " G" };
        public static string Get(int len)
        {
            if (len == 0)
                return "0 b";

            double size = len;

            var index = 0;
            while (size > 1024 && index < 4)
            {
                size /= 1024;
                index++;
            }

            if (index == 0)
                return size.ToString("# ###") + Arr[index] + "b";

            return size.ToString("f2") + Arr[index] + "b";/*" (" + len.ToString("### ### ### ### ###").TrimStart(' ') + " байт)";*/
        }
    }
}