/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Roslyn.Utilities;

namespace SonarAnalyzer.Core.AnalysisContext;

internal class ShouldAnalyzeTreeCache
{
    private enum ShouldAnalyzeTree
    {
        NotComputed,
        True,
        False
    }

    private readonly HashSet<string> rulesDisabledForRazor =
    [
        "S103",
        "S104",
        "S109",
        "S113",
        "S1147",
        "S1192",
        "S1451",
    ];

    private SyntaxTree Tree { get; }
    private bool IsRazorFile { get; }
    private ShouldAnalyzeTree ShouldAnalyzeTreeResult { get; set; }

    public ShouldAnalyzeTreeCache(SyntaxTree tree)
    {
        Tree = tree;
        IsRazorFile = GeneratedCodeRecognizer.IsRazorGeneratedFile(tree);
    }

    /// <summary>
    /// For each action registered on context we need to do some pre-processing before actually calling the rule.
    /// SonarAnalysisContext.Execute ensures that the rule does apply to the current scope (main vs test source), so we do not need to check this here.
    /// We call an external delegate (set by legacy SonarLint for VS) to ensure the rule should be run (usually
    /// the decision is made on based on whether the project contains the analyzer as NuGet).
    /// We also make sure that Razor files are not analyzed by rules that are not supported for Razor.
    /// </summary>
    [PerformanceSensitive("")]
    internal bool ShouldAnalyze<TSonarContext>(TSonarContext context, GeneratedCodeRecognizer generatedCodeRecognizer) where TSonarContext : IAnalysisContext
    {
        if (ShouldAnalyzeTreeResult == ShouldAnalyzeTree.NotComputed)
        {
            // We assume that we can ignore the generatedCodeRecognizer in the future. We only use it for the very first time we do the computation
            // and assume all other generatedCodeRecognizer we could see here are returning the same result.
            // We do not care about race conditions when setting ShouldAnalyzeTreeResult because
            // we will set from NotComputed to True or False. Any concurrency happening here is safe.
            ShouldAnalyzeTreeResult = context.ShouldAnalyzeTree(Tree, generatedCodeRecognizer)
                ? ShouldAnalyzeTree.True
                : ShouldAnalyzeTree.False;
        }
        var supportedDiagnostics = context.AnalysisContext.SupportedDiagnostics;
        return ShouldAnalyzeTreeResult == ShouldAnalyzeTree.True
            && LegacyIsRegisteredActionEnabled(supportedDiagnostics)
            && ShouldAnalyzeRazorFile(supportedDiagnostics);
    }

    [PerformanceSensitive("")]
    private bool LegacyIsRegisteredActionEnabled(ImmutableArray<DiagnosticDescriptor> supportedDiagnostics) =>
        SonarAnalysisContext.ShouldExecuteRegisteredAction is null // Box supportedDiagnostics only, if really needed
        || SonarAnalysisContext.LegacyIsRegisteredActionEnabled(supportedDiagnostics, Tree); // We can not change the signature for compatibility reasons

    [PerformanceSensitive("")]
    private bool ShouldAnalyzeRazorFile(ImmutableArray<DiagnosticDescriptor> supportedDiagnostics) =>
        !IsRazorFile
            || !supportedDiagnostics.Any(x => (x.CustomTags.Count() == 1 && x.CustomTags.Contains(DiagnosticDescriptorFactory.TestSourceScopeTag))
                || rulesDisabledForRazor.Contains(x.Id));
}
