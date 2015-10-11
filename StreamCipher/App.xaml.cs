using System.Windows;
using Microsoft.Practices.Prism.Events;

namespace StreamCipher
{
    public partial class App : Application
    {
        public static IEventAggregator EventAggregator { get; private set; }

        public App()
        {
            EventAggregator = new EventAggregator();
        }
    }
}