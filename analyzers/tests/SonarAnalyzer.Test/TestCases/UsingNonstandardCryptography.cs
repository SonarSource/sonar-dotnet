using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class CustomHashAlgorithm : HashAlgorithm  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^
    {
        public override void Initialize() => throw new NotImplementedException();

        protected override void HashCore(byte[] array, int ibStart, int cbSize) => throw new NotImplementedException();

        protected override byte[] HashFinal() => throw new NotImplementedException();
    }

    public class CustomCryptoTransform : ICryptoTransform  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^^^
    {
        public int InputBlockSize => throw new NotImplementedException();

        public int OutputBlockSize => throw new NotImplementedException();

        public bool CanTransformMultipleBlocks => throw new NotImplementedException();

        public bool CanReuseTransform => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) => throw new NotImplementedException();

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) => throw new NotImplementedException();
    }

    public interface ICustomCryptoTransform : ICryptoTransform  // Noncompliant {{Use a standard cryptographic algorithm.}}
//                   ^^^^^^^^^^^^^^^^^^^^^^
    {
    }

    public class CustomCryptoTransformWithInterface : ICustomCryptoTransform  // Noncompliant {{Use a standard cryptographic algorithm.}}
    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public int InputBlockSize => throw new NotImplementedException();

        public int OutputBlockSize => throw new NotImplementedException();

        public bool CanTransformMultipleBlocks => throw new NotImplementedException();

        public bool CanReuseTransform => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) => throw new NotImplementedException();

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) => throw new NotImplementedException();
    }

    internal class CustomDerivebytes : DeriveBytes  // Noncompliant {{Use a standard cryptographic algorithm.}}
//                 ^^^^^^^^^^^^^^^^^
    {
        public override byte[] GetBytes(int cb) => throw new NotImplementedException();

        public override void Reset() => throw new NotImplementedException();

        private class CustomSymmetricAlgorithm : SymmetricAlgorithm  // Noncompliant {{Use a standard cryptographic algorithm.}}
//                    ^^^^^^^^^^^^^^^^^^^^^^^^
        {
            public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) => throw new NotImplementedException();

            public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) => throw new NotImplementedException();

            public override void GenerateIV() => throw new NotImplementedException();

            public override void GenerateKey() => throw new NotImplementedException();
        }
    }

    public class CustomAsymmetricAlgorithm : AsymmetricAlgorithm  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^
    {
    }

    public class DerivedClassFromCustomAsymmetricAlgorithm : CustomAsymmetricAlgorithm  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
    }

    public class CustomAsymmetricKeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override string Parameters { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override byte[] DecryptKeyExchange(byte[] rgb) => throw new NotImplementedException();

        public override void SetKey(AsymmetricAlgorithm key) => throw new NotImplementedException();
    }

    public class CustomAsymmetricKeyExchangeFormatter : AsymmetricKeyExchangeFormatter  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override string Parameters => throw new NotImplementedException();

        public override byte[] CreateKeyExchange(byte[] data) => throw new NotImplementedException();

        public override byte[] CreateKeyExchange(byte[] data, Type symAlgType) => throw new NotImplementedException();

        public override void SetKey(AsymmetricAlgorithm key) => throw new NotImplementedException();
    }

    public class CustomAsymmetricSignatureDeformatter : AsymmetricSignatureDeformatter  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override void SetHashAlgorithm(string strName) => throw new NotImplementedException();

        public override void SetKey(AsymmetricAlgorithm key) => throw new NotImplementedException();

        public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature) => throw new NotImplementedException();
    }

    public class CustomAsymmetricSignatureFormatter : AsymmetricSignatureFormatter  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override byte[] CreateSignature(byte[] rgbHash) => throw new NotImplementedException();

        public override void SetHashAlgorithm(string strName) => throw new NotImplementedException();

        public override void SetKey(AsymmetricAlgorithm key) => throw new NotImplementedException();
    }

    public class CustomKeyedHashAlgorithm : KeyedHashAlgorithm  // Noncompliant {{Use a standard cryptographic algorithm.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override void Initialize() => throw new NotImplementedException();

        protected override void HashCore(byte[] array, int ibStart, int cbSize) => throw new NotImplementedException();

        protected override byte[] HashFinal() => throw new NotImplementedException();
    }

    public struct CustomCryptoTransformStruct : ICryptoTransform  // Noncompliant
    {
        public int InputBlockSize => throw new NotImplementedException();

        public int OutputBlockSize => throw new NotImplementedException();

        public bool CanTransformMultipleBlocks => throw new NotImplementedException();

        public bool CanReuseTransform => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) => throw new NotImplementedException();

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) => throw new NotImplementedException();
    }
}

public class ClassThatDoesNotInheritOrImplementAnything // Compliant
{
}

public interface InterfaceThatDoesNotInheritOrImplementAnything // Compliant
{
}

public interface InterfaceThatDoesNotInheritCryptographic : IDisposable // Compliant
{
}

public sealed class ClassThatDoesNotInheritCryptographic : IDisposable // Compliant
{
    public void Dispose() => throw new NotImplementedException();
}
