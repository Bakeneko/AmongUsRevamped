using System;
using System.Security.Cryptography;
using System.Text;

namespace AmongUsRevamped.Utils
{
    internal static class HashUtils
    {
        public const int Length = 16;

        public static byte[] Hash(string value)
        {
            byte[] buffer = new byte[Length];
            using (SHA1 algorithm = SHA1.Create()) Array.Copy(algorithm.ComputeHash(Encoding.UTF8.GetBytes(value)), 0, buffer, 0, Length);
            return buffer;
        }
    }
}
