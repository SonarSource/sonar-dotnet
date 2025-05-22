/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Verification;

public static class DiagnosticVerifier
{
    private const string AD0001 = nameof(AD0001);
    private const string LineContinuationVB12 = "BC36716";  // Visual Basic 12.0 does not support line continuation comments.

    public static int Verify(Compilation compilation,
                             DiagnosticAnalyzer[] analyzers,
                             CompilationErrorBehavior checkMode, // ToDo: Remove this parameter in https://github.com/SonarSource/sonar-dotnet/issues/8588
                             string additionalFilePath,
                             string[] onlyDiagnostics,
                             string[] additionalSourceFiles,
                             bool? concurrentAnalysis = null)
    {
        SuppressionHandler.HookSuppression();
        try
        {
            var sources = compilation.SyntaxTrees.ExceptRazorGeneratedFiles()
                .Select(x => new FileContent(x))
                .Concat((additionalSourceFiles ?? Array.Empty<string>()).Select(x => new FileContent(x)));
            var diagnostics = DiagnosticsAndErrors(compilation, analyzers, checkMode, additionalFilePath, onlyDiagnostics, concurrentAnalysis).ToArray();
            var expected = new CompilationIssues(sources);
            VerifyNoExceptionThrown(diagnostics);
            Compare(compilation.LanguageVersionString(), new(diagnostics), expected);
            // When there are no issues reported from the test (the FileLines analyzer does not report in each call to Verifier.VerifyAnalyzer) we skip the check for the extension method.
            if (diagnostics.Any(x => x.Severity != DiagnosticSeverity.Error))
            {
                SuppressionHandler.ExtensionMethodsCalledForAllDiagnostics(analyzers).Should().BeTrue("The ReportIssue should be used instead of ReportDiagnostic");
            }
            return diagnostics.Length;
        }
        finally
        {
            SuppressionHandler.UnHookSuppression();
        }
    }

    public static void VerifyNoIssues(Compilation compilation,
                                      DiagnosticAnalyzer analyzer,
                                      CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                      string additionalFilePath = null,
                                      string[] onlyDiagnostics = null) =>
        AnalyzerDiagnostics(compilation, analyzer, checkMode, additionalFilePath, onlyDiagnostics).Should().BeEmpty();

    public static void VerifyNoIssuesIgnoreErrors(Compilation compilation,
                                                  DiagnosticAnalyzer analyzer,
                                                  CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                  string additionalFilePath = null,
                                                  string[] onlyDiagnostics = null)
    {
        var diagnostics = AnalyzerDiagnostics(compilation, analyzer, checkMode, additionalFilePath, onlyDiagnostics);
        diagnostics.Should().NotContain(x => x.Severity != DiagnosticSeverity.Error);
    }

    public static IEnumerable<Diagnostic> AnalyzerDiagnostics(Compilation compilation, DiagnosticAnalyzer analyzer, CompilationErrorBehavior checkMode, string additionalFilePath = null, string[] onlyDiagnostics = null) =>
        AnalyzerDiagnostics(compilation, [analyzer], checkMode, additionalFilePath, onlyDiagnostics);

    public static IEnumerable<Diagnostic> AnalyzerDiagnostics(Compilation compilation, DiagnosticAnalyzer[] analyzers, CompilationErrorBehavior checkMode, string additionalFilePath = null, string[] onlyDiagnostics = null) =>
        VerifyNoExceptionThrown(DiagnosticsAndErrors(compilation, analyzers, checkMode, additionalFilePath, onlyDiagnostics));

    public static IEnumerable<Diagnostic> AnalyzerExceptions(Compilation compilation, DiagnosticAnalyzer analyzer) =>
        DiagnosticsAndErrors(compilation, [analyzer], CompilationErrorBehavior.FailTest, null, null).Where(x => x.Id == AD0001);

    private static ImmutableArray<Diagnostic> DiagnosticsAndErrors(Compilation compilation,
                                                                   DiagnosticAnalyzer[] analyzer,
                                                                   CompilationErrorBehavior checkMode, // ToDo: Remove in https://github.com/SonarSource/sonar-dotnet/issues/8588
                                                                   string additionalFilePath,
                                                                   string[] onlyDiagnostics,
                                                                   bool? concurrentAnalysis = null)
    {
        using var scope = concurrentAnalysis.HasValue ? new EnvironmentVariableScope { EnableConcurrentAnalysis = concurrentAnalysis.Value } : null;
        onlyDiagnostics ??= [];
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
            foreach (var pair in from x in filePairs orderby x.Type, x.LineNumber, x.Start, x.IssueId, x.RuleId select x)
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
