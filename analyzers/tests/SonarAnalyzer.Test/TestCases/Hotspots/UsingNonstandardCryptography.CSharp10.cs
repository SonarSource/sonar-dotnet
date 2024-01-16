using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public record struct CustomCryptoTransform : ICryptoTransform  // Noncompliant
    {
        public int InputBlockSize => throw new NotImplementedException();

        public int OutputBlockSize => throw new NotImplementedException();

        public bool CanTransformMultipleBlocks => throw new NotImplementedException();

        public bool CanReuseTransform => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) => throw new NotImplementedException();

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) => throw new NotImplementedException();
    }

    public interface ICustomCryptoTransform : ICryptoTransform  // Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
    {
    }

    public record struct CustomCryptoTransformWithInterface : ICustomCryptoTransform  // Noncompliant
    {
        public int InputBlockSize => throw new NotImplementedException();

        public int OutputBlockSize => throw new NotImplementedException();

        public bool CanTransformMultipleBlocks => throw new NotImplementedException();

        public bool CanReuseTransform => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) => throw new NotImplementedException();

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) => throw new NotImplementedException();
    }

    public record struct CustomCryptoTransformWithInterfacePositionalRecordStruct(string Property) : ICustomCryptoTransform  // Noncompliant
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
