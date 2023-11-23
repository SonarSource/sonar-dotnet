/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
using SonarAnalyzer.UnitTest.TestFramework.Tests;

using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.AnalysisContext;

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
                c.ReportIssue(Diagnostic.Create(TestAnalyzer.Rule, c.Tree.GetRoot().GetFirstToken().GetLocation())))))
        .AddSnippet("""
        using System; // Noncompliant
        """)
        .Verify();

    [TestMethod]
    public void RegistrationIsExecuted_SonarAnalysisContext_VB() =>
        new VerifierBuilder().AddAnalyzer(() => new TestAnalyzerVB((context, g) =>
            context.RegisterSemanticModelAction(g, c =>
                c.ReportIssue(Diagnostic.Create(TestAnalyzer.Rule, c.Tree.GetRoot().GetFirstToken().GetLocation())))))
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
                        c.ReportIssue(Diagnostic.Create(TestAnalyzer.Rule, token.GetLocation()));
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
                        c.ReportIssue(Diagnostic.Create(TestAnalyzer.Rule, token.GetLocation()));
                    }
                }))))
            .AddSnippet("""
            Imports System ' Noncompliant
            """)
            .Verify();
}
