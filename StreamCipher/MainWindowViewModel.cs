namespace StreamCipher
{
    public class MainWindowViewModel
    {
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
        public byte[] InitBytesShift { get; set; }
        public byte[] InitBytesRegister { get; set; }
    }
}
