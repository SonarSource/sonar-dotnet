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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public static class NewVerifier
    {
        private const string AnalyzerFailedDiagnosticId = "AD0001";

        public static void Verify(string path, DiagnosticAnalyzer analyzer, params MetadataReference[] additionalReferences)
        {
            var solution = SolutionBuilder
                .Create()
                .AddProject("TestProject", Path.GetExtension(path))
                .AddDocument(path)
                .AddReferences(additionalReferences)
                .GetSolution();

            foreach (var compilation in solution.Compile())
            {
                Verify(compilation, analyzer);
            }
        }

        public static void VerifySnippet(string snippet, AnalyzerLanguage language, DiagnosticAnalyzer analyzer,
            params MetadataReference[] additionalReferences)
        {
            var solution = SolutionBuilder
               .Create()
               .AddProject("TestProject", language)
               .AddSnippet(snippet)
               .AddReferences(additionalReferences)
               .GetSolution();

            // TODO: add [CallerLineNumber]int lineNumber = 0
            // then add ability to shift result reports with this line number
            foreach (var compilation in solution.Compile())
            {
                Verify(compilation, analyzer);
            }
        }

        public static void Verify(Compilation compilation, DiagnosticAnalyzer diagnosticAnalyzer, Action<DiagnosticAnalyzer, Compilation> additionalVerify = null)
        {
            try
            {
                SuppressionHandler.HookSuppression();

                var diagnostics = GetDiagnostics(compilation, diagnosticAnalyzer);

                var expectedIssues = new IssueLocationCollector()
                    .GetExpectedIssueLocations(compilation.SyntaxTrees.Skip(1).First().GetText().Lines)
                    .ToList();

                foreach (var diagnostic in diagnostics)
                {
                    VerifyIssue(expectedIssues, issue => issue.IsPrimary, diagnostic.Location, diagnostic.GetMessage(), out var issueId);

                    diagnostic.AdditionalLocations
                        .Select((location, i) => diagnostic.GetSecondaryLocation(i))
                        .OrderBy(x => x.Location.GetLineNumberToReport())
                        .ThenBy(x => x.Location.GetLineSpan().StartLinePosition.Character)
                        .ToList()
                        .ForEach(secondaryLocation =>
                        {
                            VerifyIssue(expectedIssues, issue => issue.IssueId == issueId && !issue.IsPrimary,
                                secondaryLocation.Location, secondaryLocation.Message, out issueId);
                        });
                }

                if (expectedIssues.Count != 0)
                {
                    Execute.Assertion.FailWith($"Issue expected but not raised on line(s) {string.Join(",", expectedIssues.Select(i => i.LineNumber))}.");
                }

                // When there are no diagnostics reported from the test (for example the FileLines analyzer
                // does not report in each call to Verifier.VerifyAnalyzer) we skip the check for the extension
                // method.
                if (diagnostics.Any())
                {
                    SuppressionHandler.ExtensionMethodsCalledForAllDiagnostics(diagnosticAnalyzer).Should()
                        .BeTrue("The ReportDiagnosticWhenActive should be used instead of ReportDiagnostic");
                }

                additionalVerify?.Invoke(diagnosticAnalyzer, compilation);
            }
            finally
            {
                SuppressionHandler.UnHookSuppression();
            }
        }

        private static void VerifyIssue(IList<IIssueLocation> expectedIssues, Func<IIssueLocation, bool> issueFilter, Location location, string message, out string issueId)
        {
            var lineNumber = location.GetLineNumberToReport();

            var expectedIssue = expectedIssues
                .Where(issueFilter)
                .FirstOrDefault(issue => issue.LineNumber == lineNumber);

            if (expectedIssue == null)
            {
                Execute.Assertion.FailWith($"Issue with message '{message}' not expected on line {lineNumber}");
            }

            if (expectedIssue.Message != null && expectedIssue.Message != message)
            {
                Execute.Assertion.FailWith($"Expected message on line {lineNumber} to be '{expectedIssue.Message}', but got '{message}'.");
            }

            var diagnosticStart = location.GetLineSpan().StartLinePosition.Character;

            if (expectedIssue.Start.HasValue && expectedIssue.Start != diagnosticStart)
            {
                Execute.Assertion.FailWith(
                    $"Expected issue on line {lineNumber} to start on column {expectedIssue.Start} but got column {diagnosticStart}.");
            }

            if (expectedIssue.Length.HasValue && expectedIssue.Length != location.SourceSpan.Length)
            {
                Execute.Assertion.FailWith(
                    $"Expected issue on line {lineNumber} to have a length of {expectedIssue.Length} but got a length of {location.SourceSpan.Length}).");
            }

            expectedIssues.Remove(expectedIssue);

            issueId = expectedIssue.IssueId;
        }

        internal static IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            var ids = new HashSet<string>(diagnosticAnalyzer.SupportedDiagnostics.Select(diagnostic => diagnostic.Id));

            var diagnostics = GetAllDiagnostics(compilation, new[] { diagnosticAnalyzer }).ToList();
            VerifyNoExceptionThrown(diagnostics);

            return diagnostics.Where(d => ids.Contains(d.Id));
        }

        private static void VerifyNoExceptionThrown(IEnumerable<Diagnostic> diagnostics)
        {
            diagnostics.Where(d => d.Id == AnalyzerFailedDiagnosticId).Should().BeEmpty();
        }

        private static IEnumerable<Diagnostic> GetAllDiagnostics(Compilation compilation,
            IEnumerable<DiagnosticAnalyzer> diagnosticAnalyzers)
        {
            var compilationOptions = compilation.Language == LanguageNames.CSharp
                ? (CompilationOptions)new CS.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
                : new VB.VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var supportedDiagnostics = diagnosticAnalyzers
                    .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                    .ToList();
            compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                supportedDiagnostics
                    .Select(diagnostic =>
                        new KeyValuePair<string, ReportDiagnostic>(diagnostic.Id, ReportDiagnostic.Warn))
                    .Union(
                        new[]
                        {
                                new KeyValuePair<string, ReportDiagnostic>(AnalyzerFailedDiagnosticId, ReportDiagnostic.Error)
                        }));

            var compilationWithOptions = compilation.WithOptions(compilationOptions);
            var compilationWithAnalyzer = compilationWithOptions
                .WithAnalyzers(diagnosticAnalyzers.ToImmutableArray());

            return compilationWithAnalyzer.GetAllDiagnosticsAsync().Result;
        }

        private static class SuppressionHandler
        {
            private static bool isHooked = false;

            private static ConcurrentDictionary<string, int> counters = new ConcurrentDictionary<string, int>();

            public static void HookSuppression()
            {
                if (isHooked)
                {
                    return;
                }
                isHooked = true;

                SonarAnalysisContext.ShouldDiagnosticBeReported = (s, d) => { IncrementReportCount(d.Id); return true; };
            }

            public static void UnHookSuppression()
            {
                if (!isHooked)
                {
                    return;
                }
                isHooked = false;

                SonarAnalysisContext.ShouldDiagnosticBeReported = null;
            }

            internal static void IncrementReportCount(string ruleId)
            {
                counters.AddOrUpdate(ruleId, addValueFactory: key => 1, updateValueFactory: (key, count) => count + 1);
            }

            public static bool ExtensionMethodsCalledForAllDiagnostics(DiagnosticAnalyzer analyzer)
            {
                // In general this check is not very precise, because when the tests are run in parallel
                // we cannot determine which diagnostic was reported from which analyzer instance. In other
                // words, we cannot distinguish between diagnostics reported from different tests. That's
                // why we require each diagnostic to be reported through the extension methods at least once.
                return analyzer.SupportedDiagnostics
                    .Select(d => counters.GetValueOrDefault(d.Id))
                    .Any(count => count > 0);
            }
        }
    }
}
