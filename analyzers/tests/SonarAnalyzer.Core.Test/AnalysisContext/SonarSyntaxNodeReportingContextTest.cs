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
using NSubstitute;
using SonarAnalyzer.AnalysisContext;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.AnalysisContext;

[TestClass]
public class SonarSyntaxNodeReportingContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var (tree, model) = TestHelper.CompileCS("// Nothing to see here");
        var node = tree.GetRoot();
        var options = AnalysisScaffolding.CreateOptions();
        var containingSymbol = Substitute.For<ISymbol>();
        var context = new SyntaxNodeAnalysisContext(node, containingSymbol, model, options, _ => { }, _ => true, cancel);
        var sut = new SonarSyntaxNodeReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.Tree.Should().BeSameAs(tree);
        sut.Compilation.Should().BeSameAs(model.Compilation);
        sut.Options.Should().BeSameAs(options);
        sut.Cancel.Should().Be(cancel);
        sut.Node.Should().BeSameAs(node);
        sut.SemanticModel.Should().BeSameAs(model);
        sut.ContainingSymbol.Should().BeSameAs(containingSymbol);
    }

#if NET // .NET Fx shows the message box directly, the exception cannot be caught

    [DataTestMethod]
    [DataRow(true)] // Purpose of this is to make sure that the scaffolding works successfully end-to-end
    [DataRow(false)]
    public void ReportIssue_TreeNotInCompilation_DoNotReport(bool reportOnCorrectTree)
    {
        var analysisContext = AnalysisScaffolding.CreateSonarAnalysisContext();
        var (tree, model) = TestHelper.CompileCS("// Nothing to see here");
        var nodeFromCorrectCompilation = tree.GetRoot();
        var nodeFromAnotherCompilation = TestHelper.CompileCS("// This is another Compilation with another Tree").Tree.GetRoot();
        var rule = AnalysisScaffolding.CreateDescriptorMain();
        var node = tree.GetRoot();
        var wasReported = false;
        var context = new SyntaxNodeAnalysisContext(node, model, AnalysisScaffolding.CreateOptions(), _ => wasReported = true, _ => true, default);
        var sut = new SonarSyntaxNodeReportingContext(analysisContext, context);
        try
        {
            sut.ReportIssue(rule, reportOnCorrectTree ? nodeFromCorrectCompilation : nodeFromAnotherCompilation);
        }
        catch (Exception ex)    // Can't catch internal DebugAssertException
        {
            if (reportOnCorrectTree)
            {
                throw;  // This should not happen => fail the test
            }
            else
            {
                ex.GetType().Name.Should().BeOneOf("AssertionException", "DebugAssertException");
                ex.Message.Should().Contain("Primary location should be part of the compilation. An AD0001 is raised if this is not the case.");
            }
        }

        wasReported.Should().Be(reportOnCorrectTree);
    }
#endif

    [DataTestMethod]
    [DataRow("class")]
#if NET
    [DataRow("record")]
#endif
    public void IsRedundantPrimaryConstructorBaseTypeContext_ReturnsTrueForTypeDeclaration(string type)
    {
        var compilerVersion = typeof(DiagnosticAnalyzer).Assembly.GetName().Version;
        // For Roslyn < 4.9.2, the node action is called either twice with different ContainingSymbol.
        var snippet = $$$"""
            public {{{type}}} Base(int i);
            public {{{type}}} Derived(int i) :
                Base(i); // This is the node that is asserted
            //  ^^^^^^^    {{IsRedundantPrimaryConstructorBaseTypeContext is False, ContainingSymbol is Method Derived.Derived(int)}}
            """;
        new VerifierBuilder()
            .AddAnalyzer(() => new TestAnalyzer(new[] { SyntaxKindEx.PrimaryConstructorBaseType }, c =>
                $"IsRedundantPrimaryConstructorBaseTypeContext is {c.IsRedundantPrimaryConstructorBaseTypeContext()}, ContainingSymbol is {c.ContainingSymbol.Kind} {c.ContainingSymbol.ToDisplayString()}"))
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .AddSnippet(snippet)
            .Verify();
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    private sealed class TestAnalyzer : SonarDiagnosticAnalyzer
    {
        private readonly SyntaxKind[] syntaxKinds;
        private readonly Func<SonarSyntaxNodeReportingContext, string> message;

        public DiagnosticDescriptor Rule { get; } = DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp,
            new RuleDescriptor("Test", "Test", "BUG", "BLOCKER", "READY", SourceScope.All, true, "Test"), "{0}", true, false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public TestAnalyzer(SyntaxKind[] syntaxKinds, Func<SonarSyntaxNodeReportingContext, string> message)
        {
            this.syntaxKinds = syntaxKinds;
            this.message = message;
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(TestGeneratedCodeRecognizer.Instance, c => c.ReportIssue(Rule, c.Node, message(c)), syntaxKinds);
    }
}
