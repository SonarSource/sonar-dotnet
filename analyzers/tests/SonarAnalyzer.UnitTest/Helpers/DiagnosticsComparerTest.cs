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

using SonarAnalyzer.Common;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class DiagnosticsComparerTest
    {
        [TestMethod]
        public void CompareShouldListAllDifferences_CS()
        {
            const string expected =
@"Comparing actual issues with expected:
Language version: C# 10.

DiagnosticsComparer.Primary.cs
Line 6: Unexpected Primary issue 'Return 'Task' instead.'! ID: S3168
Line 8: Primary issue message 'Wrong message' does not match the actual message 'Return 'Task' instead.'! ID: S3168
Line 10: Primary issue message 'Wrong message, wrong place' does not match the actual message 'Return 'Task' instead.'! ID: S3168
Line 13: Primary issue should start on column 15 but got column 10! ID: S3168
Line 16: Primary issue should start on column 15 but got column 10! ID: S3168
Line 19: Primary issue should have a length of 5 but got a length of 4! ID: S3168
Line 1: Expected Primary issue was not raised!

DiagnosticsComparer.Secondary.cs
Line 5: Primary issue should have a length of 5 but got a length of 4! ID: S3168
Line 8: Secondary issue message 'Wrong message' does not match the actual message ''! ID: flow-1
Line 15: Primary issue message 'Wrong message' does not match the actual message 'Update this method so that its implementation is not identical to 'Default'.'! ID: flow-0
Line 12: Expected Secondary issue was not raised! ID: flow-2
";
            var compilation = CreateCompilation(AnalyzerLanguage.CSharp, "TestCases/DiagnosticsComparer.Primary.cs", "TestCases/DiagnosticsComparer.Secondary.cs");
            var diagnostics = GetDiagnostics(compilation, new CS.AsyncVoidMethod(), new CS.MethodsShouldNotHaveIdenticalImplementations());
            var expectedIssues = GetExpectedIssues(compilation);

            var summary = DiagnosticsComparer.Compare(diagnostics, expectedIssues, "C# 10");

            summary.ToString().Should().Be(expected);
        }

        [TestMethod]
        public void CompareShouldListAllDifferences_VB()
        {
            const string expected =
@"Comparing actual issues with expected:
Language version: VB.Net 12.

DiagnosticsComparer.Primary.vb
Line 3: Primary issue should start on column 22 but got column 18! ID: S2355
Line 8: Unexpected Primary issue 'Use an array literal here instead.'! ID: S2355
Line 12: Primary issue message 'Wrong message' does not match the actual message 'Use an array literal here instead.'! ID: S2355
Line 16: Primary issue should start on column 24 but got column 18! ID: S2355
Line 21: Primary issue should start on column 22 but got column 18! ID: S2355
";
            var compilation = CreateCompilation(AnalyzerLanguage.VisualBasic, "TestCases/DiagnosticsComparer.Primary.vb");
            var diagnostics = GetDiagnostics(compilation, new VB.ArrayCreationLongSyntax());
            var expectedIssues = GetExpectedIssues(compilation);

            var summary = DiagnosticsComparer.Compare(diagnostics, expectedIssues, "VB.Net 12");

            summary.ToString().Should().Be(expected);
        }

        private static Diagnostic[] GetDiagnostics(Compilation compilation, params DiagnosticAnalyzer[] diagnostics) =>
            DiagnosticVerifier.GetAnalyzerDiagnostics(compilation, diagnostics, CompilationErrorBehavior.Ignore).ToArray();

        private static Compilation CreateCompilation(AnalyzerLanguage language, params string[] filePaths) =>
            SolutionBuilder.Create().AddProject(language, false).AddDocuments(filePaths).GetCompilation();

        private static Dictionary<string, IList<IIssueLocation>> GetExpectedIssues(Compilation compilation) =>
            compilation.SyntaxTrees.ToDictionary(x => x.FilePath, x => IssueLocationCollector.GetExpectedIssueLocations(new DiagnosticVerifier.File(x).Content.Lines));
    }
}
