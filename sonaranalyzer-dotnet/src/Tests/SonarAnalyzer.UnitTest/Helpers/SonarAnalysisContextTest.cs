/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;

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

        private readonly List<TestSetup> TestCases = new List<TestSetup>(new []
        {
            new TestSetup { Path = @"TestCases\AnonymousDelegateEventUnsubscribe.cs", Analyzer = new AnonymousDelegateEventUnsubscribe() },
            new TestSetup { Path = @"TestCases\AsyncAwaitIdentifier.cs", Analyzer = new AsyncAwaitIdentifier() },
            new TestSetup { Path = @"TestCases\GetHashCodeEqualsOverride.cs", Analyzer = new GetHashCodeEqualsOverride() },
            new TestSetup { Path = @"TestCases\DisposeNotImplementingDispose.cs", Analyzer = new DisposeNotImplementingDispose() },
            new TestSetup { Path = @"TestCases\ClassShouldNotBeAbstract.cs", Analyzer = new ClassShouldNotBeAbstract() },
            new TestSetup { Path = @"TestCases\ClassName.cs", Analyzer = new ClassAndMethodName() }
        });

        [TestMethod]
        public void SonarAnalysis_NoIssueReportedIfAnalysisIsDisabled()
        {
            SonarAnalysisContext.ShouldAnalysisBeDisabled = tree => true;

            try
            {
                foreach (var testCase in TestCases)
                {
                    Verifier.VerifyNoIssueReported(testCase.Path, testCase.Analyzer);
                }
            }
            finally
            {
                SonarAnalysisContext.ShouldAnalysisBeDisabled = null;
            }
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
            TestCases.Count.Should().BeGreaterThan(2);

            try
            {
                SonarAnalysisContext.ShouldAnalysisBeDisabled = tree =>
                    tree.FilePath.EndsWith(new FileInfo(TestCases[0].Path).Name, System.StringComparison.OrdinalIgnoreCase);
                Verifier.VerifyNoIssueReported(TestCases[0].Path, TestCases[0].Analyzer);
                Verifier.VerifyAnalyzer(TestCases[1].Path, TestCases[1].Analyzer);
            }
            finally
            {
                SonarAnalysisContext.ShouldAnalysisBeDisabled = null;
            }
        }

        [TestMethod]
        public void SonarAnalysis_IssuesAreReportedByDefault()
        {
            DiagnosticDescriptor dummyDescriptor = new DiagnosticDescriptor(
                "x1", "title", "format", "category", DiagnosticSeverity.Error, false);
            Diagnostic dummyDiag = Diagnostic.Create(dummyDescriptor, Location.None);

            var shouldIssueBeReported = SonarAnalysisContext.ShouldDiagnosticBeReported;
            shouldIssueBeReported.Should().NotBeNull();

            shouldIssueBeReported(dummyDiag).Should().BeTrue();
        }
    }
}
