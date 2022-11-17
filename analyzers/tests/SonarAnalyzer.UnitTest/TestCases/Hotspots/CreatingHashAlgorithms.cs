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
            using var DSAAsymmetricAlgorithmWithNamespace = AsymmetricAlgorithm.Create("System.Security.Cryptography.DSA"); // Noncompliant

            using var HMACCreate = HMAC.Create(); // Noncompliant
            using var HMACCreateWithParam = HMAC.Create("HMACMD5"); // Noncompliant

            using var HMACMD5 = new HMACMD5(); // Noncompliant
            using var HMACMD5Create = HMACMD5.Create(); // Noncompliant
            using var HMACMD5CreateWithParam = HMACMD5.Create("HMACMD5"); // Noncompliant
            using var HMACMD5KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACMD5"); // Noncompliant
            using var HMACMD5KeyedHashAlgorithmWithNamespace = KeyedHashAlgorithm.Create("System.Security.Cryptography.HMACMD5"); // Noncompliant
            using var HMACMD5CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("HMACMD5"); // Noncompliant

            using var HMACSHA1 = new HMACSHA1(); // Noncompliant
            using var HMACSHA1Create = HMACMD5.Create(); // Noncompliant
            using var HMACSHA1CreateWithParam = HMACMD5.Create("HMACSHA1"); // Noncompliant
            using var HMACSHA1KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA1"); // Noncompliant
            using var HMACSHA1KeyedHashAlgorithmWithNamespace = KeyedHashAlgorithm.Create("System.Security.Cryptography.HMACSHA1"); // Noncompliant
            using var HMACSHA1CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("HMACSHA1"); // Noncompliant

            using var HMACSHA256Create = HMACSHA256.Create("HMACSHA256");
            using var HMACSHA256KeyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA256");
            using var HMACSHA256KeyedHashAlgorithmWithNamespace = KeyedHashAlgorithm.Create("System.Security.Cryptography.HMACSHA256");
            using var HMACSHA256CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("HMACSHA256");

            using var MD5CryptoServiceProvider = new MD5CryptoServiceProvider(); // Noncompliant
            using var MD5CryptoConfig = (HashAlgorithm)CryptoConfig.CreateFromName("MD5"); // Noncompliant
            using var MD5HashAlgorithm = HashAlgorithm.Create("MD5"); // Noncompliant
            using var MD5HashAlgorithmWithNamespace = HashAlgorithm.Create("System.Security.Cryptography.MD5"); // Noncompliant
            using var MD5Create = MD5.Create(); // Noncompliant
            using var MD5CreateWithParam = MD5.Create("MD5"); // Noncompliant

            using var SHA1Managed = new SHA1Managed(); // Noncompliant
            using var SHA1Create = SHA1.Create(); // Noncompliant
            using var SHA1CreateWithParam = SHA1.Create("SHA1"); // Noncompliant
            using var SHA1HashAlgorithm = HashAlgorithm.Create("SHA1"); // Noncompliant
            using var SHA1HashAlgorithmWithNamespace = HashAlgorithm.Create("System.Security.Cryptography.SHA1"); // Noncompliant
            using var SHA1CryptoServiceProvider = new SHA1CryptoServiceProvider(); // Noncompliant

            using var sha256Managed = new SHA256Managed();
            using var sha256HashAlgorithm = HashAlgorithm.Create("SHA256Managed");
            using var sha256HashAlgorithmWithNamespace = HashAlgorithm.Create("System.Security.Cryptography.SHA256Managed");
            var sha256CryptoConfig = CryptoConfig.CreateFromName("SHA256Managed");

            HashAlgorithm hashAlgo = HashAlgorithm.Create();

            var algoName = "MD5";
            var md5Var = (HashAlgorithm)CryptoConfig.CreateFromName(algoName); // Noncompliant
            algoName = "SHA256Managed";
            var SHA256ManagedVar = (HashAlgorithm)CryptoConfig.CreateFromName(algoName);

            const string part1 = "System.Security.Cryptography";
            const string part2 = "SHA1";
            using var SHA1HashAlgorithmInterpolation = HashAlgorithm.Create($"{part1}.{part2}"); // Noncompliant

            string part3 = "System.Security.Cryptography";
            string part4 = "SHA1";
            using var SHA1HashAlgorithmInterpolation2 = HashAlgorithm.Create($"{part3}.{part4}"); // FN (at the moment we validate only constant string)
        }
    }
}
