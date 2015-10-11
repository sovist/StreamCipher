using Microsoft.Practices.Prism.Events;

namespace StreamCipher.Infrastructure
{
    class Events
    {
        public class InputFileIsChenged : CompositePresentationEvent<string> { }
        public class OutputFileIsChenged : CompositePresentationEvent<string> { }

        public class InputFileEntropyIsCalculated : CompositePresentationEvent<string> { }
        public class OutputFileEntropyIsCalculated : CompositePresentationEvent<string> { }

        public class InitRegisterBytesIsChenged : CompositePresentationEvent<byte[]> { }
        public class InitShiftBytesIsChenged : CompositePresentationEvent<byte[]> { }
    }
}