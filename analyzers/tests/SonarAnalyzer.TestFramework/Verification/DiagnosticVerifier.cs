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

using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Verification;

public static class DiagnosticVerifier
{
    private const string AD0001 = nameof(AD0001);
    private const string LineContinuationVB12 = "BC36716";  // Visual Basic 12.0 does not support line continuation comments.

    public static void Verify(
            Compilation compilation,
            DiagnosticAnalyzer analyzer,
            string additionalFilePath = null,
            string[] onlyDiagnostics = null,
            string[] additionalSourceFiles = null) =>
        Verify(compilation, [analyzer], CompilationErrorBehavior.FailTest, additionalFilePath, onlyDiagnostics, additionalSourceFiles);

    public static void Verify(
            Compilation compilation,
            DiagnosticAnalyzer[] analyzers,
            CompilationErrorBehavior checkMode, // ToDo: Remove this parameter in https://github.com/SonarSource/sonar-dotnet/issues/8588
            string additionalFilePath = null,
            string[] onlyDiagnostics = null,
            string[] additionalSourceFiles = null)
    {
        SuppressionHandler.HookSuppression();
        try
        {
            var diagnostics = DiagnosticsAndErrors(compilation, analyzers, checkMode, additionalFilePath, onlyDiagnostics).ToArray();

            var externalFilePaths = diagnostics
                .Where(x => x.Location.Kind == LocationKind.ExternalFile)
                .Select(x => x.Location.GetLineSpan().Path)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Select(x => new FileContent(x))
                .ToList();

            var sources = compilation.SyntaxTrees.ExceptRazorGeneratedFiles()
                .Select(x => new FileContent(x))
                .Concat((additionalSourceFiles ?? Array.Empty<string>()).Select(x => new FileContent(x)))
                .Concat(externalFilePaths);

            var expected = new CompilationIssues(sources);
            VerifyNoExceptionThrown(diagnostics);
            Compare(compilation.LanguageVersionString(), new(diagnostics), expected);
            // When there are no issues reported from the test (the FileLines analyzer does not report in each call to Verifier.VerifyAnalyzer) we skip the check for the extension method.
            if (diagnostics.Any(x => x.Severity != DiagnosticSeverity.Error))
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
        AnalyzerDiagnostics(compilation, analyzer, checkMode, additionalFilePath, onlyDiagnostics).Should().NotContain(x => x.Id == AD0001 || x.Severity != DiagnosticSeverity.Error);

    public static IEnumerable<Diagnostic> AnalyzerDiagnostics(Compilation compilation, DiagnosticAnalyzer analyzer, CompilationErrorBehavior checkMode, string additionalFilePath = null, string[] onlyDiagnostics = null) =>
        AnalyzerDiagnostics(compilation, new[] { analyzer }, checkMode, additionalFilePath, onlyDiagnostics);

    public static IEnumerable<Diagnostic> AnalyzerDiagnostics(Compilation compilation, DiagnosticAnalyzer[] analyzers, CompilationErrorBehavior checkMode, string additionalFilePath = null, string[] onlyDiagnostics = null) =>
        VerifyNoExceptionThrown(DiagnosticsAndErrors(compilation, analyzers, checkMode, additionalFilePath, onlyDiagnostics));

    public static IEnumerable<Diagnostic> AnalyzerExceptions(Compilation compilation, DiagnosticAnalyzer analyzer) =>
        DiagnosticsAndErrors(compilation, new[] { analyzer }, CompilationErrorBehavior.FailTest).Where(x => x.Id == AD0001);

    private static ImmutableArray<Diagnostic> DiagnosticsAndErrors(Compilation compilation,
                                                                   DiagnosticAnalyzer[] analyzer,
                                                                   CompilationErrorBehavior checkMode, // ToDo: Remove in https://github.com/SonarSource/sonar-dotnet/issues/8588
                                                                   string additionalFilePath = null,
                                                                   string[] onlyDiagnostics = null)
    {
        onlyDiagnostics ??= Array.Empty<string>();
        var supportedDiagnostics = analyzer
            .SelectMany(x => x.SupportedDiagnostics.Select(d => d.Id))
            .ToImmutableDictionary(x => x, Severity)
            .Add(AD0001, ReportDiagnostic.Error);
        var compilationOptions = compilation.Options.WithSpecificDiagnosticOptions(supportedDiagnostics);
        var analyzerOptions = string.IsNullOrWhiteSpace(additionalFilePath) ? null : AnalysisScaffolding.CreateOptions(additionalFilePath);
        var diagnostics = compilation
            .WithOptions(compilationOptions)
            .WithAnalyzers(analyzer.ToImmutableArray(), analyzerOptions)
            .GetAllDiagnosticsAsync(default)
            .Result
            .Where(x => (x.Severity == DiagnosticSeverity.Error && x.Id != LineContinuationVB12) || supportedDiagnostics.ContainsKey(x.Id));   // No compiler info about new syntax or unused usings

        return checkMode == CompilationErrorBehavior.Ignore    // ToDo: Remove in https://github.com/SonarSource/sonar-dotnet/issues/8588
            ? diagnostics.Where(x => x.Id == AD0001 || x.Severity != DiagnosticSeverity.Error).ToImmutableArray()
            : diagnostics.ToImmutableArray();

        ReportDiagnostic Severity(string id) =>
            onlyDiagnostics.Length == 0 || onlyDiagnostics.Contains(id) ? ReportDiagnostic.Warn : ReportDiagnostic.Suppress;
    }

    private static void Compare(string languageVersion, CompilationIssues actual, CompilationIssues expected)
    {
        var messages = new List<VerificationMessage>();
        foreach (var filePairs in MatchPairs(actual, expected).GroupBy(x => x.FilePath).OrderBy(x => x.Key))
        {
            messages.Add(new(null, $"There are differences for {languageVersion} {SerializePath(filePairs.Key)}:", null, 0));
            foreach (var pair in filePairs.OrderBy(x => (x.Type, x.LineNumber, x.Start, x.IssueId, x.RuleId)))
            {
                messages.Add(pair.CreateMessage());
            }
            messages.Add(VerificationMessage.EmptyLine);
        }
        if (messages.Any())
        {
            throw new DiagnosticVerifierException(messages);
        }
        else
        {
            actual.Dump(languageVersion);
        }

        static string SerializePath(string path) =>
            path == string.Empty ? "<project-level-issue>" : path;
    }

    private static IEnumerable<SyntaxTree> ExceptRazorGeneratedFiles(this IEnumerable<SyntaxTree> syntaxTrees) =>
        syntaxTrees.Where(x =>
            !x.FilePath.EndsWith("razor.g.cs", StringComparison.OrdinalIgnoreCase)
            && !x.FilePath.EndsWith("cshtml.g.cs", StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<Diagnostic> VerifyNoExceptionThrown(IEnumerable<Diagnostic> diagnostics) =>
        diagnostics.Should().NotContain(d => d.Id == AD0001).And.Subject;

    private static IEnumerable<IssueLocationPair> MatchPairs(CompilationIssues actual, CompilationIssues expected)
    {
        var ret = new List<IssueLocationPair>();
        // Process file-level issues before project-level that match to any expected file issue
        // Then process primary before secondary, so we can update primary.IssueId for the purpose of matching seconday issues correctly.
        foreach (var key in actual.UniqueKeys().OrderBy(x => x.FilePath == string.Empty ? 1 : 0).ThenBy(x => x.Type))
        {
            ret.AddRange(MatchDifferences(actual.Remove(key), expected.Remove(key)));
        }
        ret.AddRange(expected.Select(x => new IssueLocationPair(null, x)));
        return ret;
    }

    private static IEnumerable<IssueLocationPair> MatchDifferences(List<IssueLocation> actualIssues, List<IssueLocation> expectedIssues)
    {
        foreach (var actual in actualIssues.ToArray())  // First round removes all perfect matches, so we don't mismatch possible perfect match by imperfect one on the same line.
        {
            var expectedIndex = expectedIssues.IndexOf(actual);
            if (expectedIndex >= 0)
            {
                actual.UpdatePrimaryIssueIdFrom(expectedIssues[expectedIndex]);
                expectedIssues.RemoveAt(expectedIndex);
                actualIssues.Remove(actual);
            }
        }
        foreach (var actual in actualIssues)
        {
            var expected = expectedIssues
                .OrderBy(x => actual.IssueId == x.IssueId ? 0 : 1)
                .ThenBy(x => Math.Abs(actual.Start ?? 0 - x.Start ?? 0))
                .ThenBy(x => Math.Abs(actual.Length ?? 0 - x.Length ?? 0))
                .FirstOrDefault();
            if (expected is not null)
            {
                actual.UpdatePrimaryIssueIdFrom(expected);
                expectedIssues.Remove(expected);
            }
            yield return new(actual, expected);
        }
        foreach (var expected in expectedIssues)
        {
            yield return new(null, expected);
        }
    }
}
