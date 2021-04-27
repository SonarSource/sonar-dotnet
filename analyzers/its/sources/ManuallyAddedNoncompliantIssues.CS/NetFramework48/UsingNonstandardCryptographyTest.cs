/*
 * SonarQube, open source software quality management tool.
 * Copyright (C) 2008-2020 SonarSource
 * mailto:contact AT sonarsource DOT com
 *
 * SonarQube is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * SonarQube is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
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
