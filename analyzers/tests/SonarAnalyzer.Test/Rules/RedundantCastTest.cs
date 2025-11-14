/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public class RedundantCastTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<RedundantCast>();

    private static IEnumerable<(string Snippet, bool CompliantWithFlowState, bool CompliantWithoutFlowState)> NullableTestData =>
    [
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
        ("""_ = (string)default!;""", true, true),
        ("""_ = (string)(default)!;""", true, true),
        ("""_ = (string)((default)!);""", true, true),
        ("""_ = (string)(default!);""", true, true),
        ("""_ = (string)(default(string));""", true, false),
        ("""_ = (string)(default(string)!);""", false, false),
        ("""_ = (string?)(default(string));""", false, false),
        ("""_ = (string?)(default(string)!);""", true, false),
    ];

    private static IEnumerable<object[]> NullableTestDataWithFlowState => NullableTestData.Select(x => new object[] { x.Snippet, x.CompliantWithFlowState });

    private static IEnumerable<object[]> NullableTestDataWithoutFlowState => NullableTestData.Select(x => new object[] { x.Snippet, x.CompliantWithoutFlowState });

    [TestMethod]
    public void RedundantCast() =>
        builder.AddPaths("RedundantCast.cs").Verify();

    [TestMethod]
    public void RedundantCast_CSharp8() =>
        builder.AddPaths("RedundantCast.CSharp8.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET
    [TestMethod]
    public void RedundantCast_CSharp9() =>
        builder.AddPaths("RedundantCast.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).WithConcurrentAnalysis(false).Verify();
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
    [DataRow("IEnumerable<string?>", "IEnumerable<string>", false)]
    [DataRow("IEnumerable<string>", "IEnumerable<string?>", false)]
    [DataRow("IEnumerable<string?>", "IEnumerable<string?>", true)]
    [DataRow("IEnumerable<int>", "IEnumerable<int?>", false)]
    [DataRow("IEnumerable<Nullable<int>>", "IEnumerable<int?>", true)]
    [DataRow("IEnumerable<int?>", "IEnumerable<Nullable<int>>", true)]
    [DataRow("IEnumerable<Nullable<int>>", "IEnumerable<Nullable<int>>", true)]
    [DataRow("IDictionary<string, KeyValuePair<string, string?>>", "IDictionary<string, KeyValuePair<string, string>>", false)]
    [DataRow("IDictionary<string, KeyValuePair<string?, string>>?", "IDictionary<string, KeyValuePair<string, string>>?", false)]
    [DataRow("IDictionary<string, KeyValuePair<string?, string?>>?", "IDictionary<string, KeyValuePair<string?, string?>>?", true)]
    [DataRow("int?", "Nullable<int>", true)]
    [DataRow("Nullable<int>", "int?", true)]
    [DataRow("IEnumerable<IEnumerable<int>?>", "IEnumerable<IEnumerable<int>?>", true)]
    [DataRow("IEnumerable<IEnumerable<int>?>", "IEnumerable<IEnumerable<int>>", false)]
    [DataRow("IEnumerable<IEnumerable<int>>", "IEnumerable<IEnumerable<int>?>", false)]
    [DataRow("IEnumerable<T?>", "IEnumerable<T>", false)]
    [DataRow("IEnumerable<T>", "IEnumerable<T?>", false)]
    [DataRow("IEnumerable<TClass?>", "IEnumerable<TClass>", false)]
    [DataRow("IEnumerable<TClass>", "IEnumerable<TClass?>", false)]
    [DataRow("IEnumerable<TStruct>", "IEnumerable<TStruct?>", false)]
    [DataRow("IEnumerable<Nullable<TStruct>>", "IEnumerable<TStruct?>", true)]
    [DataRow("IEnumerable<TStruct?>", "IEnumerable<Nullable<TStruct>>", true)]
    [DataRow("IEnumerable<string>", "IEnumerable<object>", false)]
    [DataRow("IEnumerable<object>", "IEnumerable<string>", false)]
    [DataRow("List<int>", "IntList", true)]
    [DataRow("IntList", "List<int>", true)]
    [DataRow("List<string>", "StringList", true)]
    [DataRow("StringList", "List<string>", true)]
    [DataRow("List<string?>", "StringList", false)]
    [DataRow("StringList", "List<string?>", false)]
    [DataRow("List<string?>", "NullableStringList", true)]
    [DataRow("NullableStringList", "List<string?>", true)]
    [DataRow("List<string>", "NullableStringList", false)]
    [DataRow("NullableStringList", "List<string>", false)]
    [DataRow("string?", "string?", true)]
    [DataRow("string", "string", true)]
    [DataRow("string?", "string", false)]
    [DataRow("string", "string?", false)]
    public void RedundantCast_TypeArgumentAnnotations(string expressionType, string targetType, bool expected)
    {
        var verifier = builder.AddSnippet($$"""
            #nullable enable

            using System;
            using System.Collections.Generic;
            using System.Linq;
            using IntList = System.Collections.Generic.List<int>;
            using StringList = System.Collections.Generic.List<string>;
            using NullableStringList = System.Collections.Generic.List<string?>;

            class TypeArguments<T, TStruct, TClass>
                where TStruct : struct
                where TClass : class
            {
                public void Test({{expressionType}} expression)
                {
                    _ = ({{targetType}})expression; // {{(expected ? "Noncompliant" : string.Empty)}}
                }
            }
            """).WithLanguageVersion(LanguageVersion.CSharp9);
        if (expected)
        {
            verifier.Verify();
        }
        else
        {
            verifier.VerifyNoIssues();
        }
    }

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
