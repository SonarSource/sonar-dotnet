using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class MyTripleDESCryptoServiceProvider : TripleDES { }

    public class InsecureEncryptionAlgorithm
    {
        public InsecureEncryptionAlgorithm()
        {
            using (var tripleDES = new MyTripleDESCryptoServiceProvider()) //Noncompliant {{Use the recommended AES (Advanced Encryption Standard) instead.}}
//                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
                //...
            }

            using (var des = new DESCryptoServiceProvider()) //Noncompliant
            {
                //...
            }

            using (TripleDES TripleDESalg = TripleDES.Create()) //Noncompliant
//                                          ^^^^^^^^^^^^^^^^^^
            {
            }
            using (var des = DES.Create("fgdsgsdfgsd")) //Noncompliant
            {
            }

            using (var aes = new AesCryptoServiceProvider())
            {
                //...
            }

            SymmetricAlgorithm des1 = SymmetricAlgorithm.Create("DES"); //Noncompliant
            des1 = SymmetricAlgorithm.Create("TripleDES"); //Noncompliant
            des1 = SymmetricAlgorithm.Create("3DES"); //Noncompliant
        }
    }
}
