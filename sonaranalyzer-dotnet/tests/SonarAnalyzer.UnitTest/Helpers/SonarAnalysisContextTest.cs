/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2018 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using csharp::SonarAnalyzer.Rules.CSharp;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

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
        public void SonarAnalysis_NoIssueReportedIfAnalysisIsDisabled()
        {
            TestAndReset(
                () =>
                {
                    SonarAnalysisContext.ShouldAnalyze = context => false;

                    foreach (var testCase in TestCases)
                    {
                        Verifier.VerifyNoIssueReported(testCase.Path, testCase.Analyzer);
                    }
                });
        }

        [TestMethod]
        public void SonarAnalysis_IsAbleToDisableSpecificRules()
        {
            TestAndReset(
                () =>
                {
                    SonarAnalysisContext.ShouldAnalyze = context =>
                        context.SupportedDiagnostics.All(d => d.Id != AnonymousDelegateEventUnsubscribe.DiagnosticId);

                    foreach (var testCase in TestCases)
                    {
                        if (testCase.Analyzer.GetType() == typeof(AnonymousDelegateEventUnsubscribe))
                        {
                            Verifier.VerifyNoIssueReported(testCase.Path, testCase.Analyzer);
                        }
                        else
                        {
                            Verifier.VerifyAnalyzer(testCase.Path, testCase.Analyzer);
                        }
                    }
                });
        }

        [TestMethod]
        public void SonarAnalysis_IssueReportedIfAnalysisIsEnabled()
        {
            foreach (var testCase in TestCases)
            {
                Verifier.VerifyAnalyzer(testCase.Path, testCase.Analyzer);
            }
        }

        [TestMethod]
        public void SonarAnalysis_SpecificIssueTurnedOff()
        {
            TestAndReset(
                () =>
                {
                    TestCases.Count.Should().BeGreaterThan(2);

                    SonarAnalysisContext.ShouldAnalyze = context =>
                    !context.SyntaxTree.FilePath.EndsWith(new FileInfo(TestCases[0].Path).Name, System.StringComparison.OrdinalIgnoreCase);
                    Verifier.VerifyNoIssueReported(TestCases[0].Path, TestCases[0].Analyzer);
                    Verifier.VerifyAnalyzer(TestCases[1].Path, TestCases[1].Analyzer);
                });
        }

        private static void TestAndReset(Action action)
        {
            var defaultState = SonarAnalysisContext.ShouldAnalyze;
            try
            {
                action();
            }
            finally
            {
                SonarAnalysisContext.ShouldAnalyze = defaultState;
            }
        }
    }
}
