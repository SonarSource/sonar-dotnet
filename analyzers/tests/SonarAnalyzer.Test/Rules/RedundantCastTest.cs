/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class RedundantCastTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<RedundantCast>();

    private static IEnumerable<(string Snippet, bool CompliantWithFlowState, bool CompliantWithoutFlowState)> NullableTestData => new[]
    {
        ("""_ = (string)"Test";""", false, false),
        ("""_ = (string?)"Test";""", true, false),
        ("""_ = (string)null;""", true, true),
        ("""_ = (string?)null;""", true, true),
        ("""_ = (string)nullable;""", true, false),
        ("""_ = (string?)nullable;""", false, false),
        ("""_ = (string)nonNullable;""", false, false),
        ("""_ = (string?)nonNullable;""", true, false),
        ("""_ = nullable as string;""", true, false),
        ("""_ = nonNullable as string;""", false, false),
        ("""if (nullable != null) _ = (string)nullable;""", false, false),
        ("""if (nullable != null) _ = (string?)nullable;""", true, false),
        ("""if (nullable != null) _ = nullable as string;""", false, false),
        ("""if (nonNullable == null) _ = (string)nonNullable;""", true, false),
        ("""if (nonNullable == null) _ = (string?)nonNullable;""", false, false),
        ("""if (nonNullable == null) _ = nonNullable as string;""", true, false),
    };

    private static IEnumerable<object[]> NullableTestDataWithFlowState =>
        NullableTestData.Select(x => new object[] { x.Snippet, x.CompliantWithFlowState });

    private static IEnumerable<object[]> NullableTestDataWithoutFlowState =>
        NullableTestData.Select(x => new object[] { x.Snippet, x.CompliantWithoutFlowState });

    [TestMethod]
    public void RedundantCast() =>
        builder.AddPaths("RedundantCast.cs").Verify();

    [TestMethod]
    public void RedundantCast_CSharp8() =>
        builder.AddPaths("RedundantCast.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET
    [TestMethod]
    public void RedundantCast_CSharp9() =>
        builder.AddPaths("RedundantCast.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();
#endif

    [TestMethod]
    public void RedundantCast_CodeFix() =>
        builder.AddPaths("RedundantCast.cs").WithCodeFix<RedundantCastCodeFix>().WithCodeFixedPaths("RedundantCast.Fixed.cs").VerifyCodeFix();

    [TestMethod]
    public void RedundantCast_DefaultLiteral() =>
        builder.AddSnippet("""
            using System;
            public static class MyClass
            {
                public static void RunAction(Action action)
                {
                    bool myBool = (bool)default; // FN - the cast is unneeded
                    RunFunc(() => { action(); return default; }, (bool)default); // should not raise because of the generic the cast is mandatory
                    RunFunc<bool>(() => { action(); return default; }, (bool)default); // FN - the cast is unneeded
                }

                 public static T RunFunc<T>(Func<T> func, T returnValue = default) => returnValue;
            }
            """).WithLanguageVersion(LanguageVersion.CSharp7_1).VerifyNoIssues();

    [TestMethod]
    [DynamicData(nameof(NullableTestDataWithFlowState))]
    public void RedundantCast_NullableEnabled(string snippet, bool compliant) =>
        VerifyNullableTests(snippet, "enable", compliant);

    [TestMethod]
    [DynamicData(nameof(NullableTestDataWithFlowState))]
    public void RedundantCast_NullableWarnings(string snippet, bool compliant) =>
        VerifyNullableTests(snippet, "enable warnings", compliant);

    [TestMethod]
    [DynamicData(nameof(NullableTestDataWithoutFlowState))]
    public void RedundantCast_NullableDisabled(string snippet, bool compliant) =>
        VerifyNullableTests(snippet, "disable", compliant);

    [TestMethod]
    [DynamicData(nameof(NullableTestDataWithoutFlowState))]
    public void RedundantCast_NullableAnnotations(string snippet, bool compliant) =>
        VerifyNullableTests(snippet, "enable annotations", compliant);

    private void VerifyNullableTests(string snippet, string nullableContext, bool compliant)
    {
        var code = $$"""
            #nullable {{nullableContext}}
            void Test(string nonNullable, string? nullable)
            {
                {{snippet}} // {{compliant switch { true => "Compliant", false => "Noncompliant" }}}
            }
            """;
        var snippetVerifier = builder.AddSnippet(code).WithTopLevelStatements();
        if (compliant)
        {
            snippetVerifier.VerifyNoIssues();
        }
        else
        {
            snippetVerifier.Verify();
        }
    }
}
