namespace MyNamespace
{
    using System;
    using System.Security.Cryptography;

    public class MyClass
    {
        public void Main()
        {
            RSA myRSA = RSA.Create();
            RSAEncryptionPadding padding = RSAEncryptionPadding.CreateOaep(HashAlgorithmName.SHA1);

            // Review all base RSA class' Encrypt/Decrypt calls
            myRSA.Encrypt("EncryptMe"u8, padding); // Noncompliant

            myRSA.Encrypt("""EncryptMe"""u8, padding); // Noncompliant
        }

    }
}

