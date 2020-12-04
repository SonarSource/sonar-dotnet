using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class InitializationVectorShouldBeRandom
    {
        public void SymetricAlgorithmCreateEncryptor()
        {
            var initializationVectorConstant = new byte[16];

            using (SymmetricAlgorithm sa = SymmetricAlgorithm.Create("AES"))
            {
                ICryptoTransform notInitialized = sa.CreateEncryptor(); // Noncompliant {{Use a dynamically-generated, random IV.}}
//                                                ^^^^^^^^^^^^^^^^^^^^

                var keyAndIVareNotInitialized = sa.CreateEncryptor(sa.Key, sa.IV); // Noncompliant - key and iv are not generated
//                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                sa.GenerateKey();
                var ivIsNotInitiazed = sa.CreateEncryptor(sa.Key, sa.IV); // Noncompliant - iv is not generated
                var constantVector = sa.CreateEncryptor(sa.Key, initializationVectorConstant); // Noncompliant

                sa.GenerateIV();
                var defaultConstructor = sa.CreateEncryptor(); // Compliant - key and IV are used by default and now they are generated
                var compliant = sa.CreateEncryptor(sa.Key, sa.IV);

                sa.IV = initializationVectorConstant;
                var ivReplacedDefaultConstructor = sa.CreateEncryptor(); // Noncompliant
                var ivReplaced = sa.CreateEncryptor(sa.Key, sa.IV); // Noncompliant
            }
        }

        public void RandomIsNotCompliant()
        {
            var initializationVectorWeakBytes = new byte[16];
            new Random().NextBytes(initializationVectorWeakBytes);

            var sa = SymmetricAlgorithm.Create("AES");
            sa.GenerateKey();
            sa.GenerateIV();

            var encryptor = sa.CreateEncryptor(sa.Key, initializationVectorWeakBytes); // Noncompliant
        }

        public void CustomGenerationNotCompliant()
        {
            var initializationVectorWeakFor = new byte[16];

            var rnd = new Random();
            for (var i = 0; i < initializationVectorWeakFor.Length; i++)
            {
                initializationVectorWeakFor[i] = (byte)(rnd.Next() % 256);
            }

            var sa = SymmetricAlgorithm.Create("AES");
            sa.GenerateKey();
            sa.GenerateIV();

            sa.CreateEncryptor(sa.Key, initializationVectorWeakFor); // Noncompliant
        }

        public void UsingRNGCryptoServiceProviderIsCompliant()
        {
            var initializationVectorConstant = new byte[16];
            var initializationVectorRng = new byte[16];
            var initializationVectorRngNonZero = new byte[16];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(initializationVectorRng);
                rng.GetNonZeroBytes(initializationVectorRngNonZero);
            }

            using var sa = SymmetricAlgorithm.Create("AES");

            sa.GenerateKey();
            var fromRng = sa.CreateEncryptor(sa.Key, initializationVectorRng);
            var fromRngNonZero = sa.CreateEncryptor(sa.Key, initializationVectorRngNonZero);

            sa.GenerateIV();
            var fromGenerateIV = sa.CreateEncryptor(sa.Key, sa.IV);

            sa.CreateDecryptor(sa.Key, initializationVectorConstant); // Compliant, not relevant for decrypting
        }

        public void AesCryptoServiceProviderCreateEncryptor()
        {
            using var aes = new AesCryptoServiceProvider();
            using var rng = new RNGCryptoServiceProvider();

            var noParamsNoKeyAndNoIV = aes.CreateEncryptor(); // Noncompliant

            aes.GenerateKey();
            var noParamsNoIV = aes.CreateEncryptor(); // Noncompliant

            var constantIV = new byte[16];
            var withConstant = aes.CreateEncryptor(aes.Key, constantIV); // Noncompliant

            aes.GenerateIV();
            var noParams = aes.CreateEncryptor();
            var withGeneratedKeyAndIV = aes.CreateEncryptor(aes.Key, aes.IV);

            aes.CreateDecryptor(aes.Key, constantIV); // Compliant, we do not check CreateDecryptor

            rng.GetBytes(constantIV);
            var fromRng = aes.CreateEncryptor(aes.Key, constantIV);
        }
    }
}
