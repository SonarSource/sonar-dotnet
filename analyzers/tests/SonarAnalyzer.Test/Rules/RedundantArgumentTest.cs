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
public class RedundantArgumentTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<RedundantArgument>();
    private readonly VerifierBuilder codeFixBuilder = new VerifierBuilder<RedundantArgument>().WithCodeFix<RedundantArgumentCodeFix>();

    [TestMethod]
    public void RedundantArgument() =>
        builder.AddPaths("RedundantArgument.cs").Verify();

    [TestMethod]
    public void RedundantArgument_TopLevelStatements() =>
        builder.AddPaths("RedundantArgument.TopLevelStatements.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void RedundantArgument_Latest() =>
        builder.AddPaths("RedundantArgument.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void RedundantArgument_CodeFix_No_Named_Arguments() =>
        codeFixBuilder.AddPaths("RedundantArgument.cs").WithCodeFixedPaths("RedundantArgument.NoNamed.Fixed.cs").WithCodeFixTitle(RedundantArgumentCodeFix.TitleRemove).VerifyCodeFix();

    [TestMethod]
    public void RedundantArgument_CodeFix_Named_Arguments() =>
        codeFixBuilder.AddPaths("RedundantArgument.cs").WithCodeFixedPaths("RedundantArgument.Named.Fixed.cs").WithCodeFixTitle(RedundantArgumentCodeFix.TitleRemoveWithNameAdditions).VerifyCodeFix();
}
