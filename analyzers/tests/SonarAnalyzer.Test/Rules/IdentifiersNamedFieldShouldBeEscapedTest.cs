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
public class IdentifiersNamedFieldShouldBeEscapedTest
{
    private readonly VerifierBuilder builderCSharp9To13 = new VerifierBuilder<IdentifiersNamedFieldShouldBeEscaped>()
        .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp9, LanguageVersion.CSharp13))
        .WithWarningsAsErrors("CS9258"); // Does not fire in C# 9-13 — keep here so the test fails if it ever gets activated.

    private readonly VerifierBuilder builderLatest = new VerifierBuilder<IdentifiersNamedFieldShouldBeEscaped>()
        .WithOptions(LanguageOptions.CSharpLatest);

    [TestMethod]
    public void IdentifiersNamedFieldShouldBeEscaped_CSharp9_13() =>
        builderCSharp9To13.AddPaths("IdentifiersNamedFieldShouldBeEscaped.CSharp9-13.cs").Verify();

    [TestMethod]
    public void IdentifiersNamedFieldShouldBeEscaped_Latest() =>
        builderLatest.AddPaths("IdentifiersNamedFieldShouldBeEscaped.Latest.cs")
            .WithWarningsAsErrors("CS9258")
            .Verify();

    [TestMethod]
    public void IdentifiersNamedFieldShouldBeEscaped_BeforeCSharp14() =>
        new VerifierBuilder<IdentifiersNamedFieldShouldBeEscaped>()
            .WithOptions(LanguageOptions.BeforeCSharp14)
            .AddPaths("IdentifiersNamedFieldShouldBeEscaped.BeforeCSharp14.cs")
            .AddPaths("IdentifiersNamedFieldShouldBeEscaped.BeforeCSharp14.partial.cs")
            .WithAutogenerateConcurrentFiles(false)
            .Verify();

    // Declaration forms that raise CS9273 in C# 14 — S8367 should flag these in C# 9-13.
    public static IEnumerable<object[]> CS9273LocalDeclarationForms() =>
    [
        // Variable declarations
        ["""int x = 0, field = 0;"""],
        ["""using var field = new System.IO.MemoryStream();"""],
        ["""using (var field = new System.IO.MemoryStream()) { }"""],
        // foreach
        ["""foreach (var field in new int[0]) { }"""],
        ["""foreach (var (field, _) in new (int, int)[0]) { }"""],
        // catch
        ["""try { } catch (Exception field) { }"""],
        // Pattern and deconstruction variables
        ["""int.TryParse("", out var field);"""],
        ["""var (field, _) = (0, 0);"""],
        ["""(int field, int _) = (0, 0);"""],
        // LINQ range variables
        ["""var q = from field in new int[0] select 0;"""],
        ["""var q = from x in new int[0] let field = 0 select x;"""],
        ["""var q = from x in new int[0] join field in new int[0] on x equals 0 select x;"""],
        ["""var q = from x in new int[0] join y in new int[0] on x equals y into field select x;"""],
        ["""var q = from x in new int[0] group x by x into field select 0;"""],
        ["""var q = from x in new int[0] select x into field select 0;"""],
        // Local function
        ["""int field() => 0;"""],
    ];

    // Declaration forms that do NOT raise CS9273 in C# 14 — S8367 will still flag these.
    public static IEnumerable<object[]> NonCS9273LocalDeclarationForms() =>
    [
        ["""if (GetHashCode() is int field) { }"""],
        ["""switch (GetHashCode()) { case int field: break; }"""],
        ["""if (this is object { } field) { }"""],
        ["""if ((0, 0) is (int field, _)) { }"""],
    ];

    [TestMethod]
    [DynamicData(nameof(CS9273LocalDeclarationForms))]
    public void IdentifiersNamedFieldShouldBeEscaped_CS9273LocalDeclarationForms_CSharp9_13(string statement)
    {
        statement += " // Noncompliant";
        builderCSharp9To13.AddSnippet(WrapInAccessor(statement)).Verify();
    }

    [TestMethod]
    [DynamicData(nameof(CS9273LocalDeclarationForms))]
    public void IdentifiersNamedFieldShouldBeEscaped_CS9273LocalDeclarationForms_Latest(string statement)
    {
        statement += " // Error [CS9273]";
        builderLatest.AddSnippet(WrapInAccessor(statement)).Verify();
    }

    [TestMethod]
    [DynamicData(nameof(NonCS9273LocalDeclarationForms))]
    public void IdentifiersNamedFieldShouldBeEscaped_NonCS9273LocalDeclarationForms_CSharp9_13(string statement)
    {
        // Although C# 14 does not raise CS9273 for these forms (likely a Roslyn oversight),
        // S8367 flags them in C# 9-13 anyway since 'field' is equally ambiguous in these contexts.
        statement += " // Noncompliant";
        builderCSharp9To13.AddSnippet(WrapInAccessor(statement)).Verify();
    }

    [TestMethod]
    [DynamicData(nameof(NonCS9273LocalDeclarationForms))]
    public void IdentifiersNamedFieldShouldBeEscaped_NonCS9273LocalDeclarationForms_Latest(string statement) =>
        builderLatest.AddSnippet(WrapInAccessor(statement)).VerifyNoIssues();

    private static string WrapInAccessor(string statement) =>
        $$"""
        using System;
        using System.Linq;
        class C
        {
            private int field;
            public int Value
            {
                get
                {
                    {{statement}}
                    return 0;
                }
            }
        }
        """;
}
