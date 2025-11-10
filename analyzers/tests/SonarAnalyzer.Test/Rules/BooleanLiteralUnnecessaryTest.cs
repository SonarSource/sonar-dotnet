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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class BooleanLiteralUnnecessaryTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BooleanLiteralUnnecessary>();

    [TestMethod]
    public void BooleanLiteralUnnecessary_CS() =>
        builderCS.AddPaths("BooleanLiteralUnnecessary.cs").Verify();

    [TestMethod]
    public void BooleanLiteralUnnecessary_CodeFix_CS() =>
        builderCS.AddPaths("BooleanLiteralUnnecessary.cs")
            .WithCodeFix<CS.BooleanLiteralUnnecessaryCodeFix>()
            .WithCodeFixedPaths("BooleanLiteralUnnecessary.Fixed.cs")
            .VerifyCodeFix();

#if NET
    [TestMethod]
    public void BooleanLiteralUnnecessary_Latest() =>
        builderCS.AddPaths("BooleanLiteralUnnecessary.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void BooleanLiteralUnnecessary_CodeFix_Latest() =>
        builderCS.AddPaths("BooleanLiteralUnnecessary.Latest.cs")
            .WithCodeFix<CS.BooleanLiteralUnnecessaryCodeFix>()
            .WithCodeFixedPaths("BooleanLiteralUnnecessary.Latest.Fixed.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .VerifyCodeFix();

#endif

    [TestMethod]
    public void BooleanLiteralUnnecessary_VB() =>
        new VerifierBuilder<VB.BooleanLiteralUnnecessary>().AddPaths("BooleanLiteralUnnecessary.vb").Verify();
}
