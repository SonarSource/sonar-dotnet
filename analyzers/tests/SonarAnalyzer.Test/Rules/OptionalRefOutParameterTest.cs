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
public class OptionalRefOutParameterTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<OptionalRefOutParameter>();

    [TestMethod]
    public void OptionalRefOutParameter() =>
        builder.AddPaths("OptionalRefOutParameter.cs").Verify();

    [TestMethod]
    public void OptionalRefOutParameter_TopLevelStatements() =>
        builder.AddPaths("OptionalRefOutParameter.TopLevelStatements.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void OptionalRefOutParameter_CS_Latest() =>
        builder.AddPaths("OptionalRefOutParameter.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void OptionalRefOutParameter_CodeFix() =>
        builder.WithCodeFix<OptionalRefOutParameterCodeFix>()
            .AddPaths("OptionalRefOutParameter.cs")
            .WithCodeFixedPaths("OptionalRefOutParameter.Fixed.cs")
            .VerifyCodeFix();
}
