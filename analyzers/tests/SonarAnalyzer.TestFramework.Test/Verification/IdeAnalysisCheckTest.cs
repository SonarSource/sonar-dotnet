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

using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Core.Analyzers;
using SonarAnalyzer.Core.Syntax.Utilities;
using SonarAnalyzer.TestFramework.Analyzers;
using SonarAnalyzer.TestFramework.Common;
using SonarAnalyzer.TestFramework.Verification;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification;

[TestClass]
public class IdeAnalysisCheckTest
{
    private const string Code = "class C { void M() { } } // Noncompliant";

    [TestMethod]
    public void Verify_ReportInsideCallbackNode_Passes() =>
        Builder((context, g) => context.RegisterNodeAction(g, c => c.ReportIssue(TestAnalyzer.Rule, ((MethodDeclarationSyntax)c.Node).Identifier.GetLocation()), SyntaxKind.MethodDeclaration))
            .Invoking(x => x.Verify()).Should().NotThrow();

    [TestMethod]
    public void Verify_ReportOutsideCallbackNode_Throws() =>
        // The class identifier is located before the method, which is outside of the callback node.
        Builder((context, g) => context.RegisterNodeAction(g, c => c.ReportIssue(TestAnalyzer.Rule, ((ClassDeclarationSyntax)c.Node.Parent).Identifier.GetLocation()), SyntaxKind.MethodDeclaration))
            .Invoking(x => x.Verify()).Should().ThrowExactly<AssertFailedException>()
            .WithMessage("*not by the per-document analysis that IDEs use*STestAnalyzer snippet0.cs@1:7*What to do*WithoutIdeAnalysisCheck*");

    [TestMethod]
    public void Verify_ReportOutsideCallbackNode_OptOut_Passes() =>
        Builder((context, g) => context.RegisterNodeAction(g, c => c.ReportIssue(TestAnalyzer.Rule, ((ClassDeclarationSyntax)c.Node.Parent).Identifier.GetLocation()), SyntaxKind.MethodDeclaration))
            .WithoutIdeAnalysisCheck("Reported outside of the callback node").Invoking(x => x.Verify()).Should().NotThrow();

    [TestMethod]
    public void Verify_CompilationEnd_Tagged_Passes() =>
        new VerifierBuilder<TaggedCompilationEndAnalyzer>().AddSnippet(Code).Invoking(x => x.Verify()).Should().NotThrow();

    [TestMethod]
    public void Verify_MixedAnalyzer_UntaggedDescriptor_OutsideCallbackNode_Throws() =>
        MixedBuilder(reportTagged: false)
            .Invoking(x => x.Verify())
            .Should().ThrowExactly<AssertFailedException>()
            .WithMessage("*not by the per-document analysis that IDEs use*SMixed snippet0.cs@1:7*");

    [TestMethod]
    public void Verify_MixedAnalyzer_TaggedDescriptor_OutsideCallbackNode_IsSkipped() =>
        MixedBuilder(reportTagged: true)
            .Invoking(x => x.Verify())
            .Should().NotThrow();

    [TestMethod]
    public void Verify_CompilationEnd_NotTagged_Throws() =>
        Builder((context, g) => context.RegisterCompilationStartAction(start =>
                start.RegisterCompilationEndAction(end => end.ReportIssue(g, TestAnalyzer.Rule, end.Compilation.SyntaxTrees.First().GetRoot().GetFirstToken().GetLocation()))))
            .Invoking(x => x.Verify()).Should().ThrowExactly<AssertFailedException>()
            .WithMessage("*not by the per-document analysis that IDEs use*STestAnalyzer snippet0.cs@1:1*isCompilationEnd: true*");

    [TestMethod]
    public void Verify_CompilationEnd_NotTagged_WithoutLocation_Throws() =>
        CompilationEndWithoutLocation()
            .Invoking(x => x.Verify())
            .Should().ThrowExactly<AssertFailedException>()
            .WithMessage("*TestAnalyzerCS register(s) a compilation end action, but none of the descriptors has the CompilationEnd tag*isCompilationEnd: true*");

    [TestMethod]
    public void Verify_CompilationEnd_NotTagged_WithoutLocation_OptOut_Passes() =>
        CompilationEndWithoutLocation()
            .WithoutIdeAnalysisCheck("By design")
            .Invoking(x => x.Verify())
            .Should().NotThrow();

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Verify_HonorsConcurrentAnalysis(bool concurrentAnalysis)
    {
        ConcurrencyAnalyzer.Observed.Clear();
        new VerifierBuilder<ConcurrencyAnalyzer>().AddSnippet(Code).WithConcurrentAnalysis(concurrentAnalysis).Verify();
        // The main verification and the per-document analysis of the IDE check each create the analyzer.
        ConcurrencyAnalyzer.Observed.Should().HaveCountGreaterThanOrEqualTo(2).And.OnlyContain(x => x == concurrentAnalysis);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void WithoutIdeAnalysisCheck_NoReason_Throws(string reason) =>
        new VerifierBuilder().Invoking(x => x.WithoutIdeAnalysisCheck(reason)).Should().Throw<ArgumentException>().WithParameterName("reason");

    [TestMethod]
    public void WithoutIdeAnalysisCheck_Overwrites_IsImmutable()
    {
        var empty = new VerifierBuilder();
        var one = empty.WithoutIdeAnalysisCheck("One");
        var two = one.WithoutIdeAnalysisCheck("Two");
        empty.IdeAnalysisCheckOptOutReason.Should().BeNull();
        one.IdeAnalysisCheckOptOutReason.Should().Be("One");
        two.IdeAnalysisCheckOptOutReason.Should().Be("Two");
    }

    private static VerifierBuilder Builder(Action<SonarAnalysisContext, GeneratedCodeRecognizer> initialize) =>
        new VerifierBuilder().AddAnalyzer(() => new TestAnalyzerCS(initialize)).AddSnippet(Code);

    private static VerifierBuilder MixedBuilder(bool reportTagged) =>
        new VerifierBuilder().AddAnalyzer(() => new MixedAnalyzer(reportTagged)).AddSnippet(Code);

    private static VerifierBuilder CompilationEndWithoutLocation() =>
        Builder((context, g) => context.RegisterCompilationStartAction(start => start.RegisterCompilationEndAction(end => end.ReportIssue(g, TestAnalyzer.Rule, null))));

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    private sealed class ConcurrencyAnalyzer : SonarDiagnosticAnalyzer
    {
        public static readonly ConcurrentBag<bool> Observed = [];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(TestAnalyzer.Rule);

        protected override bool EnableConcurrentExecution
        {
            get
            {
                var enabled = IsConcurrentExecutionEnabled();
                Observed.Add(enabled);
                return enabled;
            }
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(TestGeneratedCodeRecognizer.Instance, c => c.ReportIssue(TestAnalyzer.Rule, c.Node.GetLocation()), SyntaxKind.ClassDeclaration);
    }

    // One rule ID with a tagged and an untagged descriptor, like a rule that reports from a node action and from a compilation end action. The tag is checked per descriptor.
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    private sealed class MixedAnalyzer : SonarDiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor untagged = AnalysisScaffolding.CreateDescriptor("SMixed", DiagnosticDescriptorFactory.MainSourceScopeTag);
        private readonly DiagnosticDescriptor tagged = AnalysisScaffolding.CreateDescriptor("SMixed", DiagnosticDescriptorFactory.MainSourceScopeTag, WellKnownDiagnosticTags.CompilationEnd);
        private readonly bool reportTagged;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(untagged, tagged);

        public MixedAnalyzer(bool reportTagged) =>
            this.reportTagged = reportTagged;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                TestGeneratedCodeRecognizer.Instance,
                c => c.ReportIssue(reportTagged ? tagged : untagged, ((ClassDeclarationSyntax)c.Node.Parent).Identifier.GetLocation()),
                SyntaxKind.MethodDeclaration);
    }

    // TestAnalyzer.Rule does not have the CompilationEnd tag, so the tagged case needs its own descriptor.
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    private sealed class TaggedCompilationEndAnalyzer : SonarDiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor rule = AnalysisScaffolding.CreateDescriptor("SCompilationEnd", DiagnosticDescriptorFactory.MainSourceScopeTag, WellKnownDiagnosticTags.CompilationEnd);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(start =>
                start.RegisterCompilationEndAction(end => end.ReportIssue(TestGeneratedCodeRecognizer.Instance, rule, end.Compilation.SyntaxTrees.First().GetRoot().GetFirstToken().GetLocation())));
    }
}
