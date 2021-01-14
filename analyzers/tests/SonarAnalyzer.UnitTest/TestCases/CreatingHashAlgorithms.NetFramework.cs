using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class InsecureHashAlgorithm
    {
        public void Hash(byte[] temp)
        {
            using var HMACRIPEMD160 = new HMACRIPEMD160(); // Noncompliant
            using var HMACRIPEMD160Create = HMACMD5.Create(); // Noncompliant
            using var HMACRIPEMD160CreateWithParam = HMACMD5.Create("HMACRIPEMD160"); // Noncompliant
            using var HMACRIPEMD160KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACRIPEMD160"); // Noncompliant
            using var HMACRIPEMD160KeyedHashAlgorithmWithNamespace = KeyedHashAlgorithm.Create("System.Security.Cryptography.HMACRIPEMD160"); // Noncompliant
            using var HMACRIPEMD160CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("HMACRIPEMD160"); // Noncompliant

            using var MD5Cng = new MD5Cng(); // Noncompliant

            using var RIPEMD160Managed  = new RIPEMD160Managed(); // Noncompliant

            using var RIPEMD160Create = RIPEMD160.Create(); // Noncompliant
            using var RIPEMD160CreateWithParam = RIPEMD160.Create("RIPEMD160"); // Noncompliant
            using var RIPEMD160HashAlgorithm = HashAlgorithm.Create("RIPEMD160"); // Noncompliant
            using var RIPEMD160HashAlgorithmWithNamespace = HashAlgorithm.Create("System.Security.Cryptography.RIPEMD160"); // Noncompliant
            using var RIPEMD160CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("RIPEMD160"); // Noncompliant
        }
    }
}
