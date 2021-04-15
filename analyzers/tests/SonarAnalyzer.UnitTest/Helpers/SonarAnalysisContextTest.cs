/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.Rules;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class SonarAnalysisContextTest
    {
        private const string MainTag = "MainSourceScope";
        private const string TestTag = "TestSourceScope";

        private class TestSetup
        {
            public string Path { get; private set; }
            public SonarDiagnosticAnalyzer Analyzer { get; private set; }
            public IEnumerable<MetadataReference> AdditionalReferences { get; private set; }

            public TestSetup(string testCase, SonarDiagnosticAnalyzer analyzer) : this(testCase, analyzer, Enumerable.Empty<MetadataReference>()) { }

            public TestSetup(string testCase, SonarDiagnosticAnalyzer analyzer, IEnumerable<MetadataReference> additionalReferences)
            {
                Path = System.IO.Path.Combine("TestCases", testCase);
                Analyzer = analyzer;
                AdditionalReferences = additionalReferences.Concat(MetadataReferenceFacade.SystemComponentModelPrimitives).Concat(NetStandardMetadataReference.Netstandard);
            }
        }

        // Various classes that invoke all the `ReportDiagnosticWhenActive` methods in AnalysisContextExtensions
        // We mention in comments the type of Context that is used to invoke (directly or indirectly) the `ReportDiagnosticWhenActive` method
        private readonly List<TestSetup> testCases = new (new[]
        {
            // SyntaxNodeAnalysisContext
            // S3244 - MAIN and TEST
            new TestSetup("AnonymousDelegateEventUnsubscribe.cs", new AnonymousDelegateEventUnsubscribe()),
            // S2699 - TEST only
            new TestSetup("TestMethodShouldContainAssertion.MsTest.cs", new TestMethodShouldContainAssertion(), TestMethodShouldContainAssertionTest.GetMsTestReferences(Constants.NuGetLatestVersion)),

            // SyntaxTreeAnalysisContext
            // S3244 - MAIN and TEST
            new TestSetup("AsyncAwaitIdentifier.cs", new AsyncAwaitIdentifier()),

            // CompilationAnalysisContext
            // S3244 - MAIN and TEST
            new TestSetup(@"Hotspots\RequestsWithExcessiveLength.cs", new RequestsWithExcessiveLength(AnalyzerConfiguration.AlwaysEnabled), RequestsWithExcessiveLengthTest.GetAdditionalReferences()),

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
            SonarAnalysisContext.ShouldExecuteRegisteredAction = (diags, tree) => false;

            try
            {
                foreach (var testCase in testCases)
                {
                    // ToDo: We should find a way to ack the fact the action was not run
                    Verifier.VerifyNoIssueReported(testCase.Path,
                                                   testCase.Analyzer,
                                                   ParseOptionsHelper.FromCSharp8,
                                                   testCase.AdditionalReferences);
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
                Verifier.VerifyAnalyzer(testCase.Path,
                                        testCase.Analyzer,
                                        ParseOptionsHelper.FromCSharp8,
                                        testCase.AdditionalReferences);
            }
        }

        [TestMethod]
        public void WhenProjectType_IsTest_RunRulesWithTestScope_SonarLint()
        {
            var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(nameof(WhenProjectType_IsTest_RunRulesWithTestScope_SonarLint), ProjectType.Test, false);
            foreach (var testCase in testCases)
            {
                var hasTestScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorBuilder.TestSourceScopeTag));
                if (hasTestScope)
                {
                    Verifier.VerifyAnalyzer(testCase.Path,
                                            testCase.Analyzer,
                                            ParseOptionsHelper.FromCSharp8,
                                            testCase.AdditionalReferences,
                                            sonarProjectConfig);
                }
                else
                {
                    // MAIN-only
                    Verifier.VerifyNoIssueReported(testCase.Path,
                                                   testCase.Analyzer,
                                                   ParseOptionsHelper.FromCSharp8,
                                                   testCase.AdditionalReferences,
                                                   sonarProjectConfig);
                }
            }
        }

        [TestMethod]
        public void WhenProjectType_IsTest_RunRulesWithTestScope_Scanner()
        {
            var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(nameof(WhenProjectType_IsTest_RunRulesWithTestScope_Scanner), ProjectType.Test);
            foreach (var testCase in testCases)
            {
                var hasMainScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorBuilder.MainSourceScopeTag));
                if (hasMainScope)
                {
                    // MAIN-only and MAIN & TEST rules
                    Verifier.VerifyNoIssueReported(testCase.Path,
                                                   testCase.Analyzer,
                                                   ParseOptionsHelper.FromCSharp8,
                                                   testCase.AdditionalReferences,
                                                   sonarProjectConfig);
                }
                else
                {
                    Verifier.VerifyAnalyzer(testCase.Path,
                                            testCase.Analyzer,
                                            ParseOptionsHelper.FromCSharp8,
                                            testCase.AdditionalReferences,
                                            sonarProjectConfig);
                }
            }
        }

        [TestMethod]
        public void WhenProjectType_IsTest_RunRulesWithMainScope()
        {
            var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(nameof(WhenProjectType_IsTest_RunRulesWithMainScope), ProjectType.Product);
            foreach (var testCase in testCases)
            {
                var hasProductScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorBuilder.MainSourceScopeTag));
                if (hasProductScope)
                {
                    Verifier.VerifyAnalyzer(testCase.Path,
                                            testCase.Analyzer,
                                            ParseOptionsHelper.FromCSharp8,
                                            testCase.AdditionalReferences,
                                            sonarProjectConfig);
                }
                else
                {
                    // TEST-only rule
                    Verifier.VerifyNoIssueReported(testCase.Path,
                                                   testCase.Analyzer,
                                                   ParseOptionsHelper.FromCSharp8,
                                                   testCase.AdditionalReferences,
                                                   sonarProjectConfig);
                }
            }
        }

        [TestMethod]
        public void WhenAnalysisDisabledBaseOnSyntaxTree_ReportIssuesForEnabledRules()
        {
            testCases.Should().HaveCountGreaterThan(2);

            try
            {
                SonarAnalysisContext.ShouldExecuteRegisteredAction = (diags, tree) => tree.FilePath.EndsWith(new FileInfo(testCases[0].Path).Name, StringComparison.OrdinalIgnoreCase);
                Verifier.VerifyAnalyzer(testCases[0].Path, testCases[0].Analyzer);
                Verifier.VerifyNoIssueReported(testCases[2].Path, testCases[2].Analyzer);
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
                            Verifier.VerifyNoIssueReported(testCase.Path,
                                                           testCase.Analyzer,
                                                           ParseOptionsHelper.FromCSharp8,
                                                           testCase.AdditionalReferences);
                        }
                        else
                        {
                            Verifier.VerifyAnalyzer(testCase.Path,
                                                    testCase.Analyzer,
                                                    ParseOptionsHelper.FromCSharp8,
                                                    testCase.AdditionalReferences);
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

            sut.Invoking(x => x.ProjectConfiguration(options)).Should().Throw<InvalidOperationException>()
                .WithMessage("File SonarProjectConfig.xml has been added as an AdditionalFile but could not be read and parsed.");
        }

        [TestMethod]
        public void ProjectConfiguration_WhenInvalidXml_ThrowException()
        {
            var options = TestHelper.CreateOptions($@"ResourceTests\SonarProjectConfig\Invalid_Xml\SonarProjectConfig.xml");

            var sut = new SonarAnalysisContext(new DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());

            sut.Invoking(x => x.ProjectConfiguration(options)).Should().Throw<InvalidOperationException>()
                .WithMessage("File SonarProjectConfig.xml has been added as an AdditionalFile but could not be read and parsed.");
        }

        [TestMethod]
        public void IsAnalysisScopeMatching_NoCompilation_IsMatching() =>
            SonarAnalysisContext.IsAnalysisScopeMatching(null, true, false, null).Should().BeTrue();

        [DataTestMethod]
        [DataRow(true, ProjectType.Product, MainTag)]
        [DataRow(true, ProjectType.Product, MainTag, TestTag)]
        [DataRow(true, ProjectType.Test, TestTag)]
        [DataRow(true, ProjectType.Test, MainTag, TestTag)]
        [DataRow(false, ProjectType.Product, TestTag)]
        [DataRow(false, ProjectType.Product, TestTag, TestTag)]
        [DataRow(false, ProjectType.Test, MainTag)]
        [DataRow(false, ProjectType.Test, MainTag, MainTag)]
        public void IsAnalysisScopeMatching_SingleDiagnostis_WithOneOrMoreScopes_SonarLint(bool expectedResult, ProjectType projectType, params string[] ruleTags)
        {
            var compilation = new SnippetCompiler("// Nothing to see here").SemanticModel.Compilation;
            var diagnostic = new DiagnosticDescriptor("Sxxx", "Title", "Message", "Category", DiagnosticSeverity.Warning, true, customTags: ruleTags);
            SonarAnalysisContext.IsAnalysisScopeMatching(compilation, projectType == ProjectType.Test, false, new[] { diagnostic }).Should().Be(expectedResult);
        }

        [DataTestMethod]
        [DataRow(true, ProjectType.Product, MainTag)]
        [DataRow(true, ProjectType.Product, MainTag, TestTag)]
        [DataRow(true, ProjectType.Test, TestTag)]
        [DataRow(false, ProjectType.Test, MainTag, TestTag)]    // Rules with scope Test&Main do not run on test code under scanner context for now.
        [DataRow(false, ProjectType.Product, TestTag)]
        [DataRow(false, ProjectType.Product, TestTag, TestTag)]
        [DataRow(false, ProjectType.Test, MainTag)]
        [DataRow(false, ProjectType.Test, MainTag, MainTag)]
        public void IsAnalysisScopeMatching_SingleDiagnostis_WithOneOrMoreScopes_Scanner(bool expectedResult, ProjectType projectType, params string[] ruleTags)
        {
            var compilation = new SnippetCompiler("// Nothing to see here").SemanticModel.Compilation;
            var diagnostic = new DiagnosticDescriptor("Sxxx", "Title", "Message", "Category", DiagnosticSeverity.Warning, true, customTags: ruleTags);
            SonarAnalysisContext.IsAnalysisScopeMatching(compilation, projectType == ProjectType.Test, true, new[] { diagnostic }).Should().Be(expectedResult);
        }

        [DataTestMethod]
        [DataRow(true, ProjectType.Product, MainTag, MainTag)]
        [DataRow(true, ProjectType.Product, MainTag, TestTag)]
        [DataRow(true, ProjectType.Test, TestTag, TestTag)]
        [DataRow(true, ProjectType.Test, TestTag, MainTag)]
        [DataRow(false, ProjectType.Product, TestTag, TestTag)]
        [DataRow(false, ProjectType.Test, MainTag, MainTag)]
        public void IsAnalysisScopeMatching_MultipleDiagnostics_WithSingleScope_SonarLint(bool expectedResult, ProjectType projectType, params string[] rulesTag)
        {
            var compilation = new SnippetCompiler("// Nothing to see here").SemanticModel.Compilation;
            var diagnostics = rulesTag.Select(x => new DiagnosticDescriptor("Sxxx", "Title", "Message", "Category", DiagnosticSeverity.Warning, true, customTags: new[] { x }));
            SonarAnalysisContext.IsAnalysisScopeMatching(compilation, projectType == ProjectType.Test, false, diagnostics).Should().Be(expectedResult);
        }

        [DataTestMethod]
        [DataRow(true, ProjectType.Product, MainTag, MainTag)]
        [DataRow(true, ProjectType.Product, MainTag, TestTag)]
        [DataRow(true, ProjectType.Test, TestTag, TestTag)]
        [DataRow(false, ProjectType.Test, TestTag, MainTag)]    // Rules with scope Test&Main do not run on test code under scanner context for now.
        [DataRow(false, ProjectType.Product, TestTag, TestTag)]
        [DataRow(false, ProjectType.Test, MainTag, MainTag)]
        public void IsAnalysisScopeMatching_MultipleDiagnostics_WithSingleScope_Scanner(bool expectedResult, ProjectType projectType, params string[] rulesTag)
        {
            var compilation = new SnippetCompiler("// Nothing to see here").SemanticModel.Compilation;
            var diagnostics = rulesTag.Select(x => new DiagnosticDescriptor("Sxxx", "Title", "Message", "Category", DiagnosticSeverity.Warning, true, customTags: new[] { x }));
            SonarAnalysisContext.IsAnalysisScopeMatching(compilation, projectType == ProjectType.Test, true, diagnostics).Should().Be(expectedResult);
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
            var configPath = TestHelper.CreateSonarProjectConfig(nameof(IsTestProject_WithConfigFile), projectType);
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
}
