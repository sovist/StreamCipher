using System.IO;

namespace StreamCipher.Infrastructure
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
}
