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
public class RedundantNullForgivingOperatorTest
{
    // Every nullable reference type warning (source: Roslyn ErrorFacts.NullableWarnings), promoted to Error so a
    // missed nested-nullability mismatch surfaces as a real, assertable compiler diagnostic instead of staying invisible.
    private static readonly string[] NullableWarningIds =
    [
        "CS8597", "CS8600", "CS8601", "CS8602", "CS8603", "CS8604", "CS8605", "CS8607", "CS8608", "CS8609",
        "CS8610", "CS8611", "CS8612", "CS8613", "CS8614", "CS8615", "CS8616", "CS8617", "CS8618", "CS8619",
        "CS8620", "CS8621", "CS8622", "CS8624", "CS8625", "CS8629", "CS8631", "CS8633", "CS8634", "CS8643",
        "CS8644", "CS8645", "CS8655", "CS8667", "CS8670", "CS8714", "CS8762", "CS8763", "CS8764", "CS8765",
        "CS8766", "CS8767", "CS8768", "CS8769", "CS8770", "CS8774", "CS8775", "CS8776", "CS8777", "CS8819",
        "CS8824", "CS8825", "CS8847", "CS9158", "CS9159", "CS9264",
    ];

    private readonly VerifierBuilder builder = new VerifierBuilder<RedundantNullForgivingOperator>().WithWarningsAsErrors(NullableWarningIds);

    [TestMethod]
    public void RedundantNullForgivingOperator_CS() =>
        builder.AddPaths("RedundantNullForgivingOperator.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void RedundantNullForgivingOperator_CodeFix() =>
        builder.AddPaths("RedundantNullForgivingOperator.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithCodeFix<RedundantNullForgivingOperatorCodeFix>()
            .WithCodeFixedPaths("RedundantNullForgivingOperator.Latest.Fixed.cs")
            .VerifyCodeFix();

    [TestMethod]
    [DataRow("enable", true)]
    [DataRow("enable warnings", true)]
    [DataRow("disable", false)]
    [DataRow("enable annotations", false)]
    [DataRow("disable warnings", false)]
    public void RedundantNullForgivingOperator_NullableContext_Pragma(string nullableContext, bool expectRedundant)
    {
        var code = $$"""
            #nullable {{nullableContext}}
            void Test(string a)
            {
                _ = a!; // Noncompliant
            }
            """;
        builder.WithOnlyDiagnostics(ExpectedRule(expectRedundant)).AddSnippet(code).WithTopLevelStatements().Verify();
    }

    [TestMethod]
    [DataRow(Microsoft.CodeAnalysis.NullableContextOptions.Enable, true)]
    [DataRow(Microsoft.CodeAnalysis.NullableContextOptions.Warnings, true)]
    [DataRow(Microsoft.CodeAnalysis.NullableContextOptions.Disable, false)]
    [DataRow(Microsoft.CodeAnalysis.NullableContextOptions.Annotations, false)]
    public void RedundantNullForgivingOperator_NullableContext_FromCompilationOptions(Microsoft.CodeAnalysis.NullableContextOptions nullableContextOptions, bool expectRedundant)
    {
        var code = """
            public class Sample
            {
                public void Method(string a)
                {
                    _ = a!; // Noncompliant
                }
            }
            """;
        builder.WithOnlyDiagnostics(ExpectedRule(expectRedundant))
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithCompilationOptionsCustomization(x => ((CSharpCompilationOptions)x).WithNullableContextOptions(nullableContextOptions))
            .AddSnippet(code)
            .Verify();
    }

    // The project-wide default and the in-source pragma are two independent ways to reach the same NullableContext;
    // a local pragma always overrides the project default at its position, in either direction.
    [TestMethod]
    [DataRow(Microsoft.CodeAnalysis.NullableContextOptions.Disable, "enable", true)]
    [DataRow(Microsoft.CodeAnalysis.NullableContextOptions.Enable, "disable warnings", false)]
    [DataRow(Microsoft.CodeAnalysis.NullableContextOptions.Annotations, "enable warnings", true)]
    public void RedundantNullForgivingOperator_PragmaOverridesProjectDefault(Microsoft.CodeAnalysis.NullableContextOptions projectDefault, string localPragma, bool expectRedundant)
    {
        var code = $$"""
            public class Sample
            {
                public void Method(string a)
                {
            #nullable {{localPragma}}
                    _ = a!; // Noncompliant
                }
            }
            """;
        builder.WithOnlyDiagnostics(ExpectedRule(expectRedundant))
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithCompilationOptionsCustomization(x => ((CSharpCompilationOptions)x).WithNullableContextOptions(projectDefault))
            .AddSnippet(code)
            .Verify();
    }

    [TestMethod]
    [DataRow("""public string[] Convert(string?[] a) => a!; // Compliant, "a" is not null but its element type is, removing "!" gives CS8619""")]
    [DataRow("""public string[] Convert(List<string?> list) => list.ToArray()!; // Compliant, ToArray() still yields string?[], removing "!" gives CS8619""")]
    [DataRow("""public string[] Filter(string?[] items) => items.Where(i => i is not null).ToArray()!; // Compliant, ToArray() still yields string?[], removing "!" gives CS8619""")]
    [DataRow("""
        public void Build(Dictionary<string, string[]> parameters, int? page)
        {
            if (page != null) parameters.Add("page", new[] { page?.ToString() }!); // Compliant, the array element is still nullable, removing "!" gives CS8620
        }
        """)]
    [DataRow("""
        public static List<T> Wrap<T>(T item) => new List<T> { item };
        public List<string> Convert(string a) => Wrap<string?>(a)!; // Compliant, the explicit type argument makes the result List<string?>, removing "!" gives CS8619
        """)]
    [DataRow("""
        public static Dictionary<T1, T2> WrapPair<T1, T2>(T1 key, T2 value) => new() { [key] = value };
        public Dictionary<string, string> Convert(string a, string b) => WrapPair<string, string?>(a, b)!; // Compliant, the second (non-first) type argument is annotated, removing "!" gives CS8620
        """)]
    [DataRow("""
        public static Dictionary<T1, T2> WrapPair<T1, T2>(T1 key, T2 value) => new() { [key] = value };
        public Dictionary<string, List<string>> Convert(string a, List<string?> b) => WrapPair<string, List<string?>>(a, b)!; // Compliant, the annotation is two levels deep (inside the nested List<T> type argument), removing "!" gives CS8619
        """)]
    public void RedundantNullForgivingOperator_NestedNullableAnnotation(string member)
    {
        var code = $$"""
            #nullable enable
            using System.Collections.Generic;
            using System.Linq;
            public class Sample
            {
                {{member}}
            }
            """;
        builder.WithOnlyDiagnostics(RedundantNullForgivingOperator.RuleS8969).WithOptions(LanguageOptions.CSharpLatest).AddSnippet(code).VerifyNoIssues();
    }

    [TestMethod]
    [DataRow("public void Discard(string a) => _ = Wrap<string?>(a)!; // FN, a discard has no target type, so no nested-nullability mismatch can occur")]
    [DataRow("public List<string?> Convert(string a) => Wrap<string?>(a)!; // FN, the return type already matches the nested annotation exactly")]
    [DataRow("public object Convert(string a) => Wrap<string?>(a)!; // FN, an \"object\" target ignores the type arguments entirely")]
    public void RedundantNullForgivingOperator_NestedNullableAnnotation_FN(string member)
    {
        var code = $$"""
            #nullable enable
            using System.Collections.Generic;
            public class Sample
            {
                public static List<T> Wrap<T>(T item) => new List<T> { item };
                {{member}}
            }
            """;
        builder.WithOnlyDiagnostics(RedundantNullForgivingOperator.RuleS8969).WithOptions(LanguageOptions.CSharpLatest).AddSnippet(code).VerifyNoIssues();
    }

    // Confirms NullableWarningIds surfaces NRT warnings as assertable // Error diagnostics.
    [TestMethod]
    [DataRow("object? o", "string s = (string)o; // Error [CS8600]")]
    [DataRow("string? s", "_ = s.Length; // Error [CS8602]")]
    public void RedundantNullForgivingOperator_OtherNullableWarningsAsErrors(string parameter, string statement)
    {
        var code = $$"""
            #nullable enable
            public class Sample
            {
                public void Method({{parameter}})
                {
                    {{statement}}
                }
            }
            """;
        builder.WithOptions(LanguageOptions.CSharpLatest).AddSnippet(code).Verify();
    }

    private static DiagnosticDescriptor ExpectedRule(bool redundant) =>
        redundant ? RedundantNullForgivingOperator.RuleS8969 : RedundantNullForgivingOperator.RuleS8970;
}
