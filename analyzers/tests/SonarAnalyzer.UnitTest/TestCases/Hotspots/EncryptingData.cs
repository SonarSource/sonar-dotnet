namespace MyNamespace
{
    using System;
    using System.Security.Cryptography;

    // RSPEC example: https://jira.sonarsource.com/browse/RSPEC-4938
    public class MyClass
    {
        public void Main()
        {
            Byte[] data = { 1, 1, 1 };

            RSA myRSA = RSA.Create();
            RSAEncryptionPadding padding = RSAEncryptionPadding.CreateOaep(HashAlgorithmName.SHA1);

            // Review all base RSA class' Encrypt/Decrypt calls
            myRSA.Encrypt(data, padding);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Make sure that encrypting data is safe here.}}
            myRSA.EncryptValue(data);       // Noncompliant
            myRSA.Decrypt(data, padding);   // Noncompliant
            myRSA.DecryptValue(data);       // Noncompliant

            RSACryptoServiceProvider myRSAC = new RSACryptoServiceProvider();
            // Review the use of any TryEncrypt/TryDecrypt and specific Encrypt/Decrypt of RSA subclasses.
            myRSAC.Encrypt(data, false);    // Noncompliant
            myRSAC.Decrypt(data, false);    // Noncompliant
            int written;

            // Note: TryEncrypt/TryDecrypt are only in .NET Core 2.1+
            //            myRSAC.TryEncrypt(data, Span<byte>.Empty, padding, out written); // Non compliant
            //            myRSAC.TryDecrypt(data, Span<byte>.Empty, padding, out written); // Non compliant

            byte[] rgbKey = { 1, 2, 3 };
            byte[] rgbIV = { 4, 5, 6 };
            SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();
            // Review the creation of Encryptors from any SymmetricAlgorithm instance.
            rijn.CreateEncryptor();
//          ^^^^^^^^^^^^^^^^^^^^^^ {{Make sure that encrypting data is safe here.}}
            rijn.CreateEncryptor(rgbKey, rgbIV);    // Noncompliant
            rijn.CreateDecryptor();                 // Noncompliant
            rijn.CreateDecryptor(rgbKey, rgbIV);    // Noncompliant
        }

    }
    public class MyAsymmetricCrypto : System.Security.Cryptography.AsymmetricAlgorithm // Noncompliant
    {
        // ...
    }

    public class MySymmetricCrypto : System.Security.Cryptography.SymmetricAlgorithm // Noncompliant 
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) { return null; }
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) { return null; }
        public override void GenerateIV() { /* no-op */ }
        public override void GenerateKey() { /* no-op */ }
    }


    public class Class2
    {
        public void AdditionalTests(Byte[] data, RSAEncryptionPadding padding)
        {
            var customAsymProvider = new MyRSA();

            // Should raise on derived asymmetric classes
            customAsymProvider.Encrypt(data, padding);  // Noncompliant
            customAsymProvider.EncryptValue(data);      // Noncompliant
            customAsymProvider.Decrypt(data, padding);  // Noncompliant
            customAsymProvider.DecryptValue(data);      // Noncompliant

            // Should raise on the Try* methods added in NET Core 2.1
            // Note: this test is cheating - we can't currently referencing the
            // real 2.1 assemblies since the test project is targetting an older
            // NET Framework, so we're testing against a custom subclass
            // to which we've added the new method names.
            customAsymProvider.TryEncrypt();        // Noncompliant
            customAsymProvider.TryEncrypt(null);    // Noncompliant
            customAsymProvider.TryDecrypt();        // Noncompliant
            customAsymProvider.TryDecrypt(null);    // Noncompliant

            customAsymProvider.OtherMethod();

            // Should raise on derived symmetric classes
            var customSymProvider = new MySymmetricCrypto();
            customSymProvider.CreateEncryptor();    // Noncompliant
            customSymProvider.CreateDecryptor();    // Noncompliant
        }
    }

    public class MyRSA : System.Security.Cryptography.RSA // Noncompliant
    {
        // Dummy methods with the same names as the additional methods added in Net Core 2.1.
        public void TryEncrypt() { /* no-op */ }
        public void TryEncrypt(string dummyMethod) { /* no-op */ }
        public void TryDecrypt() { /* no-op */ }
        public void TryDecrypt(string dummyMethod) { /* no-op */ }

        public void OtherMethod() { /* no-op */ }


        // Abstract methods
        public override RSAParameters ExportParameters(bool includePrivateParameters) { return new RSAParameters(); }
        public override void ImportParameters(RSAParameters parameters) { /* no-op */ }
    }

}

