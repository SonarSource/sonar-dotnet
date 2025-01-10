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
public class UsingNonstandardCryptographyTest
{
    private readonly VerifierBuilder builderCS = CreateBuilder().AddAnalyzer(() => new CS.UsingNonstandardCryptography(AnalyzerConfiguration.AlwaysEnabled));
    private readonly VerifierBuilder builderVB = CreateBuilder().AddAnalyzer(() => new VB.UsingNonstandardCryptography(AnalyzerConfiguration.AlwaysEnabled));

    [TestMethod]
    public void UsingNonstandardCryptography_CS() =>
        builderCS.AddPaths("UsingNonstandardCryptography.cs").Verify();

#if NET

    [TestMethod]
    public void UsingNonstandardCryptography_CSharp9() =>
        builderCS.AddPaths("UsingNonstandardCryptography.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

    [TestMethod]
    public void UsingNonstandardCryptography_CSharp10() =>
        builderCS.AddPaths("UsingNonstandardCryptography.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

    [TestMethod]
    public void UsingNonstandardCryptography_CSharp12() =>
        builderCS.AddPaths("UsingNonstandardCryptography.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).Verify();

#endif

    [TestMethod]
    public void UsingNonstandardCryptography_VB() =>
        builderVB.AddPaths("UsingNonstandardCryptography.vb").Verify();

    private static VerifierBuilder CreateBuilder() =>
        new VerifierBuilder().AddReferences(MetadataReferenceFacade.SystemSecurityCryptography).WithBasePath("Hotspots");
}
