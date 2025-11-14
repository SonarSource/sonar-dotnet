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

using System.IO;
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.CSharp.Core.Syntax.Utilities;
using SonarAnalyzer.CSharp.Rules;
using SonarAnalyzer.Test.Rules;

namespace SonarAnalyzer.Test.AnalysisContext;

[TestClass]
public partial class SonarAnalysisContextTest
{
    // Various classes that invoke all the `ReportIssue` methods in AnalysisContextExtensions
    // We mention in comments the type of Context that is used to invoke (directly or indirectly) the `ReportIssue` method
    private readonly List<TestSetup> testCases =
        [
            // SyntaxNodeAnalysisContext
            // S3244 - MAIN and TEST
            new TestSetup("AnonymousDelegateEventUnsubscribe.cs", new AnonymousDelegateEventUnsubscribe()),
            // S2699 - TEST only
            new TestSetup(
                "TestMethodShouldContainAssertion.NUnit.cs",
                new TestMethodShouldContainAssertion(),
                // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
                TestMethodShouldContainAssertionTest.WithTestReferences(NuGetMetadataReference.NUnit("3.14.0")).References),    // ToDo: Reuse the entire builder in TestSetup

            // SyntaxTreeAnalysisContext
            // S3244 - MAIN and TEST
            new TestSetup("AsyncAwaitIdentifier.cs", new AsyncAwaitIdentifier()),

            // CompilationAnalysisContext
            // S3244 - MAIN and TEST
            new TestSetup(
                @"Hotspots\RequestsWithExcessiveLength.cs",
                new RequestsWithExcessiveLength(AnalyzerConfiguration.AlwaysEnabled),
                RequestsWithExcessiveLengthTest.GetAdditionalReferences()),

            // CodeBlockAnalysisContext
            // S5693 - MAIN and TEST
            new TestSetup("GetHashCodeEqualsOverride.cs", new GetHashCodeEqualsOverride()),

            // SymbolAnalysisContext
            // S2953 - MAIN only
            new TestSetup("DisposeNotImplementingDispose.cs", new DisposeNotImplementingDispose()),
            // S1694 - MAIN only
            new TestSetup("AbstractClassToInterface.cs", new AbstractClassToInterface()),
        ];

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Constructor_Null() =>
        ((Func<SonarAnalysisContext>)(() => new(null, default))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("analysisContext");

    [TestMethod]
    public void WhenShouldAnalysisBeDisabledReturnsTrue_NoIssueReported()
    {
        SonarAnalysisContext.ShouldExecuteRegisteredAction = (_, _) => false;
        try
        {
            foreach (var testCase in testCases)
            {
                // ToDo: We should find a way to ack the fact the action was not run
                testCase.Builder
                    .WithOptions(LanguageOptions.FromCSharp8)
                    .VerifyNoIssuesIgnoreErrors();
            }
        }
        finally
        {
            SonarAnalysisContext.ShouldExecuteRegisteredAction = null;
        }
    }

    [TestMethod]
    public void ByDefault_ExecuteRule()
    {
        foreach (var testCase in testCases)
        {
            // ToDo: We test that a rule is enabled only by checking the issues are reported
            testCase.Builder
                .WithOptions(LanguageOptions.FromCSharp8)
                .Verify();
        }
    }

    [TestMethod]
    public void WhenProjectType_IsTest_RunRulesWithTestScope_SonarLint()
    {
        var sonarProjectConfig = AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Test, false);
        foreach (var testCase in testCases)
        {
            var hasTestScope = testCase.Analyzer.SupportedDiagnostics.Any(x => x.CustomTags.Contains(DiagnosticDescriptorFactory.TestSourceScopeTag));
            if (hasTestScope)
            {
                testCase.Builder
                    .WithOptions(LanguageOptions.FromCSharp8)
                    .WithAdditionalFilePath(sonarProjectConfig)
                    .Verify();
            }
            else
            {
                // MAIN-only
                testCase.Builder
                    .WithOptions(LanguageOptions.FromCSharp8)
                    .WithAdditionalFilePath(sonarProjectConfig)
                    .VerifyNoIssuesIgnoreErrors();
            }
        }
    }

    [TestMethod]
    public void WhenProjectType_IsTest_RunRulesWithTestScope_Scanner()
    {
        var sonarProjectConfig = AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Test);
        foreach (var testCase in testCases)
        {
            var hasProductScope = testCase.Analyzer.SupportedDiagnostics.Any(x => x.CustomTags.Contains(DiagnosticDescriptorFactory.MainSourceScopeTag));
            if (hasProductScope)
            {
                // MAIN-only and MAIN & TEST rules
                testCase.Builder
                    .WithOptions(LanguageOptions.FromCSharp8)
                    .WithAdditionalFilePath(sonarProjectConfig)
                    .VerifyNoIssuesIgnoreErrors();
            }
            else
            {
                testCase.Builder
                    .WithOptions(LanguageOptions.FromCSharp8)
                    .WithAdditionalFilePath(sonarProjectConfig)
                    .Verify();
            }
        }
    }

    [TestMethod]
    public void WhenProjectType_IsTest_RunRulesWithMainScope()
    {
        var sonarProjectConfig = AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product);
        foreach (var testCase in testCases)
        {
            var hasProductScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorFactory.MainSourceScopeTag));
            if (hasProductScope)
            {
                testCase.Builder
                    .WithOptions(LanguageOptions.FromCSharp8)
                    .WithAdditionalFilePath(sonarProjectConfig)
                    .Verify();
            }
            else
            {
                // TEST-only rule
                testCase.Builder
                    .WithOptions(LanguageOptions.FromCSharp8)
                    .WithAdditionalFilePath(sonarProjectConfig)
                    .VerifyNoIssues();
            }
        }
    }

    [TestMethod]
    public void WhenAnalysisDisabledBaseOnSyntaxTree_ReportIssuesForEnabledRules()
    {
        testCases.Should().HaveCountGreaterThan(2);

        try
        {
            var testCase = testCases[0];
            var testCase2 = testCases[2];
            SonarAnalysisContext.ShouldExecuteRegisteredAction = (diags, tree) => tree.FilePath.EndsWith(new FileInfo(testCase.Path).Name, StringComparison.OrdinalIgnoreCase);
            testCase.Builder.WithConcurrentAnalysis(false).Verify();
            testCase2.Builder.VerifyNoIssues();
        }
        finally
        {
            SonarAnalysisContext.ShouldExecuteRegisteredAction = null;
        }
    }

    [TestMethod]
    public void WhenReportDiagnosticActionNotNull_AllowToControlWhetherOrNotToReport()
    {
        try
        {
            SonarAnalysisContext.ReportDiagnostic = context =>
            {
                // special logic for rules with SyntaxNodeAnalysisContext
                if (context.Diagnostic.Id != AnonymousDelegateEventUnsubscribe.DiagnosticId && context.Diagnostic.Id != TestMethodShouldContainAssertion.DiagnosticId)
                {
                    // Verifier expects all diagnostics to increase the counter in order to check that all rules call the
                    // extension method and not the direct `ReportDiagnostic`.
                    SuppressionHandler.IncrementReportCount(context.Diagnostic.Id);
                    context.ReportDiagnostic(context.Diagnostic);
                }
            };

            // Because the Verifier sets the SonarAnalysisContext.ShouldDiagnosticBeReported delegate we end up in a case
            // where the Debug.Assert of the AnalysisContextExtensions.ReportDiagnostic() method will raise.
            using (new AssertIgnoreScope())
            {
                foreach (var testCase in testCases)
                {
                    // special logic for rules with SyntaxNodeAnalysisContext
                    if (testCase.Analyzer is AnonymousDelegateEventUnsubscribe || testCase.Analyzer is TestMethodShouldContainAssertion)
                    {
                        testCase.Builder
                            .WithOptions(LanguageOptions.FromCSharp8)
                            .VerifyNoIssues();
                    }
                    else
                    {
                        testCase.Builder
                            .WithOptions(LanguageOptions.FromCSharp8)
                            .Verify();
                    }
                }
            }
        }
        finally
        {
            SonarAnalysisContext.ReportDiagnostic = null;
        }
    }

    [TestMethod]
    [DataRow(ProjectType.Product, false)]
    [DataRow(ProjectType.Test, true)]
    public void IsTestProject_Standalone(ProjectType projectType, bool expectedResult)
    {
        var compilation = new SnippetCompiler("// Nothing to see here", TestCompiler.ProjectTypeReference(projectType)).SemanticModel.Compilation;
        var context = new CompilationAnalysisContext(compilation, AnalysisScaffolding.CreateOptions(), null, null, default);
        var sut = new SonarCompilationReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.IsTestProject().Should().Be(expectedResult);
    }

    [TestMethod]
    [DataRow(ProjectType.Product, false)]
    [DataRow(ProjectType.Test, true)]
    public void IsTestProject_WithConfigFile(ProjectType projectType, bool expectedResult)
    {
        var configPath = AnalysisScaffolding.CreateSonarProjectConfig(TestContext, projectType);
        var context = new CompilationAnalysisContext(null, AnalysisScaffolding.CreateOptions(configPath), null, null, default);
        var sut = new SonarCompilationReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.IsTestProject().Should().Be(expectedResult);
    }

    [TestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void ReportDiagnosticIfNonGenerated_UnchangedFiles_CompilationAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var wasReported = false;
        var location = context.Tree.GetRoot().GetLocation();
        var symbol = Substitute.For<ISymbol>();
        symbol.Locations.Returns([location]);
        var symbolContext = new SymbolAnalysisContext(symbol, context.Model.Compilation, context.Options, _ => wasReported = true, _ => true, default);
        var sut = new SonarSymbolReportingContext(new SonarAnalysisContext(context, DummyMainDescriptor), symbolContext);
        sut.ReportIssue(CSharpGeneratedCodeRecognizer.Instance, DummyMainDescriptor[0], location);

        wasReported.Should().Be(expected);
    }

    private sealed class TestSetup
    {
        public string Path { get; }
        public DiagnosticAnalyzer Analyzer { get; }
        public VerifierBuilder Builder { get; }

        public TestSetup(string testCase, SonarDiagnosticAnalyzer analyzer) : this(testCase, analyzer, Enumerable.Empty<MetadataReference>()) { }

        public TestSetup(string testCase, SonarDiagnosticAnalyzer analyzer, IEnumerable<MetadataReference> additionalReferences)
        {
            Path = testCase;
            Analyzer = analyzer;
            additionalReferences = additionalReferences
                .Concat(MetadataReferenceFacade.SystemComponentModelPrimitives)
                .Concat(MetadataReferenceFacade.NetStandard)
                .Concat(MetadataReferenceFacade.SystemData);
            Builder = new VerifierBuilder().AddAnalyzer(() => analyzer).AddPaths(Path).AddReferences(additionalReferences);
        }
    }
}
