using System;
using System.Security.Cryptography;

public abstract class CustomEllipticCurveKeyAgreement : ECDiffieHellman { } // Noncompliant {{Use a standard cryptographic algorithm.}}
public abstract class CustomX25519KeyAgreement : X25519DiffieHellman { }    // FN NET-4992

public class StandardX25519KeyAgreementUser                                 // Compliant
{
    public X25519DiffieHellman CreateKey() => X25519DiffieHellman.GenerateKey();
}

public abstract class CustomKeyEncapsulation : MLKem                        // FN NET-4992
{
    protected CustomKeyEncapsulation() : base(MLKemAlgorithm.MLKem768) { }
}

public union CustomTransformUnion(int, string) : ICryptoTransform          // FN NET-4993
{
    public int InputBlockSize => throw new NotImplementedException();
    public int OutputBlockSize => throw new NotImplementedException();
    public bool CanTransformMultipleBlocks => throw new NotImplementedException();
    public bool CanReuseTransform => throw new NotImplementedException();
    public void Dispose() => throw new NotImplementedException();
    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) => throw new NotImplementedException();
    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) => throw new NotImplementedException();
}

public union DisposableUnion(int, string) : IDisposable                    // Compliant
{
    public void Dispose() { }
}
