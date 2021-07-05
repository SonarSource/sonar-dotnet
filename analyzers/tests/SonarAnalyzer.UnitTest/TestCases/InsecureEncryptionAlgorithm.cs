using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;

namespace Tests.Diagnostics
{
    public class MyTripleDESCryptoServiceProvider : TripleDES
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new System.NotImplementedException();
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            throw new System.NotImplementedException();
        }

        public override void GenerateIV()
        {
            throw new System.NotImplementedException();
        }

        public override void GenerateKey()
        {
            throw new System.NotImplementedException();
        }
    }

    public class InsecureEncryptionAlgorithm
    {
        public InsecureEncryptionAlgorithm()
        {
            // Rule will raise an issue for both S2278 and S5547 as they are activated by default in unit tests

            using (var tripleDES = new MyTripleDESCryptoServiceProvider()) // Noncompliant    {{Use a strong cipher algorithm.}}
//                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
                //...
            }
            using (var des = new DESCryptoServiceProvider()) // Noncompliant
            {
                //...
            }
            using (TripleDES TripleDESalg = TripleDES.Create()) // Noncompliant
//                                          ^^^^^^^^^^^^^^^^^^
            {
            }

            using (var des = DES.Create("fgdsgsdfgsd")) // Noncompliant
            {
            }

            using (var aes = new AesCryptoServiceProvider())
            {
                //...
            }

            using (var rc21 = new RC2CryptoServiceProvider()) // Noncompliant
            {

            }

            using (var rc22 = RC2.Create()) // Noncompliant
            {

            }

            SymmetricAlgorithm des1 = SymmetricAlgorithm.Create("DES"); // Noncompliant
            des1 = SymmetricAlgorithm.Create("TripleDES"); // Noncompliant
            des1 = SymmetricAlgorithm.Create("3DES"); // Noncompliant

            var rc2 = SymmetricAlgorithm.Create("RC2"); // Noncompliant

            var crypto = CryptoConfig.CreateFromName("DES"); // Noncompliant

            var aesFastEngine = new AesFastEngine(); // Noncompliant
//                                  ^^^^^^^^^^^^^

            var blockCipher1 = new GcmBlockCipher(new AesFastEngine()); // Noncompliant

            var blockCipher2 = new GcmBlockCipher(new AesEngine());     // Compliant

            var oid = CryptoConfig.MapNameToOID("DES"); // Compliant

            var unknown = new Unknown(); // Compliant // Error [CS0246]
        }
    }
}
