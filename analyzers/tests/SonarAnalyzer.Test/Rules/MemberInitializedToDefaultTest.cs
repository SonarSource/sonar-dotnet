/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class MemberInitializedToDefaultTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<MemberInitializedToDefault>();

    [TestMethod]
    public void MemberInitializedToDefault() =>
        builder.AddPaths("MemberInitializedToDefault.cs").Verify();

    [TestMethod]
    public void MemberInitializedToDefault_CodeFix() =>
        builder
        .WithCodeFix<MemberInitializedToDefaultCodeFix>()
        .AddPaths("MemberInitializedToDefault.cs")
        .WithCodeFixedPaths("MemberInitializedToDefault.Fixed.cs")
        .VerifyCodeFix();

    [TestMethod]
    public void MemberInitializedToDefault_CSharp8() =>
        builder.AddPaths("MemberInitializedToDefault.CSharp8.cs").WithOptions(LanguageOptions.FromCSharp8).VerifyNoIssues();

    [TestMethod]
    public void MemberInitializedToDefault_CSharp9() =>
        builder.AddPaths("MemberInitializedToDefault.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

    [TestMethod]
    public void MemberInitializedToDefault_CSharp10() =>
        builder.AddPaths("MemberInitializedToDefault.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

    [TestMethod]
    public void MemberInitializedToDefault_CSharp11() =>
        builder.AddPaths("MemberInitializedToDefault.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

    [TestMethod]
    public void MemberInitializedToDefault_CSharp11_CodeFix() =>
        builder
        .WithCodeFix<MemberInitializedToDefaultCodeFix>()
        .AddPaths("MemberInitializedToDefault.CSharp11.cs")
        .WithCodeFixedPaths("MemberInitializedToDefault.CSharp11.Fixed.cs")
        .WithOptions(LanguageOptions.FromCSharp11)
        .VerifyCodeFix();
}
