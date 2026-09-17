using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class RSASignaturePaddingTest
    {
        void SignAndVerify(byte[] data, byte[] signature, RSA genericRSA, RSASignaturePadding padding)
        {
            using var rsaCsp = new RSACryptoServiceProvider();
            using var rsaCng = new RSACng();
            using var rsaCreate = RSA.Create();

            rsaCsp.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Noncompliant {{Use secure mode and padding scheme. RSACryptoServiceProvider does not support RSASSA-PSS, switch to RSACng.}}
            rsaCsp.SignHash(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Noncompliant
            rsaCsp.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss); // Compliant, not Pkcs1

            rsaCng.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Noncompliant {{Use secure mode and padding scheme.}}
            rsaCng.SignHash(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Noncompliant
            rsaCng.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss); // Compliant, not Pkcs1

            rsaCreate.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Noncompliant {{Use secure mode and padding scheme.}}

            genericRSA.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Noncompliant

            rsaCng.SignData(data, HashAlgorithmName.SHA256, padding); // Compliant, we don't know which padding is actually used here

            // Verifiers can't choose the padding scheme - it has to match whatever the signer used - so this is not actionable.
            rsaCsp.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Compliant
            rsaCng.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Compliant
            genericRSA.VerifyHash(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Compliant
        }

        void SignWithConditionalAccess(byte[] data, RSACryptoServiceProvider nullableCsp) =>
            nullableCsp?.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Noncompliant
    }
}
