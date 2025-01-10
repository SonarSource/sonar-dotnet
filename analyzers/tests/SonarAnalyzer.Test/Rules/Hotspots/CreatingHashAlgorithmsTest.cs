/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CreatingHashAlgorithmsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder().WithBasePath("Hotspots")
        .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
        .AddAnalyzer(() => new CS.CreatingHashAlgorithms(AnalyzerConfiguration.AlwaysEnabled));

    private readonly VerifierBuilder builderVB = new VerifierBuilder().WithBasePath("Hotspots")
        .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
        .AddAnalyzer(() => new VB.CreatingHashAlgorithms(AnalyzerConfiguration.AlwaysEnabled));

    [TestMethod]
    public void CreatingHashAlgorithms_CSharp8() =>
        builderCS.AddPaths("CreatingHashAlgorithms.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void CreatingHashAlgorithms_VB() =>
        builderVB.AddPaths("CreatingHashAlgorithms.vb").Verify();

#if NETFRAMEWORK // HMACRIPEMD160, MD5Cng, RIPEMD160Managed and RIPEMD160 are available only for .Net Framework

    [TestMethod]
    public void CreatingHashAlgorithms_CS_NetFx() =>
        builderCS.AddPaths("CreatingHashAlgorithms.NetFramework.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void CreatingHashAlgorithms_VB_NetFx() =>
        builderVB.AddPaths("CreatingHashAlgorithms.NetFramework.vb").Verify();

#endif

#if NET

    [TestMethod]
    public void CreatingHashAlgorithms_CS_Latest() =>
        builderCS.AddPaths("CreatingHashAlgorithms.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

#endif

}
