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
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class SonarAnalysisContextTest
    {
        private class TestSetup
        {
            public string Path { get; set; }
            public SonarDiagnosticAnalyzer Analyzer { get; set; }
        }

        private readonly List<TestSetup> testCases = new List<TestSetup>(new[]
        {
            new TestSetup { Path = @"TestCases\AnonymousDelegateEventUnsubscribe.cs", Analyzer = new AnonymousDelegateEventUnsubscribe() },
            new TestSetup { Path = @"TestCases\AsyncAwaitIdentifier.cs", Analyzer = new AsyncAwaitIdentifier() },
            new TestSetup { Path = @"TestCases\GetHashCodeEqualsOverride.cs", Analyzer = new GetHashCodeEqualsOverride() },
            new TestSetup { Path = @"TestCases\DisposeNotImplementingDispose.cs", Analyzer = new DisposeNotImplementingDispose() },
            new TestSetup { Path = @"TestCases\ClassShouldNotBeAbstract.cs", Analyzer = new ClassShouldNotBeAbstract() },
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
                                                   MetadataReferenceFacade.SystemComponentModelPrimitives);
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
                                        MetadataReferenceFacade.SystemComponentModelPrimitives);
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
                Verifier.VerifyNoIssueReported(testCases[1].Path, testCases[1].Analyzer);
            }
            finally
            {
                SonarAnalysisContext.ShouldExecuteRegisteredAction = null;
            }
        }

        [TestMethod]
        public void WhenReportDiagnosticActionNotNull_AllowToContolWhetherOrNotToReport()
        {
            try
            {
                SonarAnalysisContext.ReportDiagnostic = context =>
                {
                    if (context.Diagnostic.Id != AnonymousDelegateEventUnsubscribe.DiagnosticId)
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
                        if (testCase.Analyzer is AnonymousDelegateEventUnsubscribe)
                        {
                            Verifier.VerifyNoIssueReported(testCase.Path,
                                                           testCase.Analyzer,
                                                           ParseOptionsHelper.FromCSharp8,
                                                           MetadataReferenceFacade.SystemComponentModelPrimitives);
                        }
                        else
                        {
                            Verifier.VerifyAnalyzer(testCase.Path,
                                                    testCase.Analyzer,
                                                    ParseOptionsHelper.FromCSharp8,
                                                    MetadataReferenceFacade.SystemComponentModelPrimitives);
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

            var sut = new SonarAnalysisContext(null, Enumerable.Empty<DiagnosticDescriptor>());
            var config = sut.ProjectConfiguration(options);

            config.AnalysisConfigPath.Should().BeNull();
            config.ProjectPath.Should().BeNull();
            config.FilesToAnalyzePath.Should().BeNull();
            config.OutPath.Should().BeNull();
            config.ProjectType.Should().Be(ProjectType.Product);
            config.TargetFramework.Should().BeNull();
        }

        [TestMethod]
        public void ProjectConfiguration_WhenFileIsMissing_ThrowException()
        {
            var options = TestHelper.CreateOptions("ThisPathDoesNotExist\\SonarProjectConfig.xml");

            var sut = new SonarAnalysisContext(null, Enumerable.Empty<DiagnosticDescriptor>());

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

        private class DummyContext : AnalysisContext
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
