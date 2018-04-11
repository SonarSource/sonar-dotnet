using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void CompliantCases()
        {
            new AesManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.ANSIX923
            };

            new AesManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.ISO10126
            };

            new AesManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None
            };

            new AesManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros
            };
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
                Mode = CipherMode.CBC, // Compliant - False Negative
                Padding = PaddingMode.PKCS7
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
