﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using Moq;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Common;
using RoslynAnalysisContext = Microsoft.CodeAnalysis.Diagnostics.AnalysisContext;

namespace SonarAnalyzer.UnitTest.AnalysisContext;

public partial class SonarAnalysisContextTest
{
    private const string SnippetFileName = "snippet0.cs";
    private const string AnotherFileName = "Any other file name to make snippet0 considered as changed.cs";

    private static readonly DiagnosticDescriptor[] DummyMainDescriptor = { TestHelper.CreateDescriptor("Sxxxx", DiagnosticDescriptorFactory.MainSourceScopeTag) };

    [DataTestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void RegisterSyntaxNodeActionInNonGenerated_UnchangedFiles_SonarAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var sut = new SonarAnalysisContext(context, DummyMainDescriptor);
        sut.RegisterSyntaxNodeActionInNonGenerated<SyntaxKind>(CSharpGeneratedCodeRecognizer.Instance, context.DelegateAction);

        context.AssertDelegateInvoked(expected);
    }

    [DataTestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void RegisterSyntaxNodeActionInNonGenerated_UnchangedFiles_SonarParametrizedAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var sut = new SonarParametrizedAnalysisContext(new(context, DummyMainDescriptor));
        sut.RegisterSyntaxNodeActionInNonGenerated<SyntaxKind>(CSharpGeneratedCodeRecognizer.Instance, context.DelegateAction);

        context.AssertDelegateInvoked(expected);
    }

    [DataTestMethod]
    [DataRow("snippet1.cs")]
    [DataRow("Other file is unchanged.cs")]
    public void RegisterNodeActionInAllFiles_UnchangedFiles_GeneratedFiles_AlwaysRuns(string unchangedFileName) =>
        new VerifierBuilder<DummyAnalyzerForGenerated>()
            .WithSonarProjectConfigPath(TestHelper.CreateSonarProjectConfigWithUnchangedFiles(TestContext, unchangedFileName))
            .AddSnippet("""
                        // <auto-generated/>
                        public class Something { } // Noncompliant
                        """)
            .Verify();

    [DataTestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void RegisterSyntaxTreeActionInNonGenerated_UnchangedFiles_SonarAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var sut = new SonarAnalysisContext(context, DummyMainDescriptor);
        sut.RegisterSyntaxTreeActionInNonGenerated(CSharpGeneratedCodeRecognizer.Instance, context.DelegateAction);

        context.AssertDelegateInvoked(expected);
    }

    [DataTestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void RegisterSyntaxTreeActionInNonGenerated_UnchangedFiles_SonarParametrizedAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var sut = new SonarParametrizedAnalysisContext(new(context, DummyMainDescriptor));
        sut.RegisterSyntaxTreeActionInNonGenerated(CSharpGeneratedCodeRecognizer.Instance, context.DelegateAction);
        sut.ExecutePostponedActions(new(sut.Context, MockCompilationStartAnalysisContext(context)));  // Manual invocation, because SonarParametrizedAnalysisContext stores actions separately

        context.AssertDelegateInvoked(expected);
    }

    [DataTestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void RegisterCodeBlockStartActionInNonGenerated_UnchangedFiles_SonarAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var sut = new SonarAnalysisContext(context, DummyMainDescriptor);
        sut.RegisterCodeBlockStartActionInNonGenerated<SyntaxKind>(CSharpGeneratedCodeRecognizer.Instance, context.DelegateAction);

        context.AssertDelegateInvoked(expected);
    }

    private static CompilationStartAnalysisContext MockCompilationStartAnalysisContext(DummyAnalysisContext context)
    {
        var mock = new Mock<CompilationStartAnalysisContext>(context.Model.Compilation, context.Options, CancellationToken.None);
        mock.Setup(x => x.RegisterSyntaxNodeAction(It.IsAny<Action<SyntaxNodeAnalysisContext>>(), It.IsAny<ImmutableArray<SyntaxKind>>()))
            .Callback<Action<SyntaxNodeAnalysisContext>, ImmutableArray<SyntaxKind>>((action, _) => action(context.CreateSyntaxNodeAnalysisContext())); // Invoke to call RegisterSyntaxTreeAction
        mock.Setup(x => x.RegisterSyntaxTreeAction(It.IsAny<Action<SyntaxTreeAnalysisContext>>()))
            .Callback<Action<SyntaxTreeAnalysisContext>>(x => x(new SyntaxTreeAnalysisContext(context.Tree, context.Options, _ => { }, _ => true, default)));
        return mock.Object;
    }

    private sealed class DummyAnalysisContext : RoslynAnalysisContext
    {
        public readonly AnalyzerOptions Options;
        public readonly SemanticModel Model;
        public readonly SyntaxTree Tree;
        private bool delegateWasInvoked;

        public DummyAnalysisContext(TestContext testContext, params string[] unchangedFiles)
        {
            var sonarProjectConfig = TestHelper.CreateSonarProjectConfigWithUnchangedFiles(testContext, unchangedFiles);
            var additionalFile = new AnalyzerAdditionalFile(sonarProjectConfig);
            Options = new(ImmutableArray.Create<AdditionalText>(additionalFile));
            (Tree, Model) = TestHelper.CompileCS("public class Sample { }");
        }

        public void DelegateAction<T>(T arg) =>
            delegateWasInvoked = true;

        public void AssertDelegateInvoked(bool expected, string because = "") =>
            delegateWasInvoked.Should().Be(expected, because);

        public SyntaxNodeAnalysisContext CreateSyntaxNodeAnalysisContext() =>
            new(Tree.GetRoot(), Model, Options, _ => { }, _ => true, default);

        public override void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) =>
            throw new NotImplementedException();

        public override void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) =>
            action(new DummyCodeBlockStartAnalysisContext<TLanguageKindEnum>(this));

        public override void RegisterCompilationAction(Action<CompilationAnalysisContext> action) =>
            throw new NotImplementedException();

        public override void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
            action(MockCompilationStartAnalysisContext(this));  // Directly invoke to let the inner registrations be added into this.actions

        public override void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action) =>
            throw new NotImplementedException();

        public override void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds) =>
            throw new NotImplementedException();

        public override void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) =>
            action(CreateSyntaxNodeAnalysisContext());

        public override void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action) =>
            throw new NotImplementedException();
    }

    private class DummyCodeBlockStartAnalysisContext<TSyntaxKind> : CodeBlockStartAnalysisContext<TSyntaxKind> where TSyntaxKind : struct
    {
        public DummyCodeBlockStartAnalysisContext(DummyAnalysisContext baseContext) : base(baseContext.Tree.GetRoot(), null, baseContext.Model, baseContext.Options, default) { }

        public override void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action) =>
            throw new NotImplementedException();

        public override void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TSyntaxKind> syntaxKinds) =>
            throw new NotImplementedException();
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    private class DummyAnalyzerForGenerated : SonarDiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor rule = TestHelper.CreateDescriptor("SDummy", DiagnosticDescriptorFactory.MainSourceScopeTag);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeActionInAllFiles(c => c.ReportIssue(Diagnostic.Create(rule, c.Node.GetLocation())), SyntaxKind.ClassDeclaration);
    }
}
