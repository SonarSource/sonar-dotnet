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

using System.Collections.Concurrent;
using System.Linq;
using System.Text;
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

        private static readonly string[] BuildErrorsToIgnore =
        {
            "BC36716" // VB12 does not support line continuation comments" i.e. a comment at the end of a multi-line statement.
        };

        public static void VerifyExternalFile(Compilation compilation, DiagnosticAnalyzer diagnosticAnalyzer, string fileName, string sonarProjectConfigPath) =>
            Verify(compilation, new[] { diagnosticAnalyzer }, CompilationErrorBehavior.FailTest, new[] { new File(fileName) }, sonarProjectConfigPath);

        public static void Verify(
                Compilation compilation,
                DiagnosticAnalyzer diagnosticAnalyzer,
                CompilationErrorBehavior checkMode,
                string sonarProjectConfigPath = null,
                string[] onlyDiagnostics = null) =>
            Verify(compilation, new[] { diagnosticAnalyzer }, checkMode, sonarProjectConfigPath, onlyDiagnostics);

        public static void Verify(
                Compilation compilation,
                DiagnosticAnalyzer[] diagnosticAnalyzers,
                CompilationErrorBehavior checkMode,
                string sonarProjectConfigPath = null,
                string[] onlyDiagnostics = null) =>
            Verify(compilation, diagnosticAnalyzers, checkMode, compilation.SyntaxTrees.Skip(1).Select(x => new File(x)), sonarProjectConfigPath, onlyDiagnostics);

        public static void Verify(Compilation compilation,
                                  DiagnosticAnalyzer diagnosticAnalyzer,
                                  CompilationErrorBehavior checkMode,
                                  SyntaxTree syntaxTree) =>
            Verify(compilation, new[] { diagnosticAnalyzer }, checkMode, new[] { new File(syntaxTree), });

        private static void Verify(Compilation compilation,
                                   DiagnosticAnalyzer[] diagnosticAnalyzers,
                                   CompilationErrorBehavior checkMode,
                                   IEnumerable<File> sources,
                                   string sonarProjectConfigPath = null,
                                   string[] onlyDiagnostics = null)
        {
            SuppressionHandler.HookSuppression();
            try
            {
                var diagnostics = GetAnalyzerDiagnostics(compilation, diagnosticAnalyzers, checkMode, sonarProjectConfigPath, onlyDiagnostics).ToArray();
                var expectedIssues = sources.Select(x => x.ToExpectedIssueLocations()).ToArray();
                VerifyNoExceptionThrown(diagnostics);
                CompareActualToExpected(compilation.LanguageVersionString(), diagnostics, expectedIssues, false);

                // When there are no diagnostics reported from the test (for example the FileLines analyzer
                // does not report in each call to Verifier.VerifyAnalyzer) we skip the check for the extension
                // method.
                if (diagnostics.Any())
                {
                    SuppressionHandler.ExtensionMethodsCalledForAllDiagnostics(diagnosticAnalyzers).Should().BeTrue("The ReportIssue should be used instead of ReportDiagnostic");
                }
            }
            finally
            {
                SuppressionHandler.UnHookSuppression();
            }
        }

        public static void VerifyNoIssueReported(Compilation compilation,
                                                 DiagnosticAnalyzer diagnosticAnalyzer,
                                                 CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                 string sonarProjectConfigPath = null,
                                                 string[] onlyDiagnostics = null) =>
            GetDiagnosticsNoExceptions(compilation, diagnosticAnalyzer, checkMode, sonarProjectConfigPath, onlyDiagnostics).Should().BeEmpty();

        public static IEnumerable<Diagnostic> GetDiagnosticsNoExceptions(Compilation compilation,
                                                                         DiagnosticAnalyzer diagnosticAnalyzer,
                                                                         CompilationErrorBehavior checkMode,
                                                                         string sonarProjectConfigPath = null,
                                                                         string[] onlyDiagnostics = null)
        {
            var ret = GetAnalyzerDiagnostics(compilation, new[] { diagnosticAnalyzer }, checkMode, sonarProjectConfigPath, onlyDiagnostics);
            VerifyNoExceptionThrown(ret);
            return ret;
        }

        public static IEnumerable<Diagnostic> GetDiagnosticsIgnoreExceptions(Compilation compilation, DiagnosticAnalyzer diagnosticAnalyzer) =>
            GetAnalyzerDiagnostics(compilation, new[] { diagnosticAnalyzer }, CompilationErrorBehavior.FailTest);

        public static ImmutableArray<Diagnostic> GetAnalyzerDiagnostics(Compilation compilation,
                                                                        DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                                        CompilationErrorBehavior checkMode,
                                                                        string sonarProjectConfigPath = null,
                                                                        string[] onlyDiagnostics = null)
        {
            onlyDiagnostics ??= Array.Empty<string>();
            var supportedDiagnostics = diagnosticAnalyzers
                .SelectMany(x => x.SupportedDiagnostics.Select(d => d.Id))
                .Concat(new[] { AnalyzerFailedDiagnosticId })
                .Select(x => new KeyValuePair<string, ReportDiagnostic>(x, Severity(x)))
                .ToArray();

            var ids = supportedDiagnostics.Select(x => x.Key).ToHashSet();

            var compilationOptions = compilation.Options.WithSpecificDiagnosticOptions(supportedDiagnostics);
            var analyzerOptions = string.IsNullOrWhiteSpace(sonarProjectConfigPath) ? null : TestHelper.CreateOptions(sonarProjectConfigPath);
            var diagnostics = compilation
                .WithOptions(compilationOptions)
                .WithAnalyzers(diagnosticAnalyzers.ToImmutableArray(), analyzerOptions)
                .GetAllDiagnosticsAsync(default)
                .Result;

            if (checkMode == CompilationErrorBehavior.FailTest)
            {
                VerifyBuildErrors(diagnostics, compilation);
            }
            return diagnostics.Where(x => ids.Contains(x.Id)).ToImmutableArray();

            ReportDiagnostic Severity(string id)
            {
                if (id == AnalyzerFailedDiagnosticId)
                {
                    return ReportDiagnostic.Error;
                }
                else
                {
                    return !onlyDiagnostics.Any() || onlyDiagnostics.Contains(id) ? ReportDiagnostic.Warn : ReportDiagnostic.Suppress;
                }
            }
        }

        public static void CompareActualToExpected(string languageVersion, Diagnostic[] diagnostics, FileIssueLocations[] expectedIssuesPerFile, bool compareIdToMessage)
        {
            DumpActualDiagnostics(languageVersion, diagnostics);

            foreach (var diagnostic in diagnostics.OrderBy(x => x.Location.SourceSpan.Start))
            {
                var expectedIssues = ExpectedIssues(expectedIssuesPerFile, diagnostic.Location);
                var issueId = VerifyPrimaryIssue(languageVersion,
                    expectedIssues,
                    issue => issue.IsPrimary,
                    diagnostic.Location,
                    compareIdToMessage ? diagnostic.Id : diagnostic.GetMessage(),
                    compareIdToMessage
                        ? $"{languageVersion}: Unexpected build error [{diagnostic.Id}]: {diagnostic.GetMessage()} on line {diagnostic.Location.GetLineNumberToReport()}"
                        : null);

                var secondaryLocations = diagnostic.AdditionalLocations
                    .Select((_, i) => diagnostic.GetSecondaryLocation(i))
                    .OrderBy(x => x.Location.GetLineNumberToReport())
                    .ThenBy(x => x.Location.GetLineSpan().StartLinePosition.Character);

                foreach (var secondaryLocation in secondaryLocations)
                {
                    var expectedIssuesSecondaryLocation = ExpectedIssues(expectedIssuesPerFile, secondaryLocation.Location);
                    VerifySecondaryIssue(languageVersion,
                        expectedIssuesSecondaryLocation,
                        issue => issue.IssueId == issueId && !issue.IsPrimary,
                        secondaryLocation.Location,
                        secondaryLocation.Message,
                        issueId);
                }
            }

            if (expectedIssuesPerFile.Any(x => x.IssueLocations.Any()))
            {
                var issuesString = new StringBuilder();
                foreach (var fileWithIssues in expectedIssuesPerFile.Where(x => x.IssueLocations.Any()).OrderBy(x => x.IssueLocations.First().Start))
                {
                    issuesString.Append($"{Environment.NewLine}File: {fileWithIssues.FileName}");
                    var expectedIssuesDescription = fileWithIssues.IssueLocations.Select(i => $"{Environment.NewLine}Line: {i.LineNumber}, Type: {IssueType(i.IsPrimary)}, Id: '{i.IssueId}'");
                    issuesString.AppendLine(expectedIssuesDescription.JoinStr(string.Empty));
                }
                Execute.Assertion.FailWith($"{languageVersion}: Issue(s) expected but not raised in file(s):{issuesString}");
            }
        }

        private static IList<IIssueLocation> ExpectedIssues(FileIssueLocations[] expectedIssuesPerFile, Location location) =>
            location.SourceTree == null
                ? expectedIssuesPerFile.SingleOrDefault(x => x.IssueLocations.Any())?.IssueLocations ?? new List<IIssueLocation>() // Issue locations get removed, so the list could become empty
                : expectedIssuesPerFile.Single(x => x.FileName == location.SourceTree.FilePath).IssueLocations;

        private static void DumpActualDiagnostics(string languageVersion, Diagnostic[] diagnostics)
        {
            Console.WriteLine($"{languageVersion}: Actual diagnostics: {diagnostics.Length}");
            foreach (var d in diagnostics.OrderBy(x => x.GetLineNumberToReport()))
            {
                var lineSpan = d.Location.GetLineSpan();
                Console.WriteLine($"  Id: {d.Id}, Line: {d.Location.GetLineNumberToReport()}, [{lineSpan.StartLinePosition.Character}, {lineSpan.EndLinePosition.Character}]");
            }
        }

        private static void VerifyBuildErrors(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
        {
            var buildErrors = GetBuildErrors(diagnostics).ToArray();

            var expectedBuildErrors = compilation.SyntaxTrees
                                                 .Skip(1)
                                                 .Select(x => new FileIssueLocations(x.FilePath, IssueLocationCollector.GetExpectedBuildErrors(x.GetText().Lines).ToList()))
                                                 .ToArray();

            CompareActualToExpected(compilation.LanguageVersionString(), buildErrors, expectedBuildErrors, true);
        }

        private static IEnumerable<Diagnostic> GetBuildErrors(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error
                && (d.Id.StartsWith("CS") || d.Id.StartsWith("BC"))
                && !BuildErrorsToIgnore.Contains(d.Id));

        public static void VerifyNoExceptionThrown(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Should().NotContain(d => d.Id == AnalyzerFailedDiagnosticId);

        private static string VerifyPrimaryIssue(string languageVersion, IList<IIssueLocation> expectedIssues, Func<IIssueLocation, bool> issueFilter,
            Location location, string message, string extraInfo) =>
            VerifyIssue(languageVersion, expectedIssues, issueFilter, location, message, extraInfo, true, null);

        private static void VerifySecondaryIssue(string languageVersion, IList<IIssueLocation> expectedIssues, Func<IIssueLocation, bool> issueFilter,
            Location location, string message, string issueId) =>
            VerifyIssue(languageVersion, expectedIssues, issueFilter, location, message, null, false, issueId);

        private static string VerifyIssue(string languageVersion,
                                          ICollection<IIssueLocation> expectedIssues,
                                          Func<IIssueLocation, bool> issueFilter,
                                          Location location,
                                          string message,
                                          string extraInfo,
                                          bool isPrimary,
                                          string primaryIssueId)
        {
            var lineNumber = location.GetLineNumberToReport();
            var expectedIssue = expectedIssues
                .Where(issueFilter)
                .OrderBy(x => x.Start == null ? 0 : Math.Abs(location.GetLineSpan().StartLinePosition.Character - x.Start.Value))
                .ThenBy(x => x.Message == message ? 0 : 1)  // Prefer issue with explicit matching message when possible. There can be more with different messages
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

        internal class File
        {
            private readonly string fileName;

            public SourceText Content { get; }

            public File(string fileName)
            {
                this.fileName = fileName;
                Content = SourceText.From(System.IO.File.ReadAllText(fileName));
            }

            public File(SyntaxTree syntaxTree)
            {
                fileName = syntaxTree.FilePath;
                Content = syntaxTree.GetText();
            }

            public FileIssueLocations ToExpectedIssueLocations() =>
                new(fileName, IssueLocationCollector.GetExpectedIssueLocations(Content.Lines));
        }

        internal class FileIssueLocations
        {
            public string FileName { get; }
            public IList<IIssueLocation> IssueLocations { get; }

            public FileIssueLocations(string fileName, IList<IIssueLocation> issueLocations)
            {
                FileName = fileName;
                IssueLocations = issueLocations;
            }
        }

        internal static class SuppressionHandler
        {
            private static readonly ConcurrentDictionary<string, int> Counters = new();
            private static bool isHooked;

            public static void HookSuppression()
            {
                if (isHooked)
                {
                    return;
                }
                isHooked = true;

                SonarAnalysisContext.ShouldDiagnosticBeReported = (_, d) =>
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
                Counters.AddOrUpdate(ruleId, _ => 1, (_, count) => count + 1);

            public static bool ExtensionMethodsCalledForAllDiagnostics(IEnumerable<DiagnosticAnalyzer> analyzers) =>
                // In general this check is not very precise, because when the tests are run in parallel
                // we cannot determine which diagnostic was reported from which analyzer instance. In other
                // words, we cannot distinguish between diagnostics reported from different tests. That's
                // why we require each diagnostic to be reported through the extension methods at least once.
                analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics)
                         .Select(d => DictionaryExtensions.GetValueOrDefault(Counters, d.Id))
                         .Any(count => count > 0);
        }
    }
}
