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
        private const string UtilityTag = "Utility";

        private class TestSetup
        {
            public string Path { get; }
            public DiagnosticAnalyzer Analyzer { get; }
            public IEnumerable<MetadataReference> AdditionalReferences { get; }

            public TestSetup(string testCase, SonarDiagnosticAnalyzer analyzer) : this(testCase, analyzer, Enumerable.Empty<MetadataReference>()) { }

            public TestSetup(string testCase, SonarDiagnosticAnalyzer analyzer, IEnumerable<MetadataReference> additionalReferences)
            {
                Path = testCase;
                Analyzer = analyzer;
                AdditionalReferences = additionalReferences.Concat(MetadataReferenceFacade.SystemComponentModelPrimitives)
                                                           .Concat(NetStandardMetadataReference.Netstandard)
                                                           .Concat(MetadataReferenceFacade.SystemData)
                                                           .Concat(MetadataReferenceFacade.SystemNetHttp);
            }
        }

        // Various classes that invoke all the `ReportIssue` methods in AnalysisContextExtensions
        // We mention in comments the type of Context that is used to invoke (directly or indirectly) the `ReportIssue` method
        private readonly Dictionary<TestSetup, VerifierBuilder> testCases = new()
            {
            // SyntaxNodeAnalysisContext
            // S3244 - MAIN and TEST
            { new TestSetup("AnonymousDelegateEventUnsubscribe.cs", new AnonymousDelegateEventUnsubscribe()), new VerifierBuilder<AnonymousDelegateEventUnsubscribe>() },
                // S2699 - TEST only
            {
                new TestSetup(
                "TestMethodShouldContainAssertion.NUnit.cs",
                new TestMethodShouldContainAssertion(),
                TestMethodShouldContainAssertionTest.AdditionalTestReferences(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion))),
                new VerifierBuilder<TestMethodShouldContainAssertion>()
            },

            // SyntaxTreeAnalysisContext
            // S3244 - MAIN and TEST
            { new TestSetup("AsyncAwaitIdentifier.cs", new AsyncAwaitIdentifier()), new VerifierBuilder<AsyncAwaitIdentifier>() },

            // CompilationAnalysisContext
            // S3244 - MAIN and TEST
            {
                new TestSetup(
                    @"Hotspots\RequestsWithExcessiveLength.cs",
                    new RequestsWithExcessiveLength(AnalyzerConfiguration.AlwaysEnabled),
                    RequestsWithExcessiveLengthTest.GetAdditionalReferences()),
                new VerifierBuilder().AddAnalyzer(() => new RequestsWithExcessiveLength(AnalyzerConfiguration.AlwaysEnabled))
            },

            // CodeBlockAnalysisContext
            // S5693 - MAIN and TEST
            { new TestSetup("GetHashCodeEqualsOverride.cs", new GetHashCodeEqualsOverride()), new VerifierBuilder<GetHashCodeEqualsOverride>() },

            // SymbolAnalysisContext
            // S2953 - MAIN only
            { new TestSetup("DisposeNotImplementingDispose.cs", new DisposeNotImplementingDispose()), new VerifierBuilder<DisposeNotImplementingDispose>() },
            // S1694 - MAIN only
            { new TestSetup("ClassShouldNotBeAbstract.cs", new ClassShouldNotBeAbstract()), new VerifierBuilder<ClassShouldNotBeAbstract>() }
        };

        [TestMethod]
        public void WhenShouldAnalysisBeDisabledReturnsTrue_NoIssueReported()
        {
            SonarAnalysisContext.ShouldExecuteRegisteredAction = (diags, tree) => false;

            try
            {
                foreach (var entry in testCases)
                {
                    // ToDo: We should find a way to ack the fact the action was not run
                    var builder = entry.Value;
                    var testCase = entry.Key;

                    builder.AddPaths(testCase.Path)
                        .WithOptions(ParseOptionsHelper.FromCSharp8)
                        .AddReferences(testCase.AdditionalReferences)
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
            foreach (var entry in testCases)
            {
                // ToDo: We test that a rule is enabled only by checking the issues are reported
                var builder = entry.Value;
                var testCase = entry.Key;

                builder.AddPaths(testCase.Path)
                    .WithOptions(ParseOptionsHelper.FromCSharp8)
                    .AddReferences(testCase.AdditionalReferences)
                    .Verify();
            }
        }

        [TestMethod]
        public void WhenProjectType_IsTest_RunRulesWithTestScope_SonarLint()
        {
            var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(nameof(WhenProjectType_IsTest_RunRulesWithTestScope_SonarLint), ProjectType.Test, false);
            foreach (var entry in testCases)
            {
                var builder = entry.Value;
                var testCase = entry.Key;

                var hasTestScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorBuilder.TestSourceScopeTag));
                if (hasTestScope)
                {
                    builder.AddPaths(testCase.Path)
                        .WithOptions(ParseOptionsHelper.FromCSharp8)
                        .AddReferences(testCase.AdditionalReferences)
                        .WithSonarProjectConfigPath(sonarProjectConfig)
                        .Verify();

                }
                else
                {
                    // MAIN-only
                    builder.AddPaths(testCase.Path)
                        .WithOptions(ParseOptionsHelper.FromCSharp8)
                        .AddReferences(testCase.AdditionalReferences)
                        .WithSonarProjectConfigPath(sonarProjectConfig)
                        .VerifyNoIssueReported();
                }
            }
        }

        [TestMethod]
        public void WhenProjectType_IsTest_RunRulesWithTestScope_Scanner()
        {
            var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(nameof(WhenProjectType_IsTest_RunRulesWithTestScope_Scanner), ProjectType.Test);
            foreach (var entry in testCases)
            {
                var builder = entry.Value;
                var testCase = entry.Key;

                var hasProductScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorBuilder.MainSourceScopeTag));
                if (hasProductScope)
                {
                    // MAIN-only and MAIN & TEST rules
                    builder.AddPaths(testCase.Path)
                        .WithOptions(ParseOptionsHelper.FromCSharp8)
                        .AddReferences(testCase.AdditionalReferences)
                        .WithSonarProjectConfigPath(sonarProjectConfig)
                        .VerifyNoIssueReported();
                }
                else
                {
                    builder.AddPaths(testCase.Path)
                        .WithOptions(ParseOptionsHelper.FromCSharp8)
                        .AddReferences(testCase.AdditionalReferences)
                        .WithSonarProjectConfigPath(sonarProjectConfig)
                        .Verify();
                }
            }
        }

        [TestMethod]
        public void WhenProjectType_IsTest_RunRulesWithMainScope()
        {
            var sonarProjectConfig = TestHelper.CreateSonarProjectConfig(nameof(WhenProjectType_IsTest_RunRulesWithMainScope), ProjectType.Product);
            foreach (var entry in testCases)
            {
                var builder = entry.Value;
                var testCase = entry.Key;
                var hasProductScope = testCase.Analyzer.SupportedDiagnostics.Any(d => d.CustomTags.Contains(DiagnosticDescriptorBuilder.MainSourceScopeTag));
                if (hasProductScope)
                {
                    builder.AddPaths(testCase.Path)
                        .WithOptions(ParseOptionsHelper.FromCSharp8)
                        .AddReferences(testCase.AdditionalReferences)
                        .WithSonarProjectConfigPath(sonarProjectConfig)
                        .Verify();
                }
                else
                {
                    // TEST-only rule
                    builder.AddPaths(testCase.Path)
                        .WithOptions(ParseOptionsHelper.FromCSharp8)
                        .AddReferences(testCase.AdditionalReferences)
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
                var (testCase, builder) = testCases.Where(x => x.Key.Analyzer is AnonymousDelegateEventUnsubscribe).Select(x => (x.Key, x.Value)).FirstOrDefault();
                var (testCase2, builder2) = testCases.Where(x => x.Key.Analyzer is AsyncAwaitIdentifier).Select(x => (x.Key, x.Value)).FirstOrDefault();
                SonarAnalysisContext.ShouldExecuteRegisteredAction = (diags, tree) => tree.FilePath.EndsWith(new FileInfo(testCase.Path).Name, StringComparison.OrdinalIgnoreCase);
                builder.AddPaths(testCase.Path).WithConcurrentAnalysis(false).Verify();
                builder2.AddPaths(testCase2.Path).VerifyNoIssueReported();
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
                    foreach (var entry in testCases)
                    {
                        var builder = entry.Value;
                        var testCase = entry.Key;
                        // special logic for rules with SyntaxNodeAnalysisContext
                        if (testCase.Analyzer is AnonymousDelegateEventUnsubscribe || testCase.Analyzer is TestMethodShouldContainAssertion)
                        {
                            builder.AddPaths(testCase.Path)
                                .WithOptions(ParseOptionsHelper.FromCSharp8)
                                .AddReferences(testCase.AdditionalReferences)
                                .VerifyNoIssueReported();
                        }
                        else
                        {
                            builder.AddPaths(testCase.Path)
                                .WithOptions(ParseOptionsHelper.FromCSharp8)
                                .AddReferences(testCase.AdditionalReferences)
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
        [DataRow(true, ProjectType.Product, MainTag, UtilityTag)]
        [DataRow(true, ProjectType.Product, MainTag, TestTag)]
        [DataRow(true, ProjectType.Product, MainTag, TestTag, UtilityTag)]
        [DataRow(true, ProjectType.Test, TestTag)]
        [DataRow(true, ProjectType.Test, TestTag, UtilityTag)]
        [DataRow(true, ProjectType.Test, MainTag, TestTag)]
        [DataRow(true, ProjectType.Test, MainTag, TestTag, UtilityTag)]
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
        [DataRow(true, ProjectType.Product, MainTag, UtilityTag)]
        [DataRow(true, ProjectType.Product, MainTag, TestTag)]
        [DataRow(true, ProjectType.Product, MainTag, TestTag, UtilityTag)]
        [DataRow(true, ProjectType.Test, TestTag)]
        [DataRow(true, ProjectType.Test, TestTag, UtilityTag)]
        [DataRow(true, ProjectType.Test, MainTag, TestTag, UtilityTag)]     // Utility rules with scope Test&Main do run on test code under scanner context.
        [DataRow(false, ProjectType.Test, MainTag, TestTag)]                // Rules with scope Test&Main do not run on test code under scanner context for now.
        [DataRow(false, ProjectType.Product, TestTag)]
        [DataRow(false, ProjectType.Product, TestTag, UtilityTag)]
        [DataRow(false, ProjectType.Product, TestTag, TestTag)]
        [DataRow(false, ProjectType.Test, MainTag)]
        [DataRow(false, ProjectType.Test, MainTag, UtilityTag)]
        [DataRow(false, ProjectType.Test, MainTag, MainTag)]
        public void IsAnalysisScopeMatching_SingleDiagnostis_WithOneOrMoreScopes_Scanner(bool expectedResult, ProjectType projectType, params string[] ruleTags)
        {
            var compilation = new SnippetCompiler("// Nothing to see here").SemanticModel.Compilation;
            var diagnostic = new DiagnosticDescriptor("Sxxx", "Title", "Message", "Category", DiagnosticSeverity.Warning, true, customTags: ruleTags);
            SonarAnalysisContext.IsAnalysisScopeMatching(compilation, projectType == ProjectType.Test, true, new[] { diagnostic }).Should().Be(expectedResult);
        }

        [DataTestMethod]
        [DataRow(true, ProjectType.Product, MainTag, MainTag)]
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
        [DataRow(true, ProjectType.Test, TestTag, MainTag)]    // Rules with scope Test&Main will run to let the Test diagnostics to be detected. ReportDiagnostic should filter Main issues out.
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
