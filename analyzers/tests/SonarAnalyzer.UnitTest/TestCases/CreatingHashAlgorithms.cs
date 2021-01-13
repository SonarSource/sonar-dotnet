using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class InsecureHashAlgorithm
    {
        public void Hash(byte[] temp)
        {
            using var DSACng = new DSACng(10); // Noncompliant {{Make sure this weak hash algorithm is not used in a sensitive context here.}}
//                             ^^^^^^^^^^^^^^
            using var DSACryptoServiceProvider = new DSACryptoServiceProvider(); // Noncompliant
            using var DSACreate = DSA.Create(); // Noncompliant
            using var DSACreateWithParam = DSA.Create("DSA"); // Noncompliant
            using var DSACreateFromName = (AsymmetricAlgorithm)CryptoConfig.CreateFromName("DSA"); // Noncompliant
            using var DSAAsymmetricAlgorithm = AsymmetricAlgorithm.Create("DSA"); // Noncompliant

            using var HMACCreate = HMAC.Create(); // Noncompliant
            using var HMACCreateWithParam = HMAC.Create("HMACMD5"); // Noncompliant

            using var HMACMD5 = new HMACMD5(); // Noncompliant
            using var HMACMD5Create = HMACMD5.Create(); // Noncompliant
            using var HMACMD5CreateWithParam = HMACMD5.Create("HMACMD5"); // Noncompliant
            using var HMACMD5KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACMD5"); // Noncompliant
            using var HMACMD5CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("HMACMD5"); // Noncompliant

            using var HMACRIPEMD160 = new HMACRIPEMD160(); // Noncompliant
            using var HMACRIPEMD160Create = HMACMD5.Create(); // Noncompliant
            using var HMACRIPEMD160CreateWithParam = HMACMD5.Create("HMACRIPEMD160"); // Noncompliant
            using var HMACRIPEMD160KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACRIPEMD160"); // Noncompliant
            using var HMACRIPEMD160CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("HMACRIPEMD160"); // Noncompliant

            using var HMACSHA1 = new HMACSHA1(); // Noncompliant

            using var HMACSHA256Create = HMACSHA256.Create("HMACSHA256");
            using var HMACSHA256KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA256");
            using var HMACSHA256CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("HMACSHA256");

            using var MD5Cng = new MD5Cng(); // Noncompliant
            using var MD5CryptoServiceProvider = new MD5CryptoServiceProvider(); // Noncompliant
            using var MD5CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("MD5"); // Noncompliant
            using var MD5HashAlgorithm = HashAlgorithm.Create("MD5"); // Noncompliant
            using var MD5Create = MD5.Create(); // Noncompliant
            using var MD5CreateWithParam = MD5.Create("MD5"); // Noncompliant

            using var RIPEMD160Managed  = new RIPEMD160Managed(); // Noncompliant
            using var RIPEMD160ManagedCreate = RIPEMD160Managed.Create("RIPEMD160Managed"); // Noncompliant
            using var RIPEMD160ManagedHashAlgorithm = HashAlgorithm.Create("RIPEMD160Managed"); // Noncompliant
            using var RIPEMD160ManagedCryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("RIPEMD160Managed"); // Noncompliant

            using var RIPEMD160Create = RIPEMD160.Create(); // Noncompliant
            using var RIPEMD160CreateWithParam = RIPEMD160.Create("RIPEMD160"); // Noncompliant
            using var RIPEMD160HashAlgorithm = HashAlgorithm.Create("RIPEMD160"); // Noncompliant
            using var RIPEMD160CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("RIPEMD160"); // Noncompliant

            using var SHA1Create = SHA1.Create(); // Noncompliant
            using var SHA1CreateWithParam = SHA1.Create("SHA1"); // Noncompliant
            using var SHA1Managed = new SHA1Managed(); // Noncompliant
            using var SHA1HashAlgorithm = HashAlgorithm.Create("SHA1"); // Noncompliant
            using var SHA1CryptoServiceProvider = new SHA1CryptoServiceProvider(); // Noncompliant

            using var sha256Managed = new SHA256Managed();
            using var sha256HashAlgorithm = HashAlgorithm.Create("SHA256Managed");
            var sha256CryptoConfig = CryptoConfig.CreateFromName("SHA256Managed");

            HashAlgorithm hashAlgo = HashAlgorithm.Create();

            var algoName = "MD5";
            var md5Var = (HashAlgorithm)CryptoConfig.CreateFromName(algoName); // Noncompliant
            algoName = "SHA256Managed";
            var SHA256ManagedVar = (HashAlgorithm)CryptoConfig.CreateFromName(algoName);
        }
    }
}
