using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace P2PProcessing.Utils
{
    public static class Hasher
    {
        public static string getHashHexRepresentation(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;

            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }

            return hashString;
        }
    }
}
