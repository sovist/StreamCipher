using System;
using System.IO;

namespace StreamCipherCoder
{
    public static class FileCoder
    {
        public static void Coded(ICoder coder, string inputFileName, string outputFileName, Action<int> progress)
        {
            File.Create(outputFileName).Close();
            using (var fileWrite = new BinaryWriter(File.Open(outputFileName, FileMode.Open, FileAccess.Write)))
            using (var fileRead = new BinaryReader(File.Open(inputFileName, FileMode.Open, FileAccess.Read)))
            {
                const int readBufer = 3200000;
                while (fileRead.BaseStream.Position != fileRead.BaseStream.Length)
                {
                    var temp = fileRead.ReadBytes(readBufer);
                    progress((int)((double)fileRead.BaseStream.Position / fileRead.BaseStream.Length * 100));

                    coder.Coded(temp);
                    fileWrite.Write(temp);
                }
            }
        }

        public static void Decoded(ICoder coder, string inputFileName, string outputFileName, Action<int> progress)
        {
            Coded(coder, inputFileName, outputFileName, progress);
        }
    }
}