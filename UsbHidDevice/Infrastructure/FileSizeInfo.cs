using System.IO;

namespace UsbHidDevice.Infrastructure
{
    internal class FileSizeInfo
    {
        public FileSizeInfo(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            Size = fileInfo.Length;
        }
        public string ShortForm
        {
            get
            {
                string[] arr = {" ", " K", " M", " G"};
                double size = Size;

                int index = 0;
                while (size > 1024 && index < 4)
                {
                    size /= 1024;
                    index++;
                }
                return size.ToString("f2") + arr[index] + "b (" + Size.ToString("### ### ### ### ###").TrimStart(' ') + " байт)";
            }
        }

        public long Size { get; }

        public static FileSizeInfo Info(string fileName)
        {
            return new FileSizeInfo(fileName);
        }
    }

    internal static class ByteSizeInfo
    {
        private static readonly string[] Arr = { " ", " K", " M", " G" };
        public static string Get(int len)
        {
            if (len == 0)
                return "0 b";

            double size = len;

            int index = 0;
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
