using System;
using System.Collections.Generic;
using System.Security.Cryptography;

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

                                                                           // Noncompliant@+1 {{Use the recommended AES (Advanced Encryption Standard) instead.}}
            using (var tripleDES = new MyTripleDESCryptoServiceProvider()) // Noncompliant    {{Use a strong cipher algorithm.}}
//                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
                //...
            }
                                                             // Noncompliant@+1
            using (var des = new DESCryptoServiceProvider()) // Noncompliant
            {
                //...
            }
                                                                // Noncompliant@+1
            using (TripleDES TripleDESalg = TripleDES.Create()) // Noncompliant
//                                          ^^^^^^^^^^^^^^^^^^
            {
            }
                                                        // Noncompliant@+1
            using (var des = DES.Create("fgdsgsdfgsd")) // Noncompliant
            {
            }

            using (var aes = new AesCryptoServiceProvider())
            {
                //...
            }
                                                              // Noncompliant@+1
            using (var rc21 = new RC2CryptoServiceProvider()) // Noncompliant
            {

            }

                                            // Noncompliant@+1
            using (var rc22 = RC2.Create()) // Noncompliant
            {

            }
                                                                        // Noncompliant@+1
            SymmetricAlgorithm des1 = SymmetricAlgorithm.Create("DES"); // Noncompliant

                                                           // Noncompliant@+1
            des1 = SymmetricAlgorithm.Create("TripleDES"); // Noncompliant

                                                      // Noncompliant@+1
            des1 = SymmetricAlgorithm.Create("3DES"); // Noncompliant

                                                        // Noncompliant@+1
            var rc2 = SymmetricAlgorithm.Create("RC2"); // Noncompliant

                                                             // Noncompliant@+1
            var crypto = CryptoConfig.CreateFromName("DES"); // Noncompliant
        }
    }
}
