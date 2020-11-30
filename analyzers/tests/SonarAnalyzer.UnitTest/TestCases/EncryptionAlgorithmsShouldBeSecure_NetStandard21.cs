using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class SecureAesCheck
    {
        AesManaged field1 = new AesManaged(); // Noncompliant
        AesManaged Property1 { get; set; } = new AesManaged(); // Noncompliant

        void CtorSetsAllowedValue(byte[] key1, ReadOnlySpan<byte> key2)
        {
            new AesGcm(key1);
            new AesGcm(key2);
        }

        void CtorSetsNotAllowedValue()
        {
            new AesManaged(); // Noncompliant {{Use secure mode and padding scheme.}}
        }

        void InitializerSetsAllowedValue()
        {
            // none
        }

        void InitializerSetsNotAllowedValue()
        {
            new AesManaged() { Mode = CipherMode.CBC }; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new AesManaged() { Mode = CipherMode.CFB }; // Noncompliant
            new AesManaged() { Mode = CipherMode.CTS }; // Noncompliant
            new AesManaged() { Mode = CipherMode.ECB }; // Noncompliant
            new AesManaged() { Mode = CipherMode.OFB }; // Noncompliant
        }

        void PropertySetsNotAllowedValue()
        {
            var c = new AesManaged(); // Noncompliant
            c.Mode = CipherMode.CBC; // Noncompliant, we will be raising twice
            c.Mode = CipherMode.CFB; // Noncompliant, we will be raising twice
            c.Mode = CipherMode.CTS; // Noncompliant, we will be raising twice
            c.Mode = CipherMode.ECB; // Noncompliant, we will be raising twice
            c.Mode = CipherMode.OFB; // Noncompliant, we will be raising twice
        }

        void PropertySetsAllowedValue(bool foo)
        {
            // none
        }
    }

    class RSAEncryptionPaddingTest
    {
        RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();

        void InvocationSetsAllowedValue(byte[] data, RSAEncryptionPadding padding, RSA genericRSA)
        {
            rsaProvider.Encrypt(data, true); // OAEP Padding is used (second parameter set to true)
            rsaProvider.Decrypt(data, false); // Only raise on Encrypt method
            rsaProvider.Encrypt(fOAEP: true, rgb: data);
            rsaProvider.Encrypt(data, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA1);
            rsaProvider.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
            rsaProvider.TryEncrypt(data, null, RSAEncryptionPadding.OaepSHA256, out var bytesWritten1);
            rsaProvider.Encrypt(padding: padding, data: data); // we don't know which padding is actually used here so we do not raise the issue

            genericRSA.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
            genericRSA.TryEncrypt(data, null, RSAEncryptionPadding.OaepSHA256, out var bytesWritten2);
        }

        void InvocationSetsNotAllowedValue(byte[] data, RSA genericRSA)
        {
            rsaProvider.Encrypt(data, false); // Noncompliant {{Use secure mode and padding scheme.}}
            rsaProvider.Encrypt(fOAEP: false, rgb: data); // Noncompliant
            rsaProvider.Encrypt(data, System.Security.Cryptography.RSAEncryptionPadding.Pkcs1); // Noncompliant
            rsaProvider.Encrypt(data, RSAEncryptionPadding.Pkcs1); // Noncompliant
            rsaProvider.TryEncrypt(data, null, RSAEncryptionPadding.Pkcs1, out var bytesWritten1); // Noncompliant

            genericRSA.Encrypt(data, RSAEncryptionPadding.Pkcs1); // Noncompliant
            genericRSA.TryEncrypt(data, null, RSAEncryptionPadding.Pkcs1, out var bytesWritten2); // Noncompliant
        }
    }
}
