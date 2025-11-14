/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CryptographicKeyShouldNotBeTooShortTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<CryptographicKeyShouldNotBeTooShort>().AddReferences(GetAdditionalReferences());

    [TestMethod]
    public void CryptographicKeyShouldNotBeTooShort() =>
        builder.AddPaths("CryptographicKeyShouldNotBeTooShort.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

#if NETFRAMEWORK

    [TestMethod]
    public void CryptographicKeyShouldNotBeTooShort_NetFramework() =>
        builder.AddPaths("CryptographicKeyShouldNotBeTooShort.BeforeNet7.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

#else

    [TestMethod]
    public void CryptographicKeyShouldNotBeTooShort_CS_Latest() =>
        builder.AddPaths("CryptographicKeyShouldNotBeTooShort.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
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
