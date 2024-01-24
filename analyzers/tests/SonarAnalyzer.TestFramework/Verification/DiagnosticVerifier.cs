/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.Text;
using FluentAssertions.Execution;
using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.Test.TestFramework
{
    public static class DiagnosticVerifier
    {
        private const string AnalyzerFailedDiagnosticId = "AD0001";

        private static readonly string[] BuildErrorsToIgnore =
        {
            "BC36716" // VB12 does not support line continuation comments" i.e. a comment at the end of a multi-line statement.
        };

        public static void VerifyExternalFile(Compilation compilation, DiagnosticAnalyzer analyzer, string externalFilePath, string additionalFilePath) =>
            Verify(compilation, new[] { analyzer }, CompilationErrorBehavior.FailTest, additionalFilePath, null, new[] { externalFilePath });

        public static void Verify(
                Compilation compilation,
                DiagnosticAnalyzer analyzer,
                CompilationErrorBehavior checkMode,
                string additionalFilePath = null,
                string[] onlyDiagnostics = null,
                string[] additionalSourceFiles = null) =>
            Verify(compilation, new[] { analyzer }, checkMode, additionalFilePath, onlyDiagnostics, additionalSourceFiles);

        public static void Verify(
                Compilation compilation,
                DiagnosticAnalyzer[] analyzers,
                CompilationErrorBehavior checkMode,
                string additionalFilePath = null,
                string[] onlyDiagnostics = null,
                string[] additionalSourceFiles = null)
        {
            SuppressionHandler.HookSuppression();
            try
            {
                var sources = compilation.SyntaxTrees.ExceptExtraEmptyFile().ExceptRazorGeneratedFiles()
                    .Select(x => new FileContent(x))
                    .Concat((additionalSourceFiles ?? Array.Empty<string>()).Select(x => new FileContent(x)));
                var diagnostics = GetAnalyzerDiagnostics(compilation, analyzers, checkMode, additionalFilePath, onlyDiagnostics).ToArray();
                var expected = new CompilationIssues(compilation.LanguageVersionString(), sources);
                VerifyNoExceptionThrown(diagnostics);
                CompareActualToExpected(diagnostics, expected, false);

                // When there are no diagnostics reported from the test (for example the FileLines analyzer
                // does not report in each call to Verifier.VerifyAnalyzer) we skip the check for the extension
                // method.
                if (diagnostics.Any())
                {
                    SuppressionHandler.ExtensionMethodsCalledForAllDiagnostics(analyzers).Should().BeTrue("The ReportIssue should be used instead of ReportDiagnostic");
                }
            }
            finally
            {
                SuppressionHandler.UnHookSuppression();
            }
        }

        public static void VerifyNoIssueReported(Compilation compilation,
                                                 DiagnosticAnalyzer analyzer,
                                                 CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                 string additionalFilePath = null,
                                                 string[] onlyDiagnostics = null) =>
            GetDiagnosticsNoExceptions(compilation, analyzer, checkMode, additionalFilePath, onlyDiagnostics).Should().BeEmpty();

        public static IEnumerable<Diagnostic> GetDiagnosticsNoExceptions(Compilation compilation,
                                                                         DiagnosticAnalyzer analyzer,
                                                                         CompilationErrorBehavior checkMode,
                                                                         string additionalFilePath = null,
                                                                         string[] onlyDiagnostics = null)
        {
            var ret = GetAnalyzerDiagnostics(compilation, new[] { analyzer }, checkMode, additionalFilePath, onlyDiagnostics);
            VerifyNoExceptionThrown(ret);
            return ret;
        }

        public static IEnumerable<Diagnostic> GetDiagnosticsIgnoreExceptions(Compilation compilation, DiagnosticAnalyzer analyzer) =>
            GetAnalyzerDiagnostics(compilation, new[] { analyzer }, CompilationErrorBehavior.FailTest);

        public static ImmutableArray<Diagnostic> GetAnalyzerDiagnostics(Compilation compilation,
                                                                        DiagnosticAnalyzer[] analyzer,
                                                                        CompilationErrorBehavior checkMode,
                                                                        string additionalFilePath = null,
                                                                        string[] onlyDiagnostics = null)
        {
            onlyDiagnostics ??= Array.Empty<string>();
            var supportedDiagnostics = analyzer
                .SelectMany(x => x.SupportedDiagnostics.Select(d => d.Id))
                .Concat(new[] { AnalyzerFailedDiagnosticId })
                .Select(x => new KeyValuePair<string, ReportDiagnostic>(x, Severity(x)))
                .ToArray();

            var ids = supportedDiagnostics.Select(x => x.Key).ToHashSet();

            var compilationOptions = compilation.Options.WithSpecificDiagnosticOptions(supportedDiagnostics);
            var analyzerOptions = string.IsNullOrWhiteSpace(additionalFilePath) ? null : AnalysisScaffolding.CreateOptions(additionalFilePath);
            var diagnostics = compilation
                .WithOptions(compilationOptions)
                .WithAnalyzers(analyzer.ToImmutableArray(), analyzerOptions)
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

        private static void CompareActualToExpected(Diagnostic[] diagnostics, CompilationIssues expected, bool compareIdToMessage)
        {
            var actual = new CompilationIssues(expected.LanguageVersion, diagnostics);
            var pairs = MatchPairs(actual, expected);
            var assertionMessages = new StringBuilder();
            string previousFilePath = null;
            foreach (var pair in pairs.OrderBy(x => x.FilePath).ThenBy(x => x.LineNumber).ThenBy(x => x.Start))
            {
                if (previousFilePath != pair.FilePath)
                {
                    if (assertionMessages.Length > 0)
                    {
                        assertionMessages.AppendLine();
                    }
                    assertionMessages.AppendLine($"There are differences for {actual.LanguageVersion} {pair.FilePath}:");
                    previousFilePath = pair.FilePath;
                }
                pair.AppendAssertionMessage(assertionMessages);
            }
            if (assertionMessages.Length == 0)
            {
                actual.Dump();
            }
            else
            {
                Execute.Assertion.FailWith(assertionMessages.ToString().Replace("{", "{{").Replace("}", "}}"));    // Replacing { and } to avoid invalid format for string.Format
            }
        }

        private static IEnumerable<SyntaxTree> ExceptExtraEmptyFile(this IEnumerable<SyntaxTree> syntaxTrees) =>
            syntaxTrees.Where(x =>
                !x.FilePath.EndsWith("ExtraEmptyFile.g.cs", StringComparison.OrdinalIgnoreCase)
                && !x.FilePath.EndsWith("ExtraEmptyFile.g.vbnet", StringComparison.OrdinalIgnoreCase));

        private static IEnumerable<SyntaxTree> ExceptRazorGeneratedFiles(this IEnumerable<SyntaxTree> syntaxTrees) =>
            syntaxTrees.Where(x =>
                !x.FilePath.EndsWith("razor.g.cs", StringComparison.OrdinalIgnoreCase)
                && !x.FilePath.EndsWith("cshtml.g.cs", StringComparison.OrdinalIgnoreCase));

        private static void VerifyBuildErrors(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
        {
            var buildErrors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error && (x.Id.StartsWith("CS") || x.Id.StartsWith("BC")) && !BuildErrorsToIgnore.Contains(x.Id)).ToArray();
            var expected = new CompilationIssues(compilation.LanguageVersionString(), compilation.SyntaxTrees
                .ExceptExtraEmptyFile() // FIXME: Is this really needed?
                .SelectMany(x => IssueLocationCollector.GetExpectedBuildErrors(x.FilePath, x.GetText().Lines).ToList()));
            CompareActualToExpected(buildErrors, expected, true);
        }

        public static void VerifyNoExceptionThrown(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Should().NotContain(d => d.Id == AnalyzerFailedDiagnosticId);

        private static IEnumerable<IssueLocationPair> MatchPairs(CompilationIssues actualIssues, CompilationIssues expectedIssues)
        {
            var ret = new List<IssueLocationPair>();
            foreach (var key in actualIssues.UniqueKeys())
            {
                ret.AddRange(BestMatch(actualIssues.Remove(key), expectedIssues.Remove(key)));
            }
            ret.AddRange(expectedIssues.Select(x => new IssueLocationPair(null, x)));
            return ret;
        }

        private static IEnumerable<IssueLocationPair> BestMatch(List<IssueLocation> actualIssues, List<IssueLocation> expectedIssues)
        {
            foreach (var actual in actualIssues.ToArray())  // First round removes all perfect matches, so we don't mismatch possible perfect match by imperfect one on the same line.
            {
                if (expectedIssues.Remove(actual))
                {
                    actualIssues.Remove(actual);
                }
            }
            foreach (var actual in actualIssues)
            {
                var expected = expectedIssues.OrderBy(x => Math.Abs(actual.Start ?? 0 - x.Start ?? 0)).ThenBy(x => Math.Abs(actual.Length ?? 0 - x.Length ?? 0)).FirstOrDefault();
                expectedIssues.Remove(expected);
                yield return new(actual, expected); // expected can be null
            }
            foreach (var expected in expectedIssues)
            {
                yield return new(null, expected);
            }
        }
    }
}
