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

using SonarAnalyzer.AnalysisContext;

using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test.AnalysisContext;

[TestClass]
public class SonarSemanticModelReportingContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var (tree, model) = TestHelper.CompileCS("// Nothing to see here");
        var options = AnalysisScaffolding.CreateOptions();
        var context = new SemanticModelAnalysisContext(model, options, _ => { }, _ => true, cancel);
        var sut = new SonarSemanticModelReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.Tree.Should().BeSameAs(tree);
        sut.Compilation.Should().BeSameAs(model.Compilation);
        sut.SemanticModel.Should().BeSameAs(model);
        sut.Options.Should().BeSameAs(options);
        sut.Cancel.Should().Be(cancel);
    }

    [TestMethod]
    public void RegistrationIsExecuted_SonarAnalysisContext_CS() =>
        new VerifierBuilder().AddAnalyzer(() => new TestAnalyzerCS((context, g) =>
            context.RegisterSemanticModelAction(g, c =>
                c.ReportIssue(TestAnalyzer.Rule, c.Tree.GetRoot().GetFirstToken()))))
        .AddSnippet("""
        using System; // Noncompliant
        """)
        .Verify();

    [TestMethod]
    public void RegistrationIsExecuted_SonarAnalysisContext_VB() =>
        new VerifierBuilder().AddAnalyzer(() => new TestAnalyzerVB((context, g) =>
            context.RegisterSemanticModelAction(g, c =>
                c.ReportIssue(TestAnalyzer.Rule, c.Tree.GetRoot().GetFirstToken()))))
        .AddSnippet("""
        Imports System ' Noncompliant
        """)
        .Verify();

    [TestMethod]
    public void RegistrationIsExecuted_SonarCompilationStartAnalysisContext_CS() =>
        new VerifierBuilder().AddAnalyzer(() => new TestAnalyzerCS((context, _) =>
            context.RegisterCompilationStartAction(start =>
                start.RegisterSemanticModelAction(c =>
                {
                    if (c.Tree.GetRoot().GetFirstToken() is { RawKind: not (int)CS.SyntaxKind.None } token)
                    {
                        c.ReportIssue(TestAnalyzer.Rule, token);
                    }
                }))))
            .AddSnippet("""
            using System; // Noncompliant
            """)
            .Verify();

    [TestMethod]
    public void RegistrationIsExecuted_SonarCompilationStartAnalysisContext_VB() =>
        new VerifierBuilder().AddAnalyzer(() => new TestAnalyzerVB((context, _) =>
            context.RegisterCompilationStartAction(start =>
                start.RegisterSemanticModelAction(c =>
                {
                    if (c.Tree.GetRoot().GetFirstToken() is { RawKind: not (int)VB.SyntaxKind.None } token)
                    {
                        c.ReportIssue(TestAnalyzer.Rule, token);
                    }
                }))))
            .AddSnippet("""
            Imports System ' Noncompliant
            """)
            .Verify();

    internal abstract class TestAnalyzer : SonarDiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = AnalysisScaffolding.CreateDescriptorMain("SDummy");
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class TestAnalyzerCS : TestAnalyzer
    {
        private readonly Action<SonarAnalysisContext, GeneratedCodeRecognizer> initializeAction;
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;

        public TestAnalyzerCS(Action<SonarAnalysisContext, GeneratedCodeRecognizer> action) =>
            initializeAction = action;

        protected override void Initialize(SonarAnalysisContext context) =>
            initializeAction(context, GeneratedCodeRecognizer);
    }

    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    internal class TestAnalyzerVB : TestAnalyzer
    {
        private readonly Action<SonarAnalysisContext, GeneratedCodeRecognizer> initializeAction;
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;

        public TestAnalyzerVB(Action<SonarAnalysisContext, GeneratedCodeRecognizer> action) =>
            initializeAction = action;

        protected override void Initialize(SonarAnalysisContext context) =>
            initializeAction(context, GeneratedCodeRecognizer);
    }
}
