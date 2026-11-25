/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Syntax.Extensions;
using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Verification;

public static class DiagnosticVerifier
{
    private const string AD0001 = nameof(AD0001);
    private const string LineContinuationVB12 = "BC36716";  // Visual Basic 12.0 does not support line continuation comments.
    private const string ConcurrentInfix = ".Concurrent";

    public static int Verify(Compilation compilation,
                             DiagnosticAnalyzer[] analyzers,
                             CompilationErrorBehavior checkMode, // ToDo: Remove this parameter in https://github.com/SonarSource/sonar-dotnet/issues/8588
                             string additionalFilePath,
                             string[] onlyDiagnostics,
                             string[] additionalSourceFiles,
                             bool? concurrentAnalysis = null,
                             string[] warningsAsErrors = null,
                             bool ideAnalysisCheck = false)
    {
        // Analyzers read the concurrency setting when they are initialized, so the scope has to stay open for all analysis below.
        using var scope = concurrentAnalysis.HasValue ? new EnvironmentVariableScope { EnableConcurrentAnalysis = concurrentAnalysis.Value } : null;
        Diagnostic[] diagnostics;
        SuppressionHandler.HookSuppression();
        try
        {
            var sources = ExceptRazorGeneratedFiles(compilation.SyntaxTrees)
                .Select(x => new FileContent(x))
                .Concat((additionalSourceFiles ?? Array.Empty<string>()).Select(x => new FileContent(x)));
            diagnostics = DiagnosticsAndErrors(compilation, analyzers, checkMode, additionalFilePath, onlyDiagnostics, warningsAsErrors).ToArray();
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
        if (ideAnalysisCheck)
        {
            VerifyIdeAnalysis(compilation, analyzers, diagnostics, additionalFilePath, onlyDiagnostics, warningsAsErrors);
        }
        return diagnostics.Length;
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

    /// <summary>
    /// IDEs (VS Code/OmniSharp, Rider, Visual Studio) request diagnostics per document. Roslyn drops diagnostics that are located outside of the node of the callback that reported them,
    /// because it classifies them as non-local (see AnalyzerExecutor.AnalyzerDiagnosticReporter.FilterSpanForLocalDiagnostics). Full compilation analysis (build, scanner) does not drop them.
    /// This fails if the per-document analysis returns fewer diagnostics than the full compilation analysis.
    /// </summary>
    private static void VerifyIdeAnalysis(Compilation compilation,
                                          DiagnosticAnalyzer[] analyzers,
                                          Diagnostic[] fullAnalysisDiagnostics,
                                          string additionalFilePath,
                                          string[] onlyDiagnostics,
                                          string[] warningsAsErrors)
    {
        // The compilation contains the generated .Concurrent copies of the files. They are identical to the originals, so they are not analyzed twice.
        var trees = ExceptRazorGeneratedFiles(compilation.SyntaxTrees).Where(x => !IsConcurrentCopy(x.FilePath)).ToHashSet();
        var expected = ExpectedIdeDiagnostics(fullAnalysisDiagnostics, analyzers, trees);
        var (perDocument, _) = CreateCompilationWithAnalyzers(compilation, analyzers, additionalFilePath, onlyDiagnostics, warningsAsErrors);
        var actual = PerDocumentDiagnostics(perDocument, trees);
        var missing = expected.Where(x => !actual.Contains(x.Key))
            .OrderBy(x => x.Value.Path, StringComparer.Ordinal).ThenBy(x => x.Value.StartLinePosition.Line).ThenBy(x => x.Value.StartLinePosition.Character)
            .Select(x => x.Key)
            .ToArray();
        if (missing.Length > 0)
        {
            throw new AssertFailedException(IdeAnalysisFailureMessage(missing));
        }
        // Compilation end diagnostics without a source location are not part of the comparison above, so check the registered actions as well.
        var untagged = UntaggedCompilationEndAnalyzers(perDocument, analyzers);
        if (untagged.Length > 0)
        {
            throw new AssertFailedException(UntaggedCompilationEndMessage(untagged));
        }
    }

    private static DiagnosticAnalyzer[] UntaggedCompilationEndAnalyzers(CompilationWithAnalyzers compilationWithAnalyzers, DiagnosticAnalyzer[] analyzers) =>
        analyzers
            .Where(x => compilationWithAnalyzers.GetAnalyzerTelemetryInfoAsync(x, CancellationToken.None).Result.CompilationEndActionsCount > 0
                        && !x.SupportedDiagnostics.Any(d => d.CustomTags.Contains(WellKnownDiagnosticTags.CompilationEnd)))
            .ToArray();

    private static string UntaggedCompilationEndMessage(DiagnosticAnalyzer[] analyzers) =>
        $"""
        IDE analysis check failed: {string.Join(", ", analyzers.Select(x => x.GetType().Name))} register(s) a compilation end action, but none of the descriptors has the CompilationEnd tag.

        Problem: per-document analysis never runs compilation end actions, and the tag is how IDEs and Roslyn analyzer RS1037 recognize such diagnostics.

        What to do:
          - Add the CompilationEnd tag to the descriptors that are reported from the compilation end action (isCompilationEnd: true), see NET-1637.
          - The behavior is by design: call WithoutIdeAnalysisCheck("<reason: 'by design' and why, or the ticket>") on the VerifierBuilder of this test.
        """;

    private static Dictionary<string, FileLinePositionSpan> ExpectedIdeDiagnostics(Diagnostic[] fullAnalysisDiagnostics, DiagnosticAnalyzer[] analyzers, HashSet<SyntaxTree> trees)
    {
        // The full analysis diagnostics also contain compiler errors, which are not part of the analyzer results of the per-document analysis. Only the diagnostics of the analyzers under test are compared.
        var supportedIds = analyzers.SelectMany(x => x.SupportedDiagnostics).Select(x => x.Id).Where(x => x != AD0001).ToHashSet();
        return fullAnalysisDiagnostics
            .Where(x => supportedIds.Contains(x.Id)
                && x.Location.IsInSource
                && trees.Contains(x.Location.SourceTree)
                && !x.Descriptor.CustomTags.Contains(WellKnownDiagnosticTags.CompilationEnd))   // Compilation end diagnostics are never part of per-document analysis, by design
            .GroupBy(Describe)  // Roslyn de-duplicates identical diagnostics in per-document results, so compare as sets
            .ToDictionary(x => x.Key, x => x.First().Location.GetLineSpan());
    }

    private static HashSet<string> PerDocumentDiagnostics(CompilationWithAnalyzers compilationWithAnalyzers, HashSet<SyntaxTree> trees)
    {
        var actual = new HashSet<string>();
        foreach (var tree in trees)
        {
            var model = compilationWithAnalyzers.Compilation.GetSemanticModel(tree);
            actual.UnionWith(compilationWithAnalyzers.GetAnalyzerSemanticDiagnosticsAsync(model, null, default).Result.Select(Describe));
            actual.UnionWith(compilationWithAnalyzers.GetAnalyzerSyntaxDiagnosticsAsync(tree, default).Result.Select(Describe));
        }
        return actual;
    }

    private static string IdeAnalysisFailureMessage(string[] missing) =>
        $"""
        IDE analysis check failed: {missing.Length} diagnostic(s) are reported by the full compilation analysis (build, scanner), but not by the per-document analysis that IDEs use:
        {string.Join(Environment.NewLine, missing.Select(x => "  " + x))}

        Problem: the per-document analysis only keeps a diagnostic that is located inside the node of the callback that reported it, and never keeps one from a compilation end action.
                 The user does not see this issue in the IDE, while the build and the server report it.

        What to do:
          - The rule reports outside of the node of its callback: report from a node that contains the location of the issue,
            e.g. register on the outermost node that covers all locations of the issue.
          - The rule reports from a compilation end action: add the CompilationEnd tag to its descriptor (isCompilationEnd: true), see Roslyn analyzer RS1037 and NET-1637.
          - The issue is in the implementation of a partial property, indexer, event or constructor: Roslyn does not analyze it per document (NET-5179). Opt out with a reason.
          - The rule cannot be fixed now, or the behavior is by design: call WithoutIdeAnalysisCheck("<reason: 'by design' and why, or the ticket>") on the VerifierBuilder of this test.
        """;

    private static string Describe(Diagnostic diagnostic)
    {
        var span = diagnostic.Location.GetLineSpan();
        return $"{diagnostic.Id} {Path.GetFileName(span.Path)}@{span.StartLinePosition.Line + 1}:{span.StartLinePosition.Character + 1} {diagnostic.GetMessage()}";
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
                                                                   string[] warningsAsErrors = null)
    {
        var (compilationWithAnalyzers, supportedDiagnostics) = CreateCompilationWithAnalyzers(compilation, analyzer, additionalFilePath, onlyDiagnostics, warningsAsErrors);
        var diagnostics = compilationWithAnalyzers
            .GetAllDiagnosticsAsync(default)
            .Result
            .Where(x => (x.Severity == DiagnosticSeverity.Error && x.Id != LineContinuationVB12) || supportedDiagnostics.ContainsKey(x.Id));   // No compiler info about new syntax or unused usings

        return checkMode == CompilationErrorBehavior.Ignore    // ToDo: Remove in https://github.com/SonarSource/sonar-dotnet/issues/8588
            ? diagnostics.Where(x => x.Id == AD0001 || x.Severity != DiagnosticSeverity.Error).ToImmutableArray()
            : diagnostics.ToImmutableArray();
    }

    private static (CompilationWithAnalyzers CompilationWithAnalyzers, ImmutableDictionary<string, ReportDiagnostic> SupportedDiagnostics) CreateCompilationWithAnalyzers(
        Compilation compilation,
        DiagnosticAnalyzer[] analyzers,
        string additionalFilePath,
        string[] onlyDiagnostics,
        string[] warningsAsErrors)
    {
        onlyDiagnostics ??= [];
        var supportedDiagnostics = analyzers
            .SelectMany(x => x.SupportedDiagnostics.Select(d => d.Id))
            .ToImmutableDictionary(x => x, Severity)
            .Add(AD0001, ReportDiagnostic.Error);
        foreach (var id in warningsAsErrors ?? [])
        {
            supportedDiagnostics = supportedDiagnostics.SetItem(id, ReportDiagnostic.Error);
        }
        var compilationOptions = compilation.Options.WithSpecificDiagnosticOptions(supportedDiagnostics);
        var analyzerOptions = string.IsNullOrWhiteSpace(additionalFilePath) ? null : AnalysisScaffolding.CreateOptions(additionalFilePath);
        return (compilation.WithOptions(compilationOptions).WithAnalyzers(analyzers.ToImmutableArray(), analyzerOptions), supportedDiagnostics);

        ReportDiagnostic Severity(string id) =>
            onlyDiagnostics.Length == 0 || onlyDiagnostics.Contains(id) ? ReportDiagnostic.Warn : ReportDiagnostic.Suppress;
    }

    private static void Compare(string languageVersion, CompilationIssues actual, CompilationIssues expected)
    {
        var messages = new List<VerificationMessage>();
        foreach (var filePairs in MatchPairs(actual, expected).GroupBy(x => x.FilePath.Replace(ConcurrentInfix, null)).OrderBy(x => x.Key))
        {
            var defaultPairs = new List<IssueLocationPair>();
            var concurrentPairs = new List<IssueLocationPair>();
            foreach (var pair in filePairs)
            {
                (IsConcurrentCopy(pair.FilePath) ? concurrentPairs : defaultPairs).Add(pair);
            }
            messages.AddRange(SerializePairs(languageVersion, defaultPairs, concurrentPairs));
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
    }

    private static IEnumerable<VerificationMessage> SerializePairs(string languageVersion, List<IssueLocationPair> defaultPairs, List<IssueLocationPair> concurrentPairs)
    {
        if (defaultPairs.Any())
        {
            return concurrentPairs.Count switch
                {   // Avoid redundant message dumps
                    0 => CreateMessages(defaultPairs),
                    1 => CreateMessages(defaultPairs).Append(new(null, $"There is 1 more difference in {SerializePath(concurrentPairs[0].FilePath)}", null, 0)),
                    _ => CreateMessages(defaultPairs).Append(new(null, $"There are {concurrentPairs.Count} more differences in {SerializePath(concurrentPairs[0].FilePath)}", null, 0))
                };
        }
        else
        {
            return CreateMessages(concurrentPairs);
        }

        IEnumerable<VerificationMessage> CreateMessages(List<IssueLocationPair> pairs)
        {
            yield return new(null, $"There are differences for {languageVersion} {SerializePath(pairs[0].FilePath)}:", null, 0);
            foreach (var pair in pairs.OrderBy(x => x.Type).ThenBy(x => x.LineNumber).ThenBy(x => x.Start).ThenBy(x => x.IssueId).ThenBy(x => x.RuleId))
            {
                yield return pair.CreateMessage();
            }
        }

        static string SerializePath(string path) =>
            path == string.Empty ? "<project-level-issue>" : path;
    }

    // Verifier.CreateConcurrencyTest generates a copy of each file with this infix in its name.
    private static bool IsConcurrentCopy(string filePath) =>
        filePath.Contains(ConcurrentInfix);

    private static IEnumerable<SyntaxTree> ExceptRazorGeneratedFiles(IEnumerable<SyntaxTree> syntaxTrees) =>
        syntaxTrees.Where(x =>
            !x.EndsWith("razor.g.cs")
            && !x.EndsWith("cshtml.g.cs")
            && !x.EndsWith("razor.decl.g.cs"));

    private static IEnumerable<Diagnostic> VerifyNoExceptionThrown(IEnumerable<Diagnostic> diagnostics) =>
        diagnostics.Should().NotContain(x => x.Id == AD0001).And.Subject;

    private static IEnumerable<IssueLocationPair> MatchPairs(CompilationIssues actual, CompilationIssues expected)
    {
        var ret = new List<IssueLocationPair>();
        // Process file-level issues before project-level that match to any expected file issue
        // Then process primary before secondary, so we can update primary.IssueId for the purpose of matching secondary issues correctly.
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
