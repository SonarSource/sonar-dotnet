/*
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

using System.IO;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.Rules;

namespace SonarAnalyzer.UnitTest;

[TestClass]
public partial class SonarAnalysisContextTest
{
    public TestContext TestContext { get; set; }

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
                .Concat(NetStandardMetadataReference.Netstandard)
                .Concat(MetadataReferenceFacade.SystemData);
            Builder = new VerifierBuilder().AddAnalyzer(() => analyzer).AddPaths(Path).AddReferences(additionalReferences);
        }
    }

    // Various classes that invoke all the `ReportIssue` methods in AnalysisContextExtensions
    // We mention in comments the type of Context that is used to invoke (directly or indirectly) the `ReportIssue` method
    private readonly List<TestSetup> testCases = new(
        new[]
        {
            // SyntaxNodeAnalysisContext
            // S3244 - MAIN and TEST
            new TestSetup("AnonymousDelegateEventUnsubscribe.cs", new AnonymousDelegateEventUnsubscribe()),
            // S2699 - TEST only
            new TestSetup(
                "TestMethodShouldContainAssertion.NUnit.cs",
                new TestMethodShouldContainAssertion(),
                TestMethodShouldContainAssertionTest.WithTestReferences(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion)).References),    // ToDo: Reuse the entire builder in TestSetup

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
            new TestSetup("ClassShouldNotBeAbstract.cs", new ClassShouldNotBeAbstract()),
        });

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
                    .WithOptions(ParseOptionsHelper.FromCSharp8)
                    .VerifyNoIssueReported();
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
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();
        }
    }

    [TestMethod]
    public void WhenProjectType_IsTest_RunRulesWithTestScope_SonarLint()
    {
        var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(TestContext, ProjectType.Test, false);
        foreach (var testCase in testCases)
        {
            var hasTestScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorFactory.TestSourceScopeTag));
            if (hasTestScope)
            {
                testCase.Builder
                    .WithOptions(ParseOptionsHelper.FromCSharp8)
                    .WithSonarProjectConfigPath(sonarProjectConfig)
                    .Verify();
            }
            else
            {
                // MAIN-only
                testCase.Builder
                    .WithOptions(ParseOptionsHelper.FromCSharp8)
                    .WithSonarProjectConfigPath(sonarProjectConfig)
                    .VerifyNoIssueReported();
            }
        }
    }

    [TestMethod]
    public void WhenProjectType_IsTest_RunRulesWithTestScope_Scanner()
    {
        var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(TestContext, ProjectType.Test);
        foreach (var testCase in testCases)
        {
            var hasProductScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorFactory.MainSourceScopeTag));
            if (hasProductScope)
            {
                // MAIN-only and MAIN & TEST rules
                testCase.Builder
                    .WithOptions(ParseOptionsHelper.FromCSharp8)
                    .WithSonarProjectConfigPath(sonarProjectConfig)
                    .VerifyNoIssueReported();
            }
            else
            {
                testCase.Builder
                    .WithOptions(ParseOptionsHelper.FromCSharp8)
                    .WithSonarProjectConfigPath(sonarProjectConfig)
                    .Verify();
            }
        }
    }

    [TestMethod]
    public void WhenProjectType_IsTest_RunRulesWithMainScope()
    {
        var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(TestContext, ProjectType.Product);
        foreach (var testCase in testCases)
        {
            var hasProductScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorFactory.MainSourceScopeTag));
            if (hasProductScope)
            {
                testCase.Builder
                    .WithOptions(ParseOptionsHelper.FromCSharp8)
                    .WithSonarProjectConfigPath(sonarProjectConfig)
                    .Verify();
            }
            else
            {
                // TEST-only rule
                testCase.Builder
                    .WithOptions(ParseOptionsHelper.FromCSharp8)
                    .WithSonarProjectConfigPath(sonarProjectConfig)
                    .VerifyNoIssueReported();
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
            testCase2.Builder.VerifyNoIssueReported();
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
                    DiagnosticVerifier.SuppressionHandler.IncrementReportCount(context.Diagnostic.Id);
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
                            .WithOptions(ParseOptionsHelper.FromCSharp8)
                            .VerifyNoIssueReported();
                    }
                    else
                    {
                        testCase.Builder
                            .WithOptions(ParseOptionsHelper.FromCSharp8)
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
    public void ProjectConfiguration_LoadsExpectedValues()
    {
        var options = TestHelper.CreateOptions($@"ResourceTests\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");

        var sut = new SonarAnalysisContext(new DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());
        var config = sut.ProjectConfiguration(options);

        config.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
    }

    [TestMethod]
    public void ProjectConfiguration_UsesCachedValue()
    {
        var options = TestHelper.CreateOptions($@"ResourceTests\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");
        var context = new DummyContext();
        var firstSut = new SonarAnalysisContext(context, Enumerable.Empty<DiagnosticDescriptor>());
        var secondSut = new SonarAnalysisContext(context, Enumerable.Empty<DiagnosticDescriptor>());
        var firstConfig = firstSut.ProjectConfiguration(options);

        secondSut.ProjectConfiguration(options).Should().BeSameAs(firstConfig);
    }

    [TestMethod]
    public void ProjectConfiguration_WhenFileChanges_RebuildsCache()
    {
        var firstOptions = TestHelper.CreateOptions($@"ResourceTests\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");
        var secondOptions = TestHelper.CreateOptions($@"ResourceTests\SonarProjectConfig\Path_Unix\SonarProjectConfig.xml");
        var sut = new SonarAnalysisContext(new DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());
        var firstConfig = sut.ProjectConfiguration(firstOptions);

        sut.ProjectConfiguration(secondOptions).Should().NotBeSameAs(firstConfig);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("/foo/bar/does-not-exit")]
    [DataRow("/foo/bar/x.xml")]
    public void ProjectConfiguration_WhenAdditionalFileNotPresent_ReturnsEmptyConfig(string folder)
    {
        var options = TestHelper.CreateOptions(folder);

        var sut = new SonarAnalysisContext(new DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());
        var config = sut.ProjectConfiguration(options);

        config.AnalysisConfigPath.Should().BeNull();
        config.ProjectPath.Should().BeNull();
        config.FilesToAnalyzePath.Should().BeNull();
        config.OutPath.Should().BeNull();
        config.ProjectType.Should().Be(ProjectType.Unknown);
        config.TargetFramework.Should().BeNull();
    }

    [TestMethod]
    public void ProjectConfiguration_WhenFileIsMissing_ThrowException()
    {
        var options = TestHelper.CreateOptions("ThisPathDoesNotExist\\SonarProjectConfig.xml");

        var sut = new SonarAnalysisContext(new DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());

        sut.Invoking(x => x.ProjectConfiguration(options))
           .Should()
           .Throw<InvalidOperationException>()
           .WithMessage("File SonarProjectConfig.xml has been added as an AdditionalFile but could not be read and parsed.");
    }

    [TestMethod]
    public void ProjectConfiguration_WhenInvalidXml_ThrowException()
    {
        var options = TestHelper.CreateOptions($@"ResourceTests\SonarProjectConfig\Invalid_Xml\SonarProjectConfig.xml");

        var sut = new SonarAnalysisContext(new DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());

        sut.Invoking(x => x.ProjectConfiguration(options))
           .Should()
           .Throw<InvalidOperationException>()
           .WithMessage("File SonarProjectConfig.xml has been added as an AdditionalFile but could not be read and parsed.");
    }

    [TestMethod]
    public void IsTestProject_Standalone_NoCompilation_IsFalse()
    {
        var options = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);
        var context = new SonarAnalysisContext(new DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());

        context.IsTestProject(null, options).Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow(ProjectType.Product, false)]
    [DataRow(ProjectType.Test, true)]
    public void IsTestProject_Standalone(ProjectType projectType, bool expectedResult)
    {
        var compilation = new SnippetCompiler("// Nothing to see here", TestHelper.ProjectTypeReference(projectType)).SemanticModel.Compilation;
        var options = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);
        var context = new SonarAnalysisContext(new DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());

        context.IsTestProject(compilation, options).Should().Be(expectedResult);
    }

    [DataTestMethod]
    [DataRow(ProjectType.Product, false)]
    [DataRow(ProjectType.Test, true)]
    public void IsTestProject_WithConfigFile(ProjectType projectType, bool expectedResult)
    {
        var configPath = TestHelper.CreateSonarProjectConfig(TestContext, projectType);
        var context = new CompilationAnalysisContext(null, TestHelper.CreateOptions(configPath), null, null, default);

        SonarAnalysisContext.IsTestProject(context).Should().Be(expectedResult);
    }

    internal class DummyContext : AnalysisContext
    {
        public override void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) => throw new NotImplementedException();
        public override void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) => throw new NotImplementedException();
        public override void RegisterCompilationAction(Action<CompilationAnalysisContext> action) => throw new NotImplementedException();
        public override void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) => throw new NotImplementedException();
        public override void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action) => throw new NotImplementedException();
        public override void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds) => throw new NotImplementedException();
        public override void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) => throw new NotImplementedException();
        public override void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action) => throw new NotImplementedException();
    }
}
