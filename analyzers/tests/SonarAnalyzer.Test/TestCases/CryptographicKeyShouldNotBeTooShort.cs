using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class Program
    {
        private const int validKeySizeConst = 2048;
        private const int invalidKeySizeConst = 1024;

        private static readonly int validKeySize = 2048;
        private static readonly int invalidKeySize = 1024;

        public void ConstArgumentResolution()
        {
            const int localValidSize = 2048;
            new RSACryptoServiceProvider(); // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
            new RSACryptoServiceProvider(new CspParameters()); // Noncompliant - has default key size of 1024
            new RSACryptoServiceProvider(2048);
            new RSACryptoServiceProvider(localValidSize);
            new RSACryptoServiceProvider(validKeySizeConst);
            new RSACryptoServiceProvider(validKeySize);
            new RSACryptoServiceProvider(invalidKeySize); // Noncompliant

            const int localInvalidSize = 1024;
            new RSACryptoServiceProvider(1024); // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            new RSACryptoServiceProvider(1024, new CspParameters()); // Noncompliant
            new RSACryptoServiceProvider(invalidKeySizeConst); // Noncompliant
            new RSACryptoServiceProvider(localInvalidSize); // Noncompliant

            // https://github.com/SonarSource/sonar-dotnet/issues/6341
            new RSACryptoServiceProvider { PersistKeyInCsp = false }; // Noncompliant
        }
    }

    public class SystemSecurityCryptographyDSA
    {
        public void DefaultConstructors(CspParameters cspParameters)
        {
            var dsa1 = new DSACng(); // Compliant - default is 2048
            var dsa2 = new DSACryptoServiceProvider(); // Noncompliant - default is 1024
            var dsa3 = new DSACryptoServiceProvider(cspParameters); // Noncompliant - default is 1024
            var dsa4 = new DSAOpenSsl(); // Compliant - default is 2048
        }

        public void CompliantKeySizeConstructors()
        {
            var dsa1 = new DSACng(3072);
            var dsa2 = new DSAOpenSsl(2048);
        }

        public void NonCompliantKeySizeConstructors(CspParameters parameters)
        {
            var dsa1 = new DSACng(512); // Noncompliant {{Use a key length of at least 2048 bits for DSA cipher algorithm.}}
            var dsa2 = new DSACryptoServiceProvider(512); // Noncompliant {{Use a key length of at least 2048 bits for DSA cipher algorithm.}}
            var dsa3 = new DSACryptoServiceProvider(1024, parameters); // Noncompliant
            var dsa4 = new DSACryptoServiceProvider(3072); // Noncompliant - this is not valid
            var dsa5 = new DSACryptoServiceProvider(2048, parameters); // Noncompliant - this is not valid
            var dsa6 = new DSAOpenSsl(1024); // Noncompliant
        }

        public void CompliantKeySizeSet()
        {
            var dsa1 = new DSACng();
            dsa1.KeySize = 3072;

            var dsa2 = new DSAOpenSsl();
            dsa2.KeySize = 3072;
        }

        public void NoncompliantKeySizeSet()
        {
            var dsa1 = new DSACng();
            dsa1.KeySize = 512; // Noncompliant {{Use a key length of at least 2048 bits for DSA cipher algorithm.}}

            var dsa2 = new DSACryptoServiceProvider(); // Noncompliant
            dsa2.KeySize = 2048; // Noncompliant {{Use a key length of at least 2048 bits for DSA cipher algorithm. This assignment does not update the underlying key size.}}
//          ^^^^^^^^^^^^^^^^^^^
            var dsa3 = new DSAOpenSsl();
            dsa3.KeySize = 1024; // Noncompliant
        }

        public void CompliantCreate()
        {
            var dsa1 = DSA.Create();
            var dsa2 = DSA.Create(2048);
            var dsa3 = DSA.Create(3072);
        }

        public void NoncompliantCreate()
        {
            var dsa1 = DSA.Create(512); // Noncompliant {{Use a key length of at least 2048 bits for DSA cipher algorithm.}}
            var dsa2 = DSA.Create(1024); // Noncompliant {{Use a key length of at least 2048 bits for DSA cipher algorithm.}}
//                     ^^^^^^^^^^^^^^^^
        }
    }

    public class SystemSecurityCryptographyRSA
    {

        public void DefaultConstructors()
        {
            var rsa1 = new RSACng(); // Compliant - default is 2048
            var rsa2 = new RSACryptoServiceProvider(); // Noncompliant - default is 1024
            var rsa3 = new RSAOpenSsl(); // Compliant - default is 2048
        }

        public void CompliantKeySizeConstructors(CspParameters parameters)
        {
            var rsa1 = new RSACng(2048);
            var rsa2 = new RSACryptoServiceProvider(2048);
            var rsa3 = new RSACryptoServiceProvider(3072, parameters);
            var rsa4 = new RSAOpenSsl(3072);
        }

        public void NonCompliantKeySizeConstructors(CspParameters parameters)
        {
            var rsa1 = new RSACng(1024); // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
            var rsa2 = new RSACryptoServiceProvider(512); // Noncompliant
            var rsa3 = new RSACryptoServiceProvider(1024, parameters); // Noncompliant
            var rsa4 = new RSAOpenSsl(512); // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
        }

        public void CompliantKeySizeSet()
        {
            var rsa1 = new RSACng();
            rsa1.KeySize = 2048;

            var rsa2 = new RSAOpenSsl();
            rsa2.KeySize = 3072;
        }

        public void NoncompliantKeySizeSet()
        {
            var rsa1 = new RSACng();
            rsa1.KeySize = 1024; // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}

            var rsa3 = new RSACryptoServiceProvider(); // Noncompliant
            rsa3.KeySize = 2048; // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm. This assignment does not update the underlying key size.}}

            var rsa4 = new RSAOpenSsl();
            rsa4.KeySize = 512; // Noncompliant
        }

        public void CompliantCreate()
        {
            var rsa1 = RSA.Create();
            var rsa2 = RSA.Create(2048);
            var rsa3 = RSA.Create(3072);
        }

        public void NoncompliantCreate()
        {
            var rsa1 = RSA.Create(512); // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
            var rsa2 = RSA.Create(1024); // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
        }
    }

    public class SystemSecurityCryptographyEC
    {
        public void CompliantConstructors()
        {
            var ec1 = new ECDiffieHellmanCng(); // Compliant - default EC key size is 521
            var ec2 = new ECDsaCng(); // Compliant - default EC key size is 521
            var ec3 = new ECDsaOpenSsl(); // Compliant - default EC key size is 521

            // Valid key sizes are 256, 384, and 521 bits.
            var ec4 = new ECDiffieHellmanCng(256);
            var ec5 = new ECDsaCng(384);
            var ec6 = new ECDsaOpenSsl(521);
            var ec7 = new ECDsaOpenSsl((IntPtr)128);

            var ec8 = new ECDiffieHellmanCng(ECCurve.NamedCurves.brainpoolP384r1);
            var ec9 = new ECDsaCng(ECCurve.NamedCurves.brainpoolP384t1);
            var ec10 = new ECDsaOpenSsl(ECCurve.NamedCurves.brainpoolP512r1);
        }

        public void NonCompliantConstructors()
        {
            var ec1 = new ECDiffieHellmanCng(ECCurve.NamedCurves.brainpoolP192r1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
            var ec2 = new ECDsaCng(ECCurve.NamedCurves.brainpoolP160t1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
            var ec3 = new ECDsaOpenSsl(ECCurve.NamedCurves.brainpoolP192t1); // Noncompliant
        }

        public void CompliantKeySizeSet()
        {
            var ec1 = new ECDiffieHellmanCng();
            ec1.KeySize = 512;
            ec1.KeySize = 128; // OK - because this is not a valid key size for this object

            var ec2 = new ECDsaCng();
            ec2.KeySize = 512;
            ec2.KeySize = 64; // OK - because this is not a valid key size for this object

            var ec3 = new ECDsaOpenSsl();
            ec3.KeySize = 512;
            ec3.KeySize = 12; // OK - because this is not a valid key size for this object
        }

        public void CompliantGenerateKey()
        {
            var ec1 = new ECDiffieHellmanCng();
            ec1.GenerateKey(ECCurve.NamedCurves.brainpoolP224r1);
            ec1.GenerateKey(ECCurve.NamedCurves.nistP256);

            var ec2 = new ECDsaCng();
            ec2.GenerateKey(ECCurve.NamedCurves.brainpoolP256r1);
            ec2.GenerateKey(ECCurve.NamedCurves.nistP384);

            var ec3 = new ECDsaOpenSsl();
            ec3.GenerateKey(ECCurve.NamedCurves.brainpoolP384t1);
            ec3.GenerateKey(ECCurve.NamedCurves.nistP521);
        }

        public void NoncompliantGenerateKey()
        {
            var ec1 = new ECDiffieHellmanCng();
            ec1.GenerateKey(ECCurve.NamedCurves.brainpoolP160r1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}

            var ec2 = new ECDsaCng();
            ec2.GenerateKey(ECCurve.NamedCurves.brainpoolP160t1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
        }

        public void CompliantCreate()
        {
            var ec1 = ECDiffieHellman.Create();
            var ec2 = ECDiffieHellman.Create(ECCurve.NamedCurves.brainpoolP256r1);
            var ec3 = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

            var ec4 = ECDsa.Create();
            var ec5 = ECDsa.Create(ECCurve.NamedCurves.brainpoolP384t1);
            var ec6 = ECDsa.Create(ECCurve.NamedCurves.nistP521);
        }

        public void NoncompliantCreate()
        {
            var ec1 = ECDiffieHellman.Create(ECCurve.NamedCurves.brainpoolP192t1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
            var ec2 = ECDsa.Create(ECCurve.NamedCurves.brainpoolP160r1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
        }
    }

    public class SystemSecurityCryptographyAES
    {
        public void CompliantAES()
        {
            AesManaged aes1 = new AesManaged
            {
                KeySize = 128, // Compliant
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            AesCryptoServiceProvider aes2 = new AesCryptoServiceProvider();
            aes2.KeySize = 128; // Compliant

            AesCng aes3 = new AesCng();
            aes3.KeySize = 128; // Compliant

            AesManaged aes4 = new AesManaged
            {
                KeySize = 64, // OK - ignore as it is not possible to create AES instance with smaller than 128 key size
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            AesCryptoServiceProvider aes5 = new AesCryptoServiceProvider();
            aes5.KeySize = 64; // OK - ignore as it is not possible to create AES instance with smaller than 128 key size

            AesCng aes6 = new AesCng();
            aes6.KeySize = 64; // OK - ignore as it is not possible to create AES instance with smaller than 128 key size
        }
    }

    public class BouncyCastleCryptography
    {
        public void CompliantParametersGenerator()
        {
            var pGen1 = new DHParametersGenerator();
            pGen1.Init(2048, 10, new SecureRandom()); // Compliant

            var pGen2 = new DsaParametersGenerator();
            pGen2.Init(2048, 80, new SecureRandom()); // Compliant

            var kp1 = new ECKeyPairGenerator(); // OK - ignore for now

            var kp2 = new DsaKeyPairGenerator();
            var d2 = new DsaParameters(new BigInteger("2"), new BigInteger("2"), new BigInteger("2")); // FN
            var r2 = new DsaKeyGenerationParameters(new SecureRandom(), d2);
            kp2.Init(r2);

            var kp3 = new RsaKeyPairGenerator();
            var r3 = new RsaKeyGenerationParameters(new BigInteger("2"), new SecureRandom(), 2048, 5); // Compliant
            kp3.Init(r3);
        }

        public void NoncompliantParametersGenerator(int arg)
        {
            var pGen1 = new DHParametersGenerator();
            pGen1.Init(1024, 10, new SecureRandom()); // Noncompliant {{Use a key length of at least 2048 bits for DH cipher algorithm.}}

            var pGen2 = new DsaParametersGenerator();
            pGen2.Init(1024, 80, new SecureRandom()); // Noncompliant {{Use a key length of at least 2048 bits for DSA cipher algorithm.}}

            var kp3 = new RsaKeyPairGenerator();
            var r3 = new RsaKeyGenerationParameters(new BigInteger("1"), new SecureRandom(), 1024, 5); // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
            kp3.Init(r3);

            pGen1.Init(arg, 10, new SecureRandom());     // Compliant
            var keySize = 4096;
            pGen1.Init(keySize, 10, new SecureRandom()); // Compliant
            keySize = 1024;
            pGen1.Init(keySize, 10, new SecureRandom()); // Noncompliant
        }

        public void CompliantGetByName()
        {
            X9ECParameters curve = null;

            curve = SecNamedCurves.GetByName("secp224k1"); // Compliant
            curve = SecNamedCurves.GetByName("secp224r1"); // Compliant
            curve = SecNamedCurves.GetByName("secp256k1"); // Compliant
            curve = SecNamedCurves.GetByName("secp256r1"); // Compliant
            curve = SecNamedCurves.GetByName("secp384r1"); // Compliant
            curve = SecNamedCurves.GetByName("secp521r1"); // Compliant
            curve = SecNamedCurves.GetByName("sect233k1"); // Compliant
            curve = SecNamedCurves.GetByName("sect233r1"); // Compliant
            curve = SecNamedCurves.GetByName("sect239k1"); // Compliant
            curve = SecNamedCurves.GetByName("sect283k1"); // Compliant
            curve = SecNamedCurves.GetByName("sect283r1"); // Compliant
            curve = SecNamedCurves.GetByName("sect409k1"); // Compliant
            curve = SecNamedCurves.GetByName("sect409r1"); // Compliant
            curve = SecNamedCurves.GetByName("sect571k1"); // Compliant
            curve = SecNamedCurves.GetByName("sect571r1"); // Compliant

            curve = X962NamedCurves.GetByName("prime239v1"); // Compliant
            curve = X962NamedCurves.GetByName("prime239v2"); // Compliant
            curve = X962NamedCurves.GetByName("prime239v3"); // Compliant
            curve = X962NamedCurves.GetByName("prime256v1"); // Compliant
            curve = X962NamedCurves.GetByName("c2tnb239v1"); // Compliant
            curve = X962NamedCurves.GetByName("c2tnb239v2"); // Compliant
            curve = X962NamedCurves.GetByName("c2tnb239v3"); // Compliant
            curve = X962NamedCurves.GetByName("c2pnb272w1"); // Compliant
            curve = X962NamedCurves.GetByName("c2pnb304w1"); // Compliant
            curve = X962NamedCurves.GetByName("c2tnb359v1"); // Compliant
            curve = X962NamedCurves.GetByName("c2pnb368w1"); // Compliant
            curve = X962NamedCurves.GetByName("c2tnb431r1"); // Compliant

            curve = TeleTrusTNamedCurves.GetByName("brainpoolp224r1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp224t1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp256r1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp256t1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp320r1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp320t1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp384r1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp384t1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp512r1"); // Compliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp512t1"); // Compliant

            curve = SecNamedCurves.GetByName("GostR3410-2001-CryptoPro-A"); // Compliant
            curve = SecNamedCurves.GetByName("GostR3410-2001-CryptoPro-XchB"); // Compliant
            curve = SecNamedCurves.GetByName("GostR3410-2001-CryptoPro-XchA"); // Compliant
            curve = SecNamedCurves.GetByName("GostR3410-2001-CryptoPro-C"); // Compliant
            curve = SecNamedCurves.GetByName("GostR3410-2001-CryptoPro-B"); // Compliant

            curve = NistNamedCurves.GetByName("B-233"); // Compliant
            curve = NistNamedCurves.GetByName("B-283"); // Compliant
            curve = NistNamedCurves.GetByName("B-409"); // Compliant
            curve = NistNamedCurves.GetByName("B-571"); // Compliant
            curve = NistNamedCurves.GetByName("K-233"); // Compliant
            curve = NistNamedCurves.GetByName("K-283"); // Compliant
            curve = NistNamedCurves.GetByName("K-409"); // Compliant
            curve = NistNamedCurves.GetByName("K-571"); // Compliant
            curve = NistNamedCurves.GetByName("P-224"); // Compliant
            curve = NistNamedCurves.GetByName("P-256"); // Compliant
            curve = NistNamedCurves.GetByName("P-384"); // Compliant
            curve = NistNamedCurves.GetByName("P-521"); // Compliant

            curve = ECNamedCurveTable.GetByName("prime239v1"); // Compliant
            curve = ECNamedCurveTable.GetByName("secp224k1"); // Compliant
            curve = ECNamedCurveTable.GetByName("c2tnb239v1"); // Compliant
            curve = ECNamedCurveTable.GetByName("brainpoolp320t1"); // Compliant
            curve = ECNamedCurveTable.GetByName("GostR3410-2001-CryptoPro-XchA"); // Compliant
            curve = ECNamedCurveTable.GetByName("P-521"); // Compliant
            curve = ECNamedCurveTable.GetByName("RandomString"); // Compliant
        }

        public void NoncompliantGetByName(string arg)
        {
            X9ECParameters curve = null;

            // Elliptic curves always have the key length as part of their names. Rule implementation looks for this
            // key length pattern, so that all curves with a key length smaller than 224 will raise an issue
            curve = SecNamedCurves.GetByName("secp192k1"); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
            curve = SecNamedCurves.GetByName("secp192r1"); // Noncompliant
            curve = SecNamedCurves.GetByName("sect163k1"); // Noncompliant
            curve = SecNamedCurves.GetByName("sect163r1"); // Noncompliant
            curve = SecNamedCurves.GetByName("sect163r2"); // Noncompliant
            curve = SecNamedCurves.GetByName("sect193r1"); // Noncompliant
            curve = SecNamedCurves.GetByName("sect193r2"); // Noncompliant

            curve = X962NamedCurves.GetByName("prime192v1"); // Noncompliant
            curve = X962NamedCurves.GetByName("prime192v2"); // Noncompliant
            curve = X962NamedCurves.GetByName("prime192v3"); // Noncompliant
            curve = X962NamedCurves.GetByName("c2pnb163v1"); // Noncompliant
            curve = X962NamedCurves.GetByName("c2pnb163v2"); // Noncompliant
            curve = X962NamedCurves.GetByName("c2pnb163v3"); // Noncompliant
            curve = X962NamedCurves.GetByName("c2pnb176w1"); // Noncompliant
            curve = X962NamedCurves.GetByName("c2tnb191v1"); // Noncompliant
            curve = X962NamedCurves.GetByName("c2tnb191v2"); // Noncompliant
            curve = X962NamedCurves.GetByName("c2tnb191v3"); // Noncompliant
            curve = X962NamedCurves.GetByName("c2pnb208w1"); // Noncompliant

            curve = TeleTrusTNamedCurves.GetByName("brainpoolp160r1"); // Noncompliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp160t1"); // Noncompliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp192r1"); // Noncompliant
            curve = TeleTrusTNamedCurves.GetByName("brainpoolp192t1"); // Noncompliant

            curve = NistNamedCurves.GetByName("B-163"); // Noncompliant
            curve = NistNamedCurves.GetByName("K-163"); // Noncompliant
            curve = NistNamedCurves.GetByName("P-192"); // Noncompliant

            curve = ECNamedCurveTable.GetByName("secp192k1"); // Noncompliant
            curve = ECNamedCurveTable.GetByName("c2pnb208w1"); // Noncompliant
            curve = ECNamedCurveTable.GetByName("brainpoolp192t1"); // Noncompliant
            curve = ECNamedCurveTable.GetByName("B-163"); // Noncompliant

            ECNamedCurveTable.GetByName(arg);       // Compliant
            var variable = "RandomString";
            ECNamedCurveTable.GetByName(variable);  // Compliant
            variable = "B-163";
            ECNamedCurveTable.GetByName(variable);  // Noncompliant
        }
    }

    public class CustomCryptography : RSA
    {
        public void UseCompliantKeyLength()
        {
            this.KeySize = 2048;
        }

        public void UseNoncompliantKeyLength()
        {
            this.KeySize = 1024; // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
            KeySize = 1024; // FN - we only look at MemberAccessExpressionSyntax for simplicity
        }

        public override RSAParameters ExportParameters(bool includePrivateParameters)
        {
            throw new NotImplementedException();
        }

        public override void ImportParameters(RSAParameters parameters)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomClassWithSameMethods
    {
        public int KeySize { get; set; }

        public void Create() { }
        public void Create(int value) { }
        public void Create(ECCurve value) { }
        public void Create(string value) { }
        public void GenerateKey() { }
        public void GenerateKey(ECCurve value) { }
        public void GenerateKey(string value) { }
        public void GetByName() { }
        public void GetByName(string value) { }
        public void GetByName(bool value) { }
        public void Init() { }
        public void Init(int value) { }
        public void Init(string value) { }
        public void AnotherMethod() { }

        public void Test()
        {
            this.Create();
            this.Create(64);
            this.Create(ECCurve.NamedCurves.brainpoolP160t1);
            this.Create("brainpoolP160t1");

            this.GenerateKey();
            this.GenerateKey(ECCurve.NamedCurves.brainpoolP160t1);
            this.GenerateKey("brainpoolP160t1");

            this.GetByName();
            this.GetByName("brainpoolP160t1");
            this.GetByName(true);

            this.Init();
            this.Init(64);
            this.Init("64");

            this.AnotherMethod();
            var KeySize = 64;
            this.KeySize = 64;
        }
    }
}
