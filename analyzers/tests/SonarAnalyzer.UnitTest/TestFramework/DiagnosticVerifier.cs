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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal static class DiagnosticVerifier
    {
        private const string AnalyzerFailedDiagnosticId = "AD0001";

        private static readonly string[] BuildErrorsToIgnore = new string[]
        {
            "BC36716" // VB12 does not support line continuation comments" i.e. a comment at the end of a multi-line statement.
        };

        public static void VerifyExternalFile(Compilation compilation, DiagnosticAnalyzer diagnosticAnalyzer, string fileContent, string sonarProjectConfigPath) =>
            Verify(compilation, new[] { diagnosticAnalyzer }, CompilationErrorBehavior.FailTest, SourceText.From(fileContent), sonarProjectConfigPath);

        public static void Verify(Compilation compilation, DiagnosticAnalyzer diagnosticAnalyzer, CompilationErrorBehavior checkMode) =>
            Verify(compilation, new[] { diagnosticAnalyzer }, checkMode);

        public static void Verify(Compilation compilation, DiagnosticAnalyzer[] diagnosticAnalyzers, CompilationErrorBehavior checkMode, string sonarProjectConfigPath = null) =>
            Verify(compilation, diagnosticAnalyzers, checkMode, compilation.SyntaxTrees.Skip(1).First().GetText(), sonarProjectConfigPath);

        public static void Verify(Compilation compilation, DiagnosticAnalyzer[] diagnosticAnalyzers, CompilationErrorBehavior checkMode, SourceText source, string sonarProjectConfigPath = null)
        {
            SuppressionHandler.HookSuppression();
            try
            {
                var diagnostics = GetDiagnostics(compilation, diagnosticAnalyzers, checkMode, sonarProjectConfigPath: sonarProjectConfigPath);
                var expectedIssues = new IssueLocationCollector().GetExpectedIssueLocations(source.Lines).ToList();
                CompareActualToExpected(compilation.LanguageVersionString(), diagnostics, expectedIssues, false);

                // When there are no diagnostics reported from the test (for example the FileLines analyzer
                // does not report in each call to Verifier.VerifyAnalyzer) we skip the check for the extension
                // method.
                if (diagnostics.Any())
                {
                    SuppressionHandler.ExtensionMethodsCalledForAllDiagnostics(diagnosticAnalyzers).Should().BeTrue("The ReportDiagnosticWhenActive should be used instead of ReportDiagnostic");
                }
            }
            finally
            {
                SuppressionHandler.UnHookSuppression();
            }
        }

        internal static void CompareActualToExpected(string languageVersion, IEnumerable<Diagnostic> diagnostics, ICollection<IIssueLocation> expectedIssues, bool compareIdToMessage)
        {
            DumpActualDiagnostics(languageVersion, diagnostics);

            foreach (var diagnostic in diagnostics)
            {
                var issueId = VerifyPrimaryIssue(languageVersion,
                    expectedIssues,
                    issue => issue.IsPrimary,
                    diagnostic.Location,
                    compareIdToMessage ? diagnostic.Id : diagnostic.GetMessage(),
                    compareIdToMessage
                        ? $"{languageVersion}: Unexpected build error [{diagnostic.Id}]: {diagnostic.GetMessage()} on line {diagnostic.Location.GetLineNumberToReport()}"
                        : null);

                var secondaryLocations = diagnostic.AdditionalLocations
                    .Select((location, i) => diagnostic.GetSecondaryLocation(i))
                    .OrderBy(x => x.Location.GetLineNumberToReport())
                    .ThenBy(x => x.Location.GetLineSpan().StartLinePosition.Character);

                foreach (var secondaryLocation in secondaryLocations)
                {
                    VerifySecondaryIssue(languageVersion,
                        expectedIssues,
                        issue => issue.IssueId == issueId && !issue.IsPrimary,
                        secondaryLocation.Location,
                        secondaryLocation.Message,
                        issueId);
                }
            }

            if (expectedIssues.Count != 0)
            {
                var expectedIssuesDescription = expectedIssues.Select(i => $"{Environment.NewLine}Line: {i.LineNumber}, Type: {IssueType(i.IsPrimary)}, Id: '{i.IssueId}'");
                Execute.Assertion.FailWith($"{languageVersion}: Issue(s) expected but not raised on line(s):{expectedIssuesDescription.JoinStr("")}");
            }
        }

        private static void DumpActualDiagnostics(string languageVersion, IEnumerable<Diagnostic> diagnostics)
        {
            Console.WriteLine($"{languageVersion}: Actual diagnostics: {diagnostics.Count()}");
            foreach (var d in diagnostics.OrderBy(x => x.GetLineNumberToReport()))
            {
                var lineSpan = d.Location.GetLineSpan();
                Console.WriteLine($"  Id: {d.Id}, Line: {d.Location.GetLineNumberToReport()}, [{lineSpan.StartLinePosition.Character}, {lineSpan.EndLinePosition.Character}]");
            }
        }

        public static IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation,
            DiagnosticAnalyzer diagnosticAnalyzer, CompilationErrorBehavior checkMode,
            bool verifyNoExceptionIsThrown = true,
            string sonarProjectConfigPath = null) =>
            GetDiagnostics(compilation, new[] { diagnosticAnalyzer }, checkMode, verifyNoExceptionIsThrown, sonarProjectConfigPath);

        public static void VerifyNoIssueReported(Compilation compilation, DiagnosticAnalyzer diagnosticAnalyzer, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default) =>
            GetDiagnostics(compilation, diagnosticAnalyzer, checkMode).Should().BeEmpty();

        public static ImmutableArray<Diagnostic> GetAllDiagnostics(Compilation compilation,
            IEnumerable<DiagnosticAnalyzer> diagnosticAnalyzers, CompilationErrorBehavior checkMode,
            bool verifyNoException = true,
            CancellationToken? cancellationToken = null,
            string sonarProjectConfigPath = null)
        {
            var supportedDiagnostics = diagnosticAnalyzers
                    .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                    .Select(diagnostic => new KeyValuePair<string, ReportDiagnostic>(diagnostic.Id, ReportDiagnostic.Warn))
                    .Concat(new[] { new KeyValuePair<string, ReportDiagnostic>(AnalyzerFailedDiagnosticId, ReportDiagnostic.Error) });

            var compilationOptions = compilation.Options.WithSpecificDiagnosticOptions(supportedDiagnostics);
            var actualToken = cancellationToken ?? CancellationToken.None;
            var analyzerOptions = string.IsNullOrWhiteSpace(sonarProjectConfigPath) ? null : TestHelper.CreateOptions(sonarProjectConfigPath);

            var diagnostics = compilation
                .WithOptions(compilationOptions)
                .WithAnalyzers(diagnosticAnalyzers.ToImmutableArray(), analyzerOptions)
                .GetAllDiagnosticsAsync(actualToken)
                .Result;

            if (!actualToken.IsCancellationRequested)
            {
                if (verifyNoException)
                {
                    VerifyNoExceptionThrown(diagnostics);
                }
                if (checkMode == CompilationErrorBehavior.FailTest)
                {
                    VerifyBuildErrors(diagnostics, compilation);
                }
            }

            return diagnostics;
        }

        internal static IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation,
            DiagnosticAnalyzer[] diagnosticAnalyzers, CompilationErrorBehavior checkMode,
            bool verifyNoExceptionIsThrown = true,
            string sonarProjectConfigPath = null)
        {
            var ids = diagnosticAnalyzers
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .Select(diagnostic => diagnostic.Id)
                .Distinct()
                .ToHashSet();

            return GetAllDiagnostics(compilation, diagnosticAnalyzers, checkMode, verifyNoExceptionIsThrown, sonarProjectConfigPath: sonarProjectConfigPath)
                .Where(d => ids.Contains(d.Id));
        }

        private static void VerifyBuildErrors(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
        {
            var buildErrors = GetBuildErrors(diagnostics);

            var expectedBuildErrors = new IssueLocationCollector()
                .GetExpectedBuildErrors(compilation.SyntaxTrees.Skip(1).FirstOrDefault()?.GetText().Lines)
                .ToList();
            CompareActualToExpected(compilation.LanguageVersionString(), buildErrors, expectedBuildErrors, true);
        }

        private static IEnumerable<Diagnostic> GetBuildErrors(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error
                && (d.Id.StartsWith("CS") || d.Id.StartsWith("BC"))
                && !BuildErrorsToIgnore.Contains(d.Id));

        private static void VerifyNoExceptionThrown(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Should().NotContain(d => d.Id == AnalyzerFailedDiagnosticId);

        private static string VerifyPrimaryIssue(string languageVersion, ICollection<IIssueLocation> expectedIssues, Func<IIssueLocation, bool> issueFilter,
            Location location, string message, string extraInfo) =>
            VerifyIssue(languageVersion, expectedIssues, issueFilter, location, message, extraInfo, true, null);

        private static void VerifySecondaryIssue(string languageVersion, ICollection<IIssueLocation> expectedIssues, Func<IIssueLocation, bool> issueFilter,
            Location location, string message, string issueId) =>
            VerifyIssue(languageVersion, expectedIssues, issueFilter, location, message, null, false, issueId);

        private static string VerifyIssue(string languageVersion, ICollection<IIssueLocation> expectedIssues, Func<IIssueLocation, bool> issueFilter,
            Location location, string message, string extraInfo, bool isPrimary, string primaryIssueId)
        {
            var lineNumber = location.GetLineNumberToReport();
            var expectedIssue = expectedIssues
                .Where(issueFilter)
                .FirstOrDefault(issue => issue.LineNumber == lineNumber);
            var issueType = IssueType(isPrimary);

            if (expectedIssue == null)
            {
                var issueId = primaryIssueId == null ? "" : $" [{ primaryIssueId}]";
                var seeOutputMessage = $"{Environment.NewLine}See output to see all actual diagnostics raised on the file";
                var lineSpan = location.GetLineSpan().Span.ToString();
                var exceptionMessage = string.IsNullOrEmpty(extraInfo)
                    ? $"{languageVersion}: Unexpected {issueType} issue{issueId} on line {lineNumber}, span {lineSpan} with message '{message}'.{seeOutputMessage}"
                    : extraInfo;
                throw new UnexpectedDiagnosticException(location, exceptionMessage);
            }

            if (expectedIssue.Message != null && expectedIssue.Message != message)
            {
                throw new UnexpectedDiagnosticException(location,
$@"{languageVersion}: Expected {issueType} message on line {lineNumber} does not match actual message.
Expected: '{expectedIssue.Message}'
Actual  : '{message}'");
            }

            var diagnosticStart = location.GetLineSpan().StartLinePosition.Character;

            if (expectedIssue.Start.HasValue && expectedIssue.Start != diagnosticStart)
            {
                throw new UnexpectedDiagnosticException(location,
                    $"{languageVersion}: Expected {issueType} issue on line {lineNumber} to start on column {expectedIssue.Start} but got column {diagnosticStart}.");
            }

            if (expectedIssue.Length.HasValue && expectedIssue.Length != location.SourceSpan.Length)
            {
                throw new UnexpectedDiagnosticException(location,
                    $"{languageVersion}: Expected {issueType} issue on line {lineNumber} to have a length of {expectedIssue.Length} but got a length of {location.SourceSpan.Length}.");
            }

            expectedIssues.Remove(expectedIssue);
            return expectedIssue.IssueId;
        }

        private static string IssueType(bool isPrimary) => isPrimary ? "primary" : "secondary";

        internal static class SuppressionHandler
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

                SonarAnalysisContext.ShouldDiagnosticBeReported = (s, d) =>
                {
                    IncrementReportCount(d.Id);
                    return true;
                };
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

            public static void IncrementReportCount(string ruleId) =>
                counters.AddOrUpdate(ruleId, addValueFactory: key => 1, updateValueFactory: (key, count) => count + 1);

            public static bool ExtensionMethodsCalledForAllDiagnostics(IEnumerable<DiagnosticAnalyzer> analyzers) =>
                // In general this check is not very precise, because when the tests are run in parallel
                // we cannot determine which diagnostic was reported from which analyzer instance. In other
                // words, we cannot distinguish between diagnostics reported from different tests. That's
                // why we require each diagnostic to be reported through the extension methods at least once.
                analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics)
                    .Select(d => DictionaryExtensions.GetValueOrDefault(counters, d.Id))
                    .Any(count => count > 0);
        }
    }
}
