﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CryptographicKeyShouldNotBeTooShortTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CryptographicKeyShouldNotBeTooShort>().AddReferences(GetAdditionalReferences());

        [TestMethod]
        public void CryptographicKeyShouldNotBeTooShort() =>
            builder.AddPaths("CryptographicKeyShouldNotBeTooShort.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NETFRAMEWORK

        [TestMethod]
        public void CryptographicKeyShouldNotBeTooShort_NetFramework() =>
            builder.AddPaths("CryptographicKeyShouldNotBeTooShort.BeforeNet7.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#else

        [TestMethod]
        public void CryptographicKeyShouldNotBeTooShort_CSharp9() =>
            builder.AddPaths("CryptographicKeyShouldNotBeTooShort.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

#endif

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            MetadataReferenceFacade.SystemSecurityCryptography

#if NETFRAMEWORK

                .Concat(NuGetMetadataReference.SystemSecurityCryptographyOpenSsl())

#endif

                .Concat(NuGetMetadataReference.BouncyCastle());
    }
}
