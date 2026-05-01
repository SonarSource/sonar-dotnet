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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UnnecessaryUsingsTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<UnnecessaryUsings>()
        .AddReferences(MetadataReferenceFacade.MicrosoftWin32Primitives)
        .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
        .WithWarningsAsErrors("CS0105");

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void UnnecessaryUsings() =>
        builder.AddPaths("UnnecessaryUsings.cs", "UnnecessaryUsings2.cs", "UnnecessaryUsingsFNRepro.cs").WithAutogenerateConcurrentFiles(false).Verify();

    [TestMethod]
    public void UnnecessaryUsings_InheritedProperty() =>
        builder.AddPaths("UnnecessaryUsings.InheritedPropertyBase.cs", "UnnecessaryUsings.InheritedPropertyChild.cs", "UnnecessaryUsings.InheritedPropertyUsage.cs")
            .WithAutogenerateConcurrentFiles(false)
            .VerifyNoIssues();

#if NET

    [TestMethod]
    public void UnnecessaryUsings_CSharp10_GlobalUsings() =>
        builder.AddPaths("UnnecessaryUsings.CSharp10.Global.cs", "UnnecessaryUsings.CSharp10.Consumer.cs").WithTopLevelStatements().WithOptions(LanguageOptions.FromCSharp10).Verify();

    [TestMethod]
    [DataRow("_ViewImports.cshtml")]
    [DataRow("_viewimports.cshtml")]
    [DataRow("_viEwiMpoRts.cshtml")]
    public void UnnecessaryUsings_RazorViewImportsCshtmlFile_NoIssueReported(string fileName) =>
        builder
            .AddSnippet(@"@using System.Text.Json;", fileName)
            .AddReferences(NuGetMetadataReference.SystemTextJson("7.0.4"))
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .VerifyNoIssues();

    [TestMethod]
    [DataRow("_Imports.razor")]
    [DataRow("_imports.razor")]
    [DataRow("_iMpoRts.razor")]
    public void UnnecessaryUsings_RazorImportsRazorFile_NoIssueReported(string fileName) =>
        builder
            .AddSnippet(@"@using System.Text.Json;", fileName)
            .AddReferences(NuGetMetadataReference.SystemTextJson("7.0.4"))
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .VerifyNoIssues();

    [TestMethod]
    [DataRow("RandomFile_ViewImports.cshtml")]
    [DataRow("RandomFile_Imports.cshtml")]
    [DataRow("_Imports.cshtml")]
    public void UnnecessaryUsings_RazorViewImportsSimilarCshtmlFile_IssuesReported(string fileName) =>
        builder
            .AddSnippet("@using System.Linq;", "_ViewImports.cshtml")
            .AddSnippet(@"@using System.Text.Json; @* Noncompliant *@", fileName)
            .AddReferences(NuGetMetadataReference.SystemTextJson("7.0.4"))
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    [DataRow("RandomFile_ViewImports.razor")]
    [DataRow("RandomFile_Imports.razor")]
    [DataRow("_ViewImports.razor")]
    public void UnnecessaryUsings_RazorViewImportsSimilarRazorFile_IssuesReported(string fileName) =>
        builder
            .AddSnippet("@using System.Linq;", "_Imports.razor")
            .AddSnippet(@"@using System.Text.Json; @* Noncompliant *@", fileName)
            .AddReferences(NuGetMetadataReference.SystemTextJson("7.0.4"))
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    [DataRow("_ViewImports.cs")]
    [DataRow("_Imports.cs")]
    public void UnnecessaryUsings_RazorViewImportsSimilarCSFile_IssuesReported(string fileName) =>
        builder.AddSnippet(@"using System.Text; // Noncompliant", fileName).Verify();

    [TestMethod]
    public void UnnecessaryUsings_CSharp10_FileScopedNamespace() =>
        builder.AddPaths("UnnecessaryUsings.CSharp10.FileScopedNamespace.cs").WithOptions(LanguageOptions.FromCSharp10).WithConcurrentAnalysis(false).Verify();

    [TestMethod]
    public void UnnecessaryUsings_CodeFix_CSharp10_FileScopedNamespace() =>
        builder.AddPaths("UnnecessaryUsings.CSharp10.FileScopedNamespace.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .WithCodeFix<UnnecessaryUsingsCodeFix>()
            .WithCodeFixedPaths("UnnecessaryUsings.CSharp10.FileScopedNamespace.Fixed.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void UnnecessaryUsings_CSharp9() =>
        builder.AddPaths("UnnecessaryUsings.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void UnnecessaryUsings_TupleDeconstruct_NetCore() =>
        builder.AddPaths("UnnecessaryUsings.TupleDeconstruct.NetCore.cs").Verify();

    [TestMethod]
    public void UnnecessaryUsings_CSharp12() =>
        builder.AddPaths("UnnecessaryUsings.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).VerifyNoIssues();

#elif NETFRAMEWORK

    [TestMethod]
    public void UnnecessaryUsings_TupleDeconstruct_NetFx() =>
        builder.AddPaths("UnnecessaryUsings.TupleDeconstruct.NetFx.cs").Verify();

#endif

    [TestMethod]
    public void UnnecessaryUsings_CodeFix() =>
        builder.AddPaths("UnnecessaryUsings.cs")
            .WithCodeFix<UnnecessaryUsingsCodeFix>()
            .WithCodeFixedPaths("UnnecessaryUsings.Fixed.cs", "UnnecessaryUsings.Fixed.Batch.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void UnnecessaryUsings_DocumentationModeNone() =>
        builder
            .AddSnippet("""
                using System; // Noncompliant FP, used by cref https://sonarsource.atlassian.net/browse/NET-1950

                namespace SonarAnalyzer.Experiments.CSharp
                {
                    public enum S1128
                    {
                        /// <summary><see cref="DateTime"/></summary>
                        DateTimeValue,
                    }
                }
                """)
            .WithOptions(ImmutableArray.Create<ParseOptions>(new CSharpParseOptions(documentationMode: DocumentationMode.None)))
            .Verify();

    [TestMethod]
    public void EquivalentNameSyntax_Equals_Object()
    {
        var main = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
        object same = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
        object different = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Ipsum"));

        main.Equals(same).Should().BeTrue();
        main.Equals(null).Should().BeFalse();
        main.Equals("different type").Should().BeFalse();
        main.Equals(different).Should().BeFalse();
    }

    [TestMethod]
    public void EquivalentNameSyntax_Equals_EquivalentNameSyntax()
    {
        var main = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
        var same = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
        var different = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Ipsum"));

        main.Equals(same).Should().BeTrue();
        main.Equals(null).Should().BeFalse();
        main.Equals(different).Should().BeFalse();
    }

    [TestMethod]
    public void NamespaceComparer_MatchesAncestors_WhenDefaultEqualityDoesNot()
    {
        var compiler = new SnippetCompiler("""
            namespace System.IO
            {
                class C
                {
                    Exception e;
                }
            }
            """);
        var fromDeclaration = compiler.NamespaceSymbol("IO").ContainingNamespace;
        var fromType = compiler.Symbol<ISymbol>(compiler.Nodes<IdentifierNameSyntax>().Single(x => x.Identifier.Text == "Exception")).ContainingNamespace;

        // Both represent "System" but Roslyn returns a merged namespace from the declaration chain vs a per-assembly namespace from ContainingNamespace
        fromDeclaration.GetHashCode().Should().NotBe(fromType.GetHashCode());
        fromDeclaration.Equals(fromType).Should().BeFalse();

        NamespaceComparer.Instance.GetHashCode(fromDeclaration).Should().Be(NamespaceComparer.Instance.GetHashCode(fromType));
        NamespaceComparer.Instance.Equals(fromDeclaration, fromType).Should().BeTrue();
    }
}
