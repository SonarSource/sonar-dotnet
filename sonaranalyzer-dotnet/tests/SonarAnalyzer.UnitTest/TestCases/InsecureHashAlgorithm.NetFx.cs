using System;
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
            using (var sha1 = new SHA1Managed()) //Noncompliant {{Use a stronger hashing/asymmetric algorithm.}}
//                                ^^^^^^^^^^^
            {
            }
            using (var sha1 = new SHA1CryptoServiceProvider()) //Noncompliant
            {
            }
            using (var sha1 = new MySHA1Managed()) //Noncompliant
            {
            }

            using (var sha256 = new SHA256Managed())
            {
            }
            using (var sha256 = (HashAlgorithm)CryptoConfig.CreateFromName("SHA256Managed"))
            {
            }
            using (var sha256 = HashAlgorithm.Create("SHA256Managed"))
            {
            }
        }

        public void Md5Calls()
        {
            byte[] temp = null;

            using (var md5 = new MD5CryptoServiceProvider()) //Noncompliant
            {
                var hash = md5.ComputeHash(temp);
            }

            using (var md5 = (HashAlgorithm)CryptoConfig.CreateFromName("MD5")) //Noncompliant
//                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
                var hash = md5.ComputeHash(temp);
            }
            using (var md5 = HashAlgorithm.Create("MD5")) //Noncompliant
            {
                var hash = md5.ComputeHash(temp);
            }
            var algoName = "MD5";
            using (var md5 = (HashAlgorithm)CryptoConfig.CreateFromName(algoName)) // not recognized yet
            {
                var hash = md5.ComputeHash(temp);
            }
        }

        public void DsaCalls()
        {
            using (var dsa = new DSACryptoServiceProvider()) // Noncompliant
            {
            }
            using (var dsa = DSA.Create()) // Noncompliant
            {
            }
            using (var dsa = (AsymmetricAlgorithm)CryptoConfig.CreateFromName("DSA")) // Noncompliant
            {
            }
            using (var dsa = AsymmetricAlgorithm.Create("DSA")) // Noncompliant
            {
            }
        }

        public void HmaCalls()
        {
            using (var hmac = new HMACSHA1()) // Noncompliant
            {
            }
            using (var hmac = HMAC.Create()) // Noncompliant
            {
            }
            using (var hmacmd5 = HMACMD5.Create("HMACMD5")) // Noncompliant
            {
            }
            using (var hmacmd5 = KeyedHashAlgorithm.Create("HMACMD5")) // Noncompliant
            {
            }
            using (var hmacmd5 = (KeyedHashAlgorithm)CryptoConfig.CreateFromName("HMACMD5")) // Noncompliant
            {
            }

            using (var hmacripemd160 = HMACRIPEMD160.Create("HMACRIPEMD160")) // Noncompliant
            {
            }
            using (var hmacripemd160 = KeyedHashAlgorithm.Create("HMACRIPEMD160")) // Noncompliant
            {
            }
            using (var hmacripemd160 = (KeyedHashAlgorithm)CryptoConfig.CreateFromName("HMACRIPEMD160")) // Noncompliant
            {
            }

            using (var hmacsha256 = HMACSHA256.Create("HMACSHA256"))
            {
            }
            using (var hmacsha256 = KeyedHashAlgorithm.Create("HMACSHA256"))
            {
            }
            using (var hmacsha256 = (KeyedHashAlgorithm)CryptoConfig.CreateFromName("HMACSHA256"))
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
