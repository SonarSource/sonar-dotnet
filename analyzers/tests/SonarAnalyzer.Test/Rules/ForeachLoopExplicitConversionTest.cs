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
public class ForeachLoopExplicitConversionTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ForeachLoopExplicitConversion>();

    [TestMethod]
    public void ForeachLoopExplicitConversion() =>
        builder.AddPaths("ForeachLoopExplicitConversion.cs").Verify();

    [TestMethod]
    public void ForeachLoopExplicitConversion_CodeFix() =>
        builder.WithCodeFix<ForeachLoopExplicitConversionCodeFix>()
            .AddPaths("ForeachLoopExplicitConversion.cs")
            .WithCodeFixedPaths("ForeachLoopExplicitConversion.Fixed.cs")
            .VerifyCodeFix();
#if NET

    [TestMethod]
    public void ForeachLoopExplicitConversion_CSharpLatest() =>
        builder.AddPaths("ForeachLoopExplicitConversion.Latest.cs")
            .WithAutogenerateConcurrentFiles(false)
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void ForeachLoopExplicitConversion_CSharpLatest_CodeFix() =>
        builder.WithCodeFix<ForeachLoopExplicitConversionCodeFix>()
            .AddPaths("ForeachLoopExplicitConversion.Latest.cs")
            .WithCodeFixedPaths("ForeachLoopExplicitConversion.Latest.Fixed.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .VerifyCodeFix();

#endif

}
