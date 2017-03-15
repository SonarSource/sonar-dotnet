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
            using (var sha1 = new SHA1Managed()) //Noncompliant {{Use a stronger encryption algorithm than SHA1.}}
//                                ^^^^^^^^^^^
            {
                var hash = sha1.ComputeHash(temp);
            }
            using (var sha1 = new SHA1CryptoServiceProvider()) //Noncompliant
            {
                var hash = sha1.ComputeHash(temp);
            }
            using (var sha1 = new MySHA1Managed()) //Noncompliant
            {
                var hash = sha1.ComputeHash(temp);
            }

            using (var md5 = new MD5CryptoServiceProvider()) //Noncompliant
            {
                var hash = md5.ComputeHash(temp);
            }

            using (var md5 = (HashAlgorithm)CryptoConfig.CreateFromName("MD5")) //Noncompliant {{Use a stronger encryption algorithm than MD5.}}
//                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
                var hash = md5.ComputeHash(temp);
            }
            using (var md5 = HashAlgorithm.Create("MD5")) //Noncompliant
            {
                var hash = md5.ComputeHash(temp);
            }

            using (var sha1 = new SHA256Managed())
            {
                var hash = sha1.ComputeHash(temp);
            }

            using (var md5 = (HashAlgorithm)CryptoConfig.CreateFromName("SHA256Managed"))
            {
                var hash = md5.ComputeHash(temp);
            }

            using (var md5 = HashAlgorithm.Create("SHA256Managed"))
            {
                var hash = md5.ComputeHash(temp);
            }

            var algoName = "MD5";
            using (var md5 = (HashAlgorithm)CryptoConfig.CreateFromName(algoName)) // not recognized yet
            {
                var hash = md5.ComputeHash(temp);
            }
        }
    }
}
