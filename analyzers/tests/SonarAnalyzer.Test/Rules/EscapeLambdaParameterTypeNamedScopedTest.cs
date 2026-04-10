/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class EscapeLambdaParameterTypeNamedScopedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<EscapeLambdaParameterTypeNamedScoped>();

    [TestMethod]
    public void EscapeLambdaParameterTypeNamedScoped_TopLevelStatements_CSharp11_13() =>
        builder.AddPaths("EscapeLambdaParameterTypeNamedScoped.TopLevelStatements.CSharp11-13.cs")
            .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp11, LanguageVersion.CSharp13))
            .WithTopLevelStatements()
            .VerifyNoIssues();

    [TestMethod]
    public void EscapeLambdaParameterTypeNamedScoped_CSharp11_13() =>
        builder.AddPaths("EscapeLambdaParameterTypeNamedScoped.CSharp11-13.cs")
            .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp11, LanguageVersion.CSharp13))
            .Verify();

    [TestMethod]
    public void EscapeLambdaParameterTypeNamedScoped_TopLevelStatements_Latest() =>
        builder.AddPaths("EscapeLambdaParameterTypeNamedScoped.TopLevelStatements.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithTopLevelStatements()
            .VerifyNoIssues();

    [TestMethod]
    public void EscapeLambdaParameterTypeNamedScoped_Latest() =>
        builder.AddPaths("EscapeLambdaParameterTypeNamedScoped.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
}
