using System;
using System.Security.Cryptography;

var constantIV = new byte[16];

using var aes = new AesCng();
using var rng = new RNGCryptoServiceProvider();

aes.CreateEncryptor(); // FN
aes.CreateEncryptor(aes.Key, constantIV); // FN

aes.GenerateIV();
aes.CreateEncryptor();
aes.CreateEncryptor(aes.Key, aes.IV);

void TopLevelLocalFunction()
{
    using var aes = new AesCng();
    aes.CreateEncryptor(); // FN
    aes.GenerateIV();
    aes.CreateEncryptor();
}

public class Sample
{
    public void TargetTypedNew()
    {
        AesCng aes = new();
        aes.CreateEncryptor(); // FN
        aes.GenerateIV();
        aes.CreateEncryptor();
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            AesCng aes = new AesCng();
            aes.CreateEncryptor(); // Noncompliant
        };
        a();
    }

    public int Property
    {
        get => 42;
        init
        {
            AesCng aes = new AesCng();
            aes.CreateEncryptor(); // FN
            aes.GenerateIV();
            aes.CreateEncryptor();
        }
    }
}

public record Record
{
    public void Method()
    {
        AesCng aes = new AesCng();
        aes.CreateEncryptor(); // Noncompliant
        aes.GenerateIV();
        aes.CreateEncryptor();
    }
}

public partial class Partial
{
    public partial void Method();
}

public partial class Partial
{
    public partial void Method()
    {
        AesCng aes = new AesCng();
        aes.CreateEncryptor(); // Noncompliant
        aes.GenerateIV();
        aes.CreateEncryptor();
    }
}

namespace TartetTypedConditional
{
    public class Sample
    {
        public void Go(bool condition)
        {
            SymmetricAlgorithm aes = condition ? new AesCng() : new AesCryptoServiceProvider();
            aes.CreateEncryptor();  // Noncompliant
            aes.GenerateIV();
            aes.CreateEncryptor();
        }
    }
}
