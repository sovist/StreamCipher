using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace UsbHidDevice
{
    public class Hash : IHash
    {
        private readonly MD5Cng _md5Cng = new MD5Cng();
        public byte[] Compute(int hashSizeInBytes, params byte[][] arr)
        {
            var allArrays = combineArrays(arr);
            var hash = _md5Cng.ComputeHash(allArrays);
            return compressHash(hashSizeInBytes, hash);
        }

        private static byte[] combineArrays(params byte[][] arrays)
        {
            var allLen = arrays.Sum(_ => _.Length);
            var resultArr = new byte[allLen];

            Array.Copy(arrays[0], 0, resultArr, 0, arrays[0].Length);
            for(var i = 1; i < arrays.Length; i++)
                Array.Copy(arrays[i], 0, resultArr, arrays[i-1].Length, arrays[i].Length);

            return resultArr;
        }

        private static byte[] compressHash(int hashSizeInBytes, IReadOnlyList<byte> hash)
        {
            var compresedHash = hashSizeInBytes > hash.Count ? new byte[hash.Count] : new byte[hashSizeInBytes];
            var blocks = (int)Math.Ceiling((double)hash.Count / hashSizeInBytes);

            for (var currentBlock = 0; currentBlock <= blocks; currentBlock++)
                for (int j = blocks * currentBlock, k = 0; k < compresedHash.Length && j < hash.Count; j++, k++)
                    compresedHash[k] ^= hash[j];

            return compresedHash;
        }
    }
}