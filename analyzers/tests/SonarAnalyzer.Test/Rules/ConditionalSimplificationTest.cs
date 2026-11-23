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
public class ConditionalSimplificationTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ConditionalSimplification>();
    private readonly VerifierBuilder codeFix = new VerifierBuilder<ConditionalSimplification>().WithCodeFix<ConditionalSimplificationCodeFix>();

    [TestMethod]
    public void ConditionalSimplification_BeforeCSharp8() =>
        builder.AddPaths("ConditionalSimplification.BeforeCSharp8.cs").WithOptions(LanguageOptions.BeforeCSharp8).Verify();

    [TestMethod]
    public void ConditionalSimplification_CSharp8() =>
        builder.AddPaths("ConditionalSimplification.CSharp8.cs").WithLanguageVersion(LanguageVersion.CSharp8).Verify();

    [TestMethod]
    public void ConditionalSimplification_CSharp8_CodeFix() =>
        codeFix.AddPaths("ConditionalSimplification.CSharp8.cs").WithLanguageVersion(LanguageVersion.CSharp8).WithCodeFixedPaths("ConditionalSimplification.CSharp8.Fixed.cs").VerifyCodeFix();

    [TestMethod]
    public void ConditionalSimplification_FromCSharp8() =>
        builder.AddPaths("ConditionalSimplification.FromCSharp8.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

    [TestMethod]
    public void ConditionalSimplification_BeforeCSharp8_CodeFix() =>
        codeFix.AddPaths("ConditionalSimplification.BeforeCSharp8.cs")
            .WithCodeFixedPaths("ConditionalSimplification.BeforeCSharp8.Fixed.cs")
            .WithOptions(LanguageOptions.BeforeCSharp8).VerifyCodeFix();

    [TestMethod]
    public void ConditionalSimplification_FromCSharp8_CodeFix() =>
        codeFix.AddPaths("ConditionalSimplification.FromCSharp8.cs")
            .WithCodeFixedPaths("ConditionalSimplification.FromCSharp8.Fixed.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .VerifyCodeFix();

    [TestMethod]
    public void ConditionalSimplification_FromCSharp9() =>
        builder.AddPaths("ConditionalSimplification.FromCSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void ConditionalSimplification_FromCSharp10() =>
        builder.AddPaths("ConditionalSimplification.FromCSharp10.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.FromCSharp10)
            .AddReferences(NuGetMetadataReference.FluentAssertions(NugetPackageVersions.FluentAssertionsVersions.Ver7))
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .AddReferences(MetadataReferenceFacade.SystemXmlLinq)
            .Verify();

    // https://github.com/SonarSource/sonar-dotnet/issues/9846
    // https://sonarsource.atlassian.net/browse/NET-5208
    [TestMethod]
    [DataRow("c with { F = F ?? input.F }")]
    [DataRow("c with { F = (F) ?? input.F }")]
    [DataRow("c with { F = F ?? \"x\" }")]
    public void ConditionalSimplification_Initializer_Coalesce_FP(string expression) =>
        builder.AddSnippet($$"""
            #nullable enable
            record C { public string? F { get; init; } }

            class A
            {
                string? F { get; init; }

                private C AppliedTo(C c, A input) =>
                    {{expression}};   // Noncompliant FP, '??=' is not valid in a 'with' initializer
            }
            """).WithOptions(LanguageOptions.CSharpLatest).Verify();

    // https://sonarsource.atlassian.net/browse/NET-5228
    [TestMethod]
    [DataRow("c with { F = F != null ? F : input.F }")]
    [DataRow("c with { F = F == null ? input.F : F }")]
    [DataRow("c with { F = (F != null) ? F : input.F }")]
    [DataRow("new C { F = F != null ? F : input.F }")]
    public void ConditionalSimplification_Initializer_Ternary_FP(string expression) =>
        builder.AddSnippet($$"""
            #nullable enable
            record C { public string? F { get; init; } }

            class A
            {
                string? F { get; init; }

                private C AppliedTo(C c, A input) =>
                    {{expression}};   // Noncompliant FP, '??=' is not valid in an initializer
            }
            """).WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    [DataRow("c with { F = this.F ?? input.F }")]
    [DataRow("c with { F = input.F ?? F }")]
    [DataRow("c with { F = c.F ?? input.F }")]
    [DataRow("c with { F = F != null ? input.F : F }")]
    [DataRow("new C { F = F ?? input.F }")]
    public void ConditionalSimplification_Initializer_Compliant(string expression) =>
        builder.AddSnippet($$"""
            #nullable enable
            record C { public string? F { get; init; } }

            class A
            {
                string? F { get; init; }

                private object AppliedTo(C c, A input) =>
                    {{expression}};
            }
            """).WithOptions(LanguageOptions.CSharpLatest).VerifyNoIssues();

    [TestMethod]
    public void ConditionalSimplification_FromCSharp9_CodeFix() =>
        codeFix.AddPaths("ConditionalSimplification.FromCSharp9.cs")
            .WithCodeFixedPaths("ConditionalSimplification.FromCSharp9.Fixed.cs")
            .WithTopLevelStatements()
            .VerifyCodeFix();

    [TestMethod]
    public void ConditionalSimplification_FromCSharp10_CodeFix() =>
        codeFix.AddPaths("ConditionalSimplification.FromCSharp10.cs")
            .WithCodeFixedPaths("ConditionalSimplification.FromCSharp10.Fixed.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.FromCSharp10)
            .AddReferences(NuGetMetadataReference.FluentAssertions(NugetPackageVersions.FluentAssertionsVersions.Ver7))
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .AddReferences(MetadataReferenceFacade.SystemXmlLinq)
            .VerifyCodeFix();
}
