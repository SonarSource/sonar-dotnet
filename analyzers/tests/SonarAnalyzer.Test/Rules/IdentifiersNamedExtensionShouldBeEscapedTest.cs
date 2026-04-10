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
public class IdentifiersNamedExtensionShouldBeEscapedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<IdentifiersNamedExtensionShouldBeEscaped>();

    [TestMethod]
    public void IdentifiersNamedExtensionShouldBeEscaped_BeforeCSharp14() =>
        builder.AddPaths("IdentifiersNamedExtensionShouldBeEscaped.BeforeCSharp14.cs")
            .WithOptions(LanguageOptions.BeforeCSharp14)
            .Verify();

    [TestMethod]
    public void IdentifiersNamedExtensionShouldBeEscaped_Latest() =>
        builder.AddPaths("IdentifiersNamedExtensionShouldBeEscaped.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    // Type declarations are tested via parameterized tests to allow version-specific ranges
    // (record struct requires C# 10+) and to keep the test case files focused on member-level cases.
    // '$$' marks the position where '@' must be inserted to make the declaration compliant.
    public static IEnumerable<object[]> TypeDeclarations() =>
    [
        ["""class $$extension { }"""],
        ["""struct $$extension { }"""],
        ["""interface $$extension { }"""],
        ["""enum $$extension { }"""],
        ["""delegate void $$extension();"""],
        ["""class MyClass<$$extension> { }"""],
        ["""using $$extension = System.Object;"""],
        ["""record $$extension { }"""],
        ["""record struct $$extension { }"""],
    ];

    [TestMethod]
    [DynamicData(nameof(TypeDeclarations))]
    public void IdentifiersNamedExtensionShouldBeEscaped_TypeDeclarations_CSharp10ToCSharp13(string declaration) =>
        builder
            .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp10, LanguageVersion.CSharp13))
            .AddSnippet($"{declaration.Replace("$$", string.Empty)} // Noncompliant")
            .Verify();

    [TestMethod]
    [DynamicData(nameof(TypeDeclarations))]
    public void IdentifiersNamedExtensionShouldBeEscaped_TypeDeclarations_Latest(string declaration) =>
        builder
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddSnippet($"{declaration.Replace("$$", string.Empty)} // Error [CS9306]")
            .Verify();

    [TestMethod]
    [DynamicData(nameof(TypeDeclarations))]
    public void IdentifiersNamedExtensionShouldBeEscaped_TypeDeclarations_Compliant_CSharp10ToCSharp13(string declaration) =>
        builder
            .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp10, LanguageVersion.CSharp13))
            .AddSnippet(declaration.Replace("$$", "@"))
            .VerifyNoIssues();

    [TestMethod]
    [DynamicData(nameof(TypeDeclarations))]
    public void IdentifiersNamedExtensionShouldBeEscaped_TypeDeclarations_Compliant_Latest(string declaration) =>
        builder
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddSnippet(declaration.Replace("$$", "@"))
            .VerifyNoIssues();
}
