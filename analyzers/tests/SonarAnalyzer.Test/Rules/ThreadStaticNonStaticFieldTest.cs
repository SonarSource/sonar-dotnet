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
public class ThreadStaticNonStaticFieldTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ThreadStaticNonStaticField>();

    [TestMethod]
    public void ThreadStaticNonStaticField() =>
        builder.AddPaths("ThreadStaticNonStaticField.cs").Verify();

    [TestMethod]
    public void ThreadStaticNonStaticField_Latest() =>
        builder.AddPaths("ThreadStaticNonStaticField.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void ThreadStaticNonStaticField_CodeFix() =>
        builder.WithCodeFix<ThreadStaticNonStaticFieldCodeFix>()
            .AddPaths("ThreadStaticNonStaticField.cs")
            .WithCodeFixedPaths("ThreadStaticNonStaticField.Fixed.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void ThreadStaticNonStaticField_Latest_CodeFix() =>
        builder.WithCodeFix<ThreadStaticNonStaticFieldCodeFix>()
            .AddPaths("ThreadStaticNonStaticField.Latest.cs")
            .WithCodeFixedPaths("ThreadStaticNonStaticField.Latest.Fixed.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .VerifyCodeFix();
}
