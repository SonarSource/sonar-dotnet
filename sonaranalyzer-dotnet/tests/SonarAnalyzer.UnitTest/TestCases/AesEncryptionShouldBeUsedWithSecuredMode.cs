using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void CompliantCases()
        {
            // None
        }

        public void NonCompliantCases()
        {
            var aes = new AesManaged();

            new AesManaged
            {
                Mode = CipherMode.ECB // Noncompliant
//                     ^^^^^^^^^^^^^^
            };

            new AesManaged
            {
                Mode = CipherMode.CBC, // Noncompliant
            };

            new AesManaged
            {
                Mode = CipherMode.OFB // Noncompliant
            };

            new AesManaged
            {
                Mode = CipherMode.CFB // Noncompliant
            };

            new AesManaged
            {
                Mode = CipherMode.CTS // Noncompliant
            };

            aes.Mode = CipherMode.ECB; // Noncompliant
        }
    }
}
