/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System;
using System.Security.Cryptography;

namespace NetFramework48
{
    public class UsingNonstandardCryptographyTest
    {
        private readonly CustomHashAlgorithm hashAlgorithm;

        public UsingNonstandardCryptographyTest()
        {
            hashAlgorithm = new CustomHashAlgorithm();
        }

        private class CustomHashAlgorithm : HashAlgorithm // Noncompliant (S2257)
        {
            public override void Initialize() => throw new NotImplementedException();

            protected override void HashCore(byte[] array, int ibStart, int cbSize) => throw new NotImplementedException();

            protected override byte[] HashFinal() => throw new NotImplementedException();
        }
    }

    public interface ICustomCryptoTransform : ICryptoTransform  // Noncompliant
    {
    }

    public class CustomCryptoTransform : ICryptoTransform  // Noncompliant
    {
        public int InputBlockSize => throw new NotImplementedException();

        public int OutputBlockSize => throw new NotImplementedException();

        public bool CanTransformMultipleBlocks => throw new NotImplementedException();

        public bool CanReuseTransform => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) => throw new NotImplementedException();

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) => throw new NotImplementedException();
    }

    public class CustomAsymmetricAlgorithm : AsymmetricAlgorithm  // Noncompliant
    {
    }
}
