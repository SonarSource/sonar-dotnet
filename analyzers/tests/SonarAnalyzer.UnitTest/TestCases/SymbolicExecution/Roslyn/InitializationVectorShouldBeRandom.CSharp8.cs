using System;
using System.Collections.Generic;
using System.Security.Cryptography;

class InitializationVectorShouldBeRandom
{
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
        aes3.CreateEncryptor();

        var aes4 = true ? aes2 : aes;
        aes4.CreateEncryptor(); // Noncompliant
    }
}

public class CustomAes : Aes
{
    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) => throw new NotImplementedException();

    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) => throw new NotImplementedException();

    public override void GenerateIV() => throw new NotImplementedException();

    public override void GenerateKey() => throw new NotImplementedException();
}
