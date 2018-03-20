using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class Program
    {
        private const int validKeySizeConst = 2048;
        private const int invalidKeySizeConst = 1024;

        private static readonly int validKeySize = 2048;
        private static readonly int invalidKeySize = 1024;

        public void Method()
        {
            const int localValidSize = 2048;
            new RSACryptoServiceProvider();
            new RSACryptoServiceProvider(new CspParameters());
            new RSACryptoServiceProvider(2048);
            new RSACryptoServiceProvider(localValidSize);
            new RSACryptoServiceProvider(validKeySizeConst);
            new RSACryptoServiceProvider(validKeySize);
            new RSACryptoServiceProvider(invalidKeySize); // Compliant - FN - cannot detect static readonly from GetConstantValue


            const int localInvalidSize = 1024;
            new RSACryptoServiceProvider(1024); // Noncompliant {{Use a key length of at least '2048' bits.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new RSACryptoServiceProvider(1024, new CspParameters()); // Noncompliant
            new RSACryptoServiceProvider(invalidKeySizeConst); // Noncompliant
            new RSACryptoServiceProvider(localInvalidSize); // Noncompliant
        }
    }
}
