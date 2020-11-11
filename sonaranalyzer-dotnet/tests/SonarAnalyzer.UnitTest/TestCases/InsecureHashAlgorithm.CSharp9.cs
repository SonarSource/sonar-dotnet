using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class InsecureHashAlgorithm
    {
        public void Hash()
        {
            using (SHA1Managed sha1 = new())                // FN
            { }
            using (SHA1CryptoServiceProvider sha1 = new())  //FN
            { }

            using (SHA256Managed sha256 = new())            // Compliant
            {
            }
            using SHA512Managed sha512 = new();             // Compliant

            using MD5CryptoServiceProvider md5 = new();     // FN
            using DSACryptoServiceProvider dsa = new();     // FN
            using HMACSHA1 hmac = new();                    // FN
        }
    }
}
