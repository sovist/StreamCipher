﻿using System.Security.Cryptography;

namespace StreamCipher.Infrastructure
{
    static class GenerateBytesSequense
    {
        public static byte[] Get(int byteCount)
        {
            var arr = new byte[byteCount];
            new RNGCryptoServiceProvider().GetNonZeroBytes(arr);
            return arr;
        }
    }
}