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
                ICryptoTransform noParams = sa.CreateEncryptor(); // Compliant - IV is automatically generated
                var defaultKeyAndIV = sa.CreateEncryptor(sa.Key, sa.IV); // Compliant

                sa.GenerateKey();
                var generateIVNotCalled = sa.CreateEncryptor(sa.Key, sa.IV);
                var constantVector = sa.CreateEncryptor(sa.Key, initializationVectorConstant); // Noncompliant {{Use a dynamically-generated, random IV.}}
//                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                sa.GenerateIV();
                var defaultConstructor = sa.CreateEncryptor(); // Compliant
                var compliant = sa.CreateEncryptor(sa.Key, sa.IV);

                sa.KeySize = 12;
                sa.CreateEncryptor(); // Compliant - updating key size does not change the status
                sa.CreateEncryptor(sa.Key, new CustomAlg().IV);
                sa.CreateEncryptor(sa.Key, new CustomAlg().Key);

                sa.IV = initializationVectorConstant;
                sa.GenerateKey();

                var ivReplacedDefaultConstructor = sa.CreateEncryptor(); // Noncompliant
                var ivReplaced = sa.CreateEncryptor(sa.Key, sa.IV); // Noncompliant
            }
        }

        public void RandomIsNotCompliant()
        {
            var initializationVectorWeakBytes = new byte[16];
            new Random().NextBytes(initializationVectorWeakBytes);

            var sa = SymmetricAlgorithm.Create("AES");
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

        public void UsingRandomNumberGeneratorIsCompliant()
        {
            var initializationVectorConstant = new byte[16];
            var initializationVectorRng = new byte[16];
            var initializationVectorRngNonZero = new byte[16];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(initializationVectorRng);
            rng.GetNonZeroBytes(initializationVectorRngNonZero);

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

            var noParams = aes.CreateEncryptor(); // Compliant

            var constantIV = new byte[16];
            var withConstant = aes.CreateEncryptor(aes.Key, constantIV); // Noncompliant

            aes.GenerateKey();
            aes.GenerateIV();
            aes.CreateEncryptor();
            var withGeneratedKeyAndIV = aes.CreateEncryptor(aes.Key, aes.IV);

            aes.CreateDecryptor(aes.Key, constantIV); // Compliant, we do not check CreateDecryptor

            rng.GetBytes(constantIV);
            var fromRng = aes.CreateEncryptor(aes.Key, constantIV);
        }

        public void AesCreateEncryptor()
        {
            using var aes = Aes.Create();
            using var rng = new RNGCryptoServiceProvider();

            var noParams = aes.CreateEncryptor(); // Compliant

            aes.GenerateKey();
            var reGeneratedKey = aes.CreateEncryptor(); // Compliant

            var constantIV = new byte[16];
            var withConstant = aes.CreateEncryptor(aes.Key, constantIV); // Noncompliant

            aes.GenerateIV();
            aes.CreateEncryptor();
            var withGeneratedKeyAndIV = aes.CreateEncryptor(aes.Key, aes.IV);

            aes.CreateDecryptor(aes.Key, constantIV); // Compliant, we do not check CreateDecryptor

            rng.GetBytes(constantIV);
            var fromRng = aes.CreateEncryptor(aes.Key, constantIV);
        }

        public void AesCngCreateEncryptor()
        {
            using var aes = new AesCng();
            using var rng = new RNGCryptoServiceProvider();

            var noParams = aes.CreateEncryptor(); // Compliant

            aes.GenerateKey();
            var withGeneratedKey = aes.CreateEncryptor(); // Compliant

            var constantIV = new byte[16];
            var withConstant = aes.CreateEncryptor(aes.Key, constantIV); // Noncompliant

            aes.GenerateIV();
            aes.CreateEncryptor();
            var withGeneratedKeyAndIV = aes.CreateEncryptor(aes.Key, aes.IV);

            aes.CreateDecryptor(aes.Key, constantIV); // Compliant, we do not check CreateDecryptor

            rng.GetBytes(constantIV);
            var fromRng = aes.CreateEncryptor(aes.Key, constantIV);
        }

        public void CustomImplementationOfAes()
        {
            using var aes = new CustomAes();
            using var rng = new RNGCryptoServiceProvider();

            var noParams = aes.CreateEncryptor(); // Compliant

            aes.GenerateKey();
            var withGeneratedKey = aes.CreateEncryptor(); // Compliant

            var constantIV = new byte[16];
            var withConstant = aes.CreateEncryptor(aes.Key, constantIV); // Noncompliant

            aes.GenerateIV();
            aes.CreateEncryptor();
            var withGeneratedKeyAndIV = aes.CreateEncryptor(aes.Key, aes.IV);

            aes.CreateDecryptor(aes.Key, constantIV); // Compliant, we do not check CreateDecryptor

            rng.GetBytes(constantIV);
            var fromRng = aes.CreateEncryptor(aes.Key, constantIV);
        }

        public void InConditionals(int a)
        {
            var constantIV = new byte[16];

            using var aes = new AesCng();
            using var rng = new RNGCryptoServiceProvider();

            var e = a switch
            {
                1 => aes.CreateEncryptor(), // Compliant
                2 => aes.CreateEncryptor(aes.Key, constantIV), // Noncompliant
                _ => null
            };

            var iv = new byte[16];

            using var aes2 = new AesCng();
            if (a == 1)
            {
                aes2.IV = iv; // Set IV to constant
            }
            aes2.CreateEncryptor(); // Noncompliant

            var aes3 = a == 2 ? aes2 : aes;
            aes3.CreateEncryptor(); // Noncompliant
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/4274
        public void ImplicitlyTypedArrayWithoutNew()
        {
            byte[] initializationVector =
            {
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
            };
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, initializationVector); //Noncompliant
            }
        }

        public void ImplicitlyTypedArrayWithNew()
        {
            var initializationVector = new byte[]
            {
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
            };
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, initializationVector); //Noncompliant
            }
        }

        public void ImplicitlyTypedArrayNonByte()
        {
            int[] intArray = { 1, 2, 3 };
            byte[] byteArray = new byte[intArray.Length * sizeof(int)];
            Buffer.BlockCopy(intArray, 0, byteArray, 0, byteArray.Length);
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, byteArray); // Noncompliant
            }
        }

        public void DifferentCases()
        {
            var alg = new CustomAlg();
            alg.IV = new byte[16];
        }
    }

    public class CodeWhichDoesNotCompile
    {
        public void Check()
        {
            var initializationVectorConstant = new byte[16];

            using (FakeUnresolvedType sa = SymmetricAlgorithm.Create("AES")) // Error [CS0246]
            {
                sa.IV = initializationVectorConstant;
            }
        }
    }

    public class CustomAlg
    {
        public virtual byte[] IV { get; set; }

        public virtual byte[] Key { get; set; }
    }

    public class CustomAes : Aes
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) => throw new NotImplementedException();

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) => throw new NotImplementedException();

        public override void GenerateIV() => throw new NotImplementedException();

        public override void GenerateKey() => throw new NotImplementedException();
    }

    public class SymmetricalEncryptorWrapper
    {
        private readonly SymmetricAlgorithm algorithm;

        public SymmetricalEncryptorWrapper()
        {
            algorithm = Aes.Create();
        }

        public void GenerateIV()
        {
            algorithm.GenerateIV();
        }

        public ICryptoTransform CreateEncryptor()
        {
            return algorithm.CreateEncryptor();
        }
    }
}
