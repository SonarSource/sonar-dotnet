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
public class ReturnTypeNamedPartialShouldBeEscapedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ReturnTypeNamedPartialShouldBeEscaped>();

    [TestMethod]
    public void ReturnTypeNamedPartialShouldBeEscaped_CSharp8_13() =>
        builder.AddPaths("ReturnTypeNamedPartialShouldBeEscaped.CSharp8-13.cs")
            .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp8, LanguageVersion.CSharp13))
            .Verify();

    [TestMethod]
    public void ReturnTypeNamedPartialShouldBeEscaped_TopLevelStatements_CSharp9_13() =>
        builder.AddPaths("ReturnTypeNamedPartialShouldBeEscaped.TopLevelStatements.CSharp9-13.cs")
            .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp9, LanguageVersion.CSharp13))
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void ReturnTypeNamedPartialShouldBeEscaped_TopLevelStatements_Latest() =>
        builder.AddPaths("ReturnTypeNamedPartialShouldBeEscaped.TopLevelStatements.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void ReturnTypeNamedPartialShouldBeEscaped_CS_Latest() =>
        builder.AddPaths("ReturnTypeNamedPartialShouldBeEscaped.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
}
