using System;
using System.Security.Cryptography;

class CustomHashAlgorithmUsingPrimaryConstructor(byte[] array) : HashAlgorithm  // Noncompliant
{
    public override void Initialize() => throw new NotImplementedException();

    protected override void HashCore(byte[] array, int ibStart, int cbSize) => throw new NotImplementedException();

    protected override byte[] HashFinal() => throw new NotImplementedException();
}

class CustomAsymmetricAlgorithm(byte[] array) : AsymmetricAlgorithm { } // Noncompliant

class DerivedFromCustomAsymmetricAlgorithm(byte[] array) : CustomAsymmetricAlgorithm(array) { } // Noncompliant
