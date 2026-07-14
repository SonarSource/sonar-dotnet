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
    private readonly VerifierBuilder builder = new VerifierBuilder<RedundantNullForgivingOperator>();

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

    private static DiagnosticDescriptor ExpectedRule(bool redundant) =>
        redundant ? RedundantNullForgivingOperator.RuleS8969 : RedundantNullForgivingOperator.RuleS8970;
}
