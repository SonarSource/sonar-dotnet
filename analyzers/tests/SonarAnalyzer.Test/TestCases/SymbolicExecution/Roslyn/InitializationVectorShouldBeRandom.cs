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
            var constantVector = sa.CreateEncryptor(sa.Key, initializationVectorConstant); // Noncompliant  {{Use a dynamically-generated, random IV.}}
//                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

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

            sa.IV = new byte[16];
            sa.CreateEncryptor(sa.Key, sa.IV); // Noncompliant

            ClassWithStaticProperty.Count = 10; // To catch AD0001 coming from asking the instance from a static property.
        }
    }

    public void CallEncryptorAgainWithoutInput()
    {
        var initializationVectorConstant = new byte[16];

        using (SymmetricAlgorithm sa = SymmetricAlgorithm.Create("AES"))
        {
            sa.CreateEncryptor(sa.Key, initializationVectorConstant); // Noncompliant
            sa.CreateEncryptor(); // Compliant - CreateEncryptor will generate an IV internally with GenerateIV method
        }
    }

    public void CallEncryptorWithIVProperty(bool condition)
    {
        var initializationVectorConstant = new byte[16];

        SymmetricAlgorithm sa = SymmetricAlgorithm.Create("AES");
        SymmetricAlgorithm sa2 = SymmetricAlgorithm.Create("AES");

        sa2.IV = new byte[16];
        sa.CreateEncryptor(sa.Key, sa2.IV); // Noncompliant

        var x = sa2.IV;
        var y = x;
        sa.CreateEncryptor(sa.Key, x); // Noncompliant
        sa.CreateEncryptor(sa.Key, y); // Noncompliant

        sa.CreateEncryptor(sa.Key, (condition ? sa2 : sa2).IV);  // Noncompliant

    }

    public void InstanceFromExpression(bool condition)
    {
        var initializationVectorConstant = new byte[16];

        using (SymmetricAlgorithm sa = SymmetricAlgorithm.Create("AES"))
        {
            (condition ? sa : sa).IV = initializationVectorConstant;
            sa.CreateEncryptor(); // Noncompliant
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

    public void UsingRNGCryptoServiceProviderIsCompliant(bool condition)
    {
        var initializationVectorConstant = new byte[16];
        var initializationVectorRng = new byte[16];
        var initializationVectorRngNonZero = new byte[16];
        var initializationVectorInExpression = new byte[16];

        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(initializationVectorRng);
            rng.GetNonZeroBytes(initializationVectorRngNonZero);

            rng.GetBytes((condition ? initializationVectorInExpression : initializationVectorInExpression));
        }

        using (var sa = SymmetricAlgorithm.Create("AES"))
        {

            sa.GenerateKey();
            var fromRng = sa.CreateEncryptor(sa.Key, initializationVectorRng);
            var fromRngNonZero = sa.CreateEncryptor(sa.Key, initializationVectorRngNonZero);
            var fromRandomGeneratorWithExpression = sa.CreateEncryptor(sa.Key, initializationVectorInExpression);

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
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, initializationVectorConstants); // Noncompliant
            encryptor = aes.CreateEncryptor(aes.Key, initializationVector); // Noncompliant
        }
    }

    public void ImplicitlyTypedArrayWithNewWithConstantsInside()
    {
        var initializationVectorConstants = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        var initializationVector = new byte[] { Rnd() };
        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, initializationVectorConstants); // Noncompliant
            encryptor = aes.CreateEncryptor(aes.Key, initializationVector); // Noncompliant
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

    public void CollectionInitializer()
    {
        List<byte> listWithConstant = new List<byte> { 0x00 };
        List<byte> list = new List<byte> { Rnd() };
        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, listWithConstant.ToArray()); // FN
            encryptor = aes.CreateEncryptor(aes.Key, list.ToArray()); // FN
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
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, anonymous.IV); // FN https://github.com/SonarSource/sonar-dotnet/issues/4555
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

public class ClassWithStaticProperty
{
    public static int Count { get; set; }
}

public class ReproAD0001
{
    private static void EncryptBytes(CustomAlg customAlg)
    {
        AesManaged aes = new AesManaged();
        aes.Key = customAlg.Key;
        aes.IV = customAlg.IV; // Repro for AD0001
        aes.CreateEncryptor(customAlg.Key, customAlg.IV);

        var customIV = new byte[16];
        var rng = new RNGCryptoServiceProvider();
        rng.GetBytes(customIV);

        aes.IV = customIV;
        aes.CreateEncryptor();
    }
}
