using System.IO;

namespace StreamCipher.Infrastructure
{
    class FileSizeInfo
    {
        private readonly long _fileLenght;

        public FileSizeInfo(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            _fileLenght = fileInfo.Length;
        }
        public string ShortForm
        {
            get
            {
                string[] arr = {" ", " K", " M", " G"};
                double size = _fileLenght;

                int index = 0;
                while (size > 1024 && index < 4)
                {
                    size /= 1024;
                    index++;
                }
                return size.ToString("f2") + arr[index] + "b (" + _fileLenght.ToString("### ### ### ### ###").TrimStart(' ') + " байт)";
            }
        }

        public long Size 
        {
            get { return _fileLenght; }
        }

        public static FileSizeInfo Info(string fileName)
        {
            return new FileSizeInfo(fileName);
        }
    }
}
