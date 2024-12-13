/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
public class ClassShouldNotBeEmptyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ClassShouldNotBeEmpty>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ClassShouldNotBeEmpty>();

    [TestMethod]
    public void ClassShouldNotBeEmpty_CS() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.cs")
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_VB() =>
        builderVB
            .AddPaths("ClassShouldNotBeEmpty.vb")
            .Verify();

#if NET

    private static readonly MetadataReference[] AdditionalReferences =
    [
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcRazorPages
    ];

    [TestMethod]
    public void ClassShouldNotBeEmpty_CSharp9() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.CSharp9.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_CSharp10() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_CSharp12() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.CSharp12.cs")
            .WithOptions(LanguageOptions.FromCSharp12)
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_Inheritance_CS() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.Inheritance.cs")
            .AddReferences(AdditionalReferences)
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_Inheritance_VB() =>
        builderVB
            .AddPaths("ClassShouldNotBeEmpty.Inheritance.vb")
            .AddReferences(AdditionalReferences)
            .Verify();

#endif
}
