using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class MySHA1Managed : SHA1Managed
    {
    }

    public class InsecureHashAlgorithm
    {
        public void Hash(byte[] temp)
        {
            using (var sha1 = new MySHA1Managed()) //Noncompliant
            {
            }
        }

        public void HmaCalls()
        {
            using (var hmacripemd160 = HMACRIPEMD160.Create("HMACRIPEMD160")) // Noncompliant
            {
            }
            using (var hmacripemd160 = KeyedHashAlgorithm.Create("HMACRIPEMD160")) // Noncompliant
            {
            }
            using (var hmacripemd160 = (KeyedHashAlgorithm)CryptoConfig.CreateFromName("HMACRIPEMD160")) // Noncompliant
            {
            }
        }

        public void RipemdCalls()
        {
            using (var ripemd160 = RIPEMD160.Create()) // Noncompliant
            {
            }
            using (var ripemd160 = HashAlgorithm.Create("RIPEMD160")) // Noncompliant
            {
            }
            using (var ripemd160 = (HashAlgorithm)CryptoConfig.CreateFromName("RIPEMD160")) // Noncompliant
            {
            }

            using (var ripemd160Managed = RIPEMD160Managed.Create()) // Noncompliant
            {
            }
            using (var ripemd160Managed = HashAlgorithm.Create("RIPEMD160Managed")) // Noncompliant
            {
            }
            using (var ripemd160Managed = (HashAlgorithm)CryptoConfig.CreateFromName("RIPEMD160Managed")) // Noncompliant
            {
            }
        }
    }
}
