using System;
using System.Collections.Generic;
using System.Security.Cryptography;

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
            var constantVector = sa.CreateEncryptor(sa.Key, initializationVectorConstant); // // FIXME Non-compliant  {{Use a dynamically-generated, random IV.}}
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

            var ivReplacedDefaultConstructor = sa.CreateEncryptor(); // FIXME Non-compliant
            var ivReplaced = sa.CreateEncryptor(sa.Key, sa.IV); // FIXME Non-compliant
        }
    }

    public void RandomIsNotCompliant()
    {
        var initializationVectorWeakBytes = new byte[16];
        new Random().NextBytes(initializationVectorWeakBytes);

        var sa = SymmetricAlgorithm.Create("AES");
        var encryptor = sa.CreateEncryptor(sa.Key, initializationVectorWeakBytes); // FIXME Non-compliant
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
        sa.CreateEncryptor(sa.Key, initializationVectorWeakFor); // FIXME Non-compliant
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

        using (var sa = SymmetricAlgorithm.Create("AES"))
        {

            sa.GenerateKey();
            var fromRng = sa.CreateEncryptor(sa.Key, initializationVectorRng);
            var fromRngNonZero = sa.CreateEncryptor(sa.Key, initializationVectorRngNonZero);

            sa.GenerateIV();
            var fromGenerateIV = sa.CreateEncryptor(sa.Key, sa.IV);

            sa.CreateDecryptor(sa.Key, initializationVectorConstant); // Compliant, not relevant for decrypting
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4274
    public void ImplicitlyTypedArrayWithoutNew()
    {
        byte[] initializationVectorConstants = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] initializationVector = { Rnd(), Rnd(), Rnd() };
        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, initializationVectorConstants); // FIXME Non-compliant
            encryptor = aes.CreateEncryptor(aes.Key, initializationVector);
        }
    }

    public void ImplicitlyTypedArrayWithNewWithConstantsInside()
    {
        var initializationVectorConstants = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        var initializationVector = new byte[] { Rnd() };
        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, initializationVectorConstants); // FIXME Non-compliant
            encryptor = aes.CreateEncryptor(aes.Key, initializationVector);
        }
    }

    public void ImplicitlyTypedArrayNonByte()
    {
        int[] intArray = { 1, 2, 3 };
        byte[] byteArray = new byte[intArray.Length * sizeof(int)];
        Buffer.BlockCopy(intArray, 0, byteArray, 0, byteArray.Length);
        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, byteArray); // FIXME Non-compliant
        }
    }

    public void CollectionInitializer()
    {
        List<byte> listWithConstant = new List<byte> { 0x00 };
        List<byte> list = new List<byte> { Rnd() };
        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, listWithConstant.ToArray()); // FIXME Non-compliant
            encryptor = aes.CreateEncryptor(aes.Key, list.ToArray()); // FIXME Non-compliant
        }
    }

    public void InsideObjectInitializer()
    {
        var anonymous = new
        {
            IV = new byte[] { 0x00 },
            Key = new byte[] { 0x00 }
        };
        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, anonymous.IV); // FIXME Non-compliant https://github.com/SonarSource/sonar-dotnet/issues/4555
        }
    }

    public void DifferentCases()
    {
        var alg = new CustomAlg();
        alg.IV = new byte[16];
    }

    private byte Rnd()
    {
        var rand = new Random();
        var bytes = new byte[1];
        rand.NextBytes(bytes);
        return bytes[0];
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
