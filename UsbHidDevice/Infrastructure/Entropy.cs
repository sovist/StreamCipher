using System;
using System.IO;
using System.Threading;

namespace UsbHidDevice.Infrastructure
{
    internal static class Entropy
    {
        public static double Value(string file, Action<int> progress)
        {
            ulong[] arr;
            return Calculate(file, progress, out arr);
        }

        public static ulong[] Calculate(string file, Action<int> progress)
        {
            ulong[] masCountBytes = new ulong[256];
            try
            {
                using (var fileRead = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read)))
                {
                    var length = fileRead.BaseStream.Length;

                    while (fileRead.BaseStream.Position != length)
                    {
                        byte[] temp1 = fileRead.ReadBytes(3200000);

                        progress((int)(((double)fileRead.BaseStream.Position / fileRead.BaseStream.Length) * 100));

                        foreach (byte b in temp1)
                            masCountBytes[b]++;
                    }
                }
            }
            catch (IOException) { }

            return masCountBytes;
        }

        public static double Calculate(string file, Action<int> progress, out ulong[] masBytes)
        {
            masBytes = Calculate(file, progress);

            long length = 1;
            try
            {
                length = new FileInfo(file).Length;
            }
            catch (IOException) { }

            double entropy = 0;
            for (int i = 0; i < masBytes.Length; i++)
                if (masBytes[i] != 0)
                {
                    double temp = (double) masBytes[i]/length;
                    entropy += -temp * Math.Log(temp, 2);
                }
            progress(100);
            Thread.Sleep(300);
            return entropy;
        }
    }
}
