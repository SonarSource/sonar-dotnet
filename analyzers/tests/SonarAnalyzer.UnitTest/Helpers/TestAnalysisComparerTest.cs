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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class TestAnalysisComparerTest
    {
        [TestMethod]
        public void CompareShouldListAllDifferences()
        {
            const string expected = @"Comparing actual issues with expected:
Language version: C# 10.

TestAnalysisComparer_1.cs
Line 8: unexpected primary issue S3168 with message 'Return 'Task' instead.'.
Line 10: primary issue S3168 message 'Wrong message' does not match the actual message 'Return 'Task' instead.'.
Line 12: primary issue S3168 should start on column 19 but got column 14.
Line 15: primary issue S3168 should have a length of 5 but got a length of 4.
Line 3: expected primary issue (no id) was not raised!

TestAnalysisComparer_2.cs
Line 7: primary issue S3168 should have a length of 5 but got a length of 4.
Line 10: secondary issue flow-1 message 'Wrong message' does not match the actual message ''.
Line 17: primary issue flow-0 message 'Wrong message' does not match the actual message 'Update this method so that its implementation is not identical to 'Default'.'.
Line 14: expected secondary issue flow-2 was not raised!
";

            var compilation = GetCompilation(new[] { "TestCases/TestAnalysisComparer_1.cs", "TestCases/TestAnalysisComparer_2.cs"});
            var diagnostics = GetDiagnostics(compilation);
            var expectedIssues = GetExpectedIssues(compilation);

            var summary = TestAnalysisComparer.Compare(diagnostics, expectedIssues, "C# 10");

            summary.ToString().Should().Be(expected);
        }

        private static Diagnostic[] GetDiagnostics(Compilation compilation) =>
            DiagnosticVerifier.GetAnalyzerDiagnostics(compilation, new DiagnosticAnalyzer[] { new AsyncVoidMethod(), new MethodsShouldNotHaveIdenticalImplementations() }, CompilationErrorBehavior.Ignore).ToArray();

        private static Compilation GetCompilation(IEnumerable<string> filePaths) =>
            SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp, false).AddDocuments(filePaths).GetCompilation();

        private static Dictionary<string, IList<IIssueLocation>> GetExpectedIssues(Compilation compilation) =>
            compilation.SyntaxTrees.ToDictionary(x => x.FilePath, x => IssueLocationCollector.GetExpectedIssueLocations(new DiagnosticVerifier.File(x).Content.Lines));
    }
}
