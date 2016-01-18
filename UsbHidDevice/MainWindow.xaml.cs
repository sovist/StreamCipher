namespace UsbHidDevice
{
    public partial class MainWindow
    {
        public MainWindowViewModel Model { get; }
        public MainWindow()
        {
            Model = new MainWindowViewModel();
            InitializeComponent();
        }
    }
}