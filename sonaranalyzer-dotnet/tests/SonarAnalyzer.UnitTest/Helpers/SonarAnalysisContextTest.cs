/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using System.Collections.Generic;
using System.IO;
using csharp::SonarAnalyzer.Rules.CSharp;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
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

        private readonly List<TestSetup> TestCases = new List<TestSetup>(new[]
        {
            new TestSetup { Path = @"TestCases\AnonymousDelegateEventUnsubscribe.cs", Analyzer = new AnonymousDelegateEventUnsubscribe() },
            new TestSetup { Path = @"TestCases\AsyncAwaitIdentifier.cs", Analyzer = new AsyncAwaitIdentifier() },
            new TestSetup { Path = @"TestCases\GetHashCodeEqualsOverride.cs", Analyzer = new GetHashCodeEqualsOverride() },
            new TestSetup { Path = @"TestCases\DisposeNotImplementingDispose.cs", Analyzer = new DisposeNotImplementingDispose() },
            new TestSetup { Path = @"TestCases\ClassShouldNotBeAbstract.cs", Analyzer = new ClassShouldNotBeAbstract() },
        });

        [TestMethod]
        public void SonarAnalysis_WhenShouldAnalysisBeDisabledReturnsTrue_NoIssueReported()
        {
            SonarAnalysisContext.ShouldExecuteRegisteredAction = (diags, tree )=> false;

            try
            {
                foreach (var testCase in this.TestCases)
                {
                    // TODO: We should find a way to ack the fact the action was not run
                    Verifier.VerifyNoIssueReported(testCase.Path, testCase.Analyzer);
                }
            }
            finally
            {
                SonarAnalysisContext.ShouldExecuteRegisteredAction = null;
            }
        }

        [TestMethod]
        public void SonarAnalysis_ByDefault_ExecuteRule()
        {
            foreach (var testCase in this.TestCases)
            {
                // FIX ME: We test that a rule is enabled only by checking the issues are reported
                Verifier.VerifyAnalyzer(testCase.Path, testCase.Analyzer);
            }
        }

        [TestMethod]
        public void SonarAnalysis_WhenAnalysisDisabledBaseOnSyntaxTree_ReportIssuesForEnabledRules()
        {
            this.TestCases.Should().HaveCountGreaterThan(2);

            try
            {
                SonarAnalysisContext.ShouldExecuteRegisteredAction = (diags, tree) =>
                    tree.FilePath.EndsWith(new FileInfo(this.TestCases[0].Path).Name, System.StringComparison.OrdinalIgnoreCase);
                Verifier.VerifyAnalyzer(this.TestCases[0].Path, this.TestCases[0].Analyzer);
                Verifier.VerifyNoIssueReported(this.TestCases[1].Path, this.TestCases[1].Analyzer);
            }
            finally
            {
                SonarAnalysisContext.ShouldExecuteRegisteredAction = null;
            }
        }

        [TestMethod]
        public void SonarAnalysis_WhenReportDiagnosticActionNotNull_AllowToContolWhetherOrNotToReport()
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
                    foreach (var testCase in this.TestCases)
                    {
                        if (testCase.Analyzer is AnonymousDelegateEventUnsubscribe)
                        {
                            Verifier.VerifyNoIssueReported(testCase.Path, testCase.Analyzer);
                        }
                        else
                        {
                            Verifier.VerifyAnalyzer(testCase.Path, testCase.Analyzer);
                        }
                    }
                }
            }
            finally
            {
                SonarAnalysisContext.ReportDiagnostic = null;
            }
        }
    }
}
