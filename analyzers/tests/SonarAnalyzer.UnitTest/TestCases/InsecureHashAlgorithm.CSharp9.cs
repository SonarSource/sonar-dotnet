using System.Security.Cryptography;

using (SHA1Managed sha1 = new())                // Noncompliant {{Use a stronger hashing/asymmetric algorithm.}}
{ }
using (SHA1CryptoServiceProvider sha1 = new())  // Noncompliant
{ }

using (SHA256Managed sha256 = new())            // Compliant
{
}
using SHA512Managed sha512 = new();             // Compliant

using MD5CryptoServiceProvider md5 = new();     // Noncompliant
using DSACryptoServiceProvider dsa = new();     // Noncompliant
using HMACSHA1 hmac = new();                    // Noncompliant
