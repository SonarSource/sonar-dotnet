using System;
using System.Security.Cryptography;

public abstract class CustomEllipticCurveKeyAgreement : ECDiffieHellman { } // Noncompliant {{Use a standard cryptographic algorithm.}}
public abstract class CustomX25519KeyAgreement : X25519DiffieHellman { }    // Noncompliant NET-4992
public abstract class DerivedCustomX25519KeyAgreement : CustomX25519KeyAgreement { } // Noncompliant

public class StandardX25519KeyAgreementUser                                 // Compliant
{
    public X25519DiffieHellman CreateKey() => X25519DiffieHellman.GenerateKey();
}

public abstract class CustomKeyEncapsulation : MLKem                        // Noncompliant NET-4992
{
    protected CustomKeyEncapsulation() : base(MLKemAlgorithm.MLKem768) { }
}

public class StandardKeyEncapsulationUser                                   // Compliant
{
    public MLKem CreateKey() => MLKem.GenerateKey(MLKemAlgorithm.MLKem768);
}

public abstract class CustomSignature : MLDsa                               // Noncompliant
{
    protected CustomSignature() : base(MLDsaAlgorithm.MLDsa65) { }
}

public class StandardSignatureUser                                          // Compliant
{
    public MLDsa CreateKey() => MLDsa.GenerateKey(MLDsaAlgorithm.MLDsa65);
}

#pragma warning disable SYSLIB5006
public abstract class CustomHashBasedSignature : SlhDsa                     // Noncompliant
{
    protected CustomHashBasedSignature(SlhDsaAlgorithm algorithm) : base(algorithm) { }
}

public abstract class CustomCompositeKeyEncapsulation : CompositeMLKem     // Noncompliant
{
    protected CustomCompositeKeyEncapsulation(CompositeMLKemAlgorithm algorithm) : base(algorithm) { }
}

public abstract class CustomCompositeSignature : CompositeMLDsa            // Noncompliant
{
    protected CustomCompositeSignature(CompositeMLDsaAlgorithm algorithm) : base(algorithm) { }
}

public class StandardPostQuantumUser                                        // Compliant
{
    public SlhDsa HashBasedSignature { get; set; }
    public CompositeMLKem CompositeKeyEncapsulation { get; set; }
    public CompositeMLDsa CompositeSignature { get; set; }
}
#pragma warning restore SYSLIB5006

#pragma warning disable SYSLIB5009
public abstract class CustomHybridEncryption : Hpke                         // Noncompliant
{
    protected CustomHybridEncryption(HpkeSuite suite) : base(suite) { }
}

public abstract class CustomHybridEncryptionSender : HpkeSender             // Noncompliant
{
    protected CustomHybridEncryptionSender(HpkeSuite suite) : base(suite) { }
}

public abstract class CustomHybridEncryptionRecipient : HpkeRecipient       // Noncompliant
{
    protected CustomHybridEncryptionRecipient(HpkeSuite suite) : base(suite) { }
}

public class StandardHybridEncryptionUser                                   // Compliant
{
    public Hpke HybridEncryption { get; set; }
    public HpkeSender Sender { get; set; }
    public HpkeRecipient Recipient { get; set; }
}
#pragma warning restore SYSLIB5009

public union CustomTransformUnion(int, string) : ICryptoTransform          // Noncompliant NET-4993
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
