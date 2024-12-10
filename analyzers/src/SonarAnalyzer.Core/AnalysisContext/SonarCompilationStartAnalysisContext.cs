/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Collections.Concurrent;
using Roslyn.Utilities;
using SonarAnalyzer.ShimLayer.AnalysisContext;

namespace SonarAnalyzer.AnalysisContext;

public sealed class SonarCompilationStartAnalysisContext : SonarAnalysisContextBase<CompilationStartAnalysisContext>
{
    public override Compilation Compilation => Context.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;
    internal SonarCompilationStartAnalysisContext(SonarAnalysisContext analysisContext, CompilationStartAnalysisContext context) : base(analysisContext, context) { }

    public void RegisterCodeBlockStartAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarCodeBlockStartAnalysisContext<TSyntaxKind>> action)
        where TSyntaxKind : struct =>
        Context.RegisterCodeBlockStartAction<TSyntaxKind>(x => Execute(new(AnalysisContext, x), action, x.CodeBlock.SyntaxTree, generatedCodeRecognizer));

    public void RegisterSymbolAction(Action<SonarSymbolReportingContext> action, params SymbolKind[] symbolKinds) =>
        Context.RegisterSymbolAction(x => action(new(AnalysisContext, x)), symbolKinds);

    public void RegisterSymbolStartAction(Action<SonarSymbolStartAnalysisContext> action, SymbolKind symbolKind) =>
        Context.RegisterSymbolStartAction(x => action(new(AnalysisContext, x)), symbolKind);

    public void RegisterCompilationEndAction(Action<SonarCompilationReportingContext> action) =>
        Context.RegisterCompilationEndAction(x => action(new(AnalysisContext, x)));

    public void RegisterSemanticModelAction(Action<SonarSemanticModelReportingContext> action) =>
        Context.RegisterSemanticModelAction(x => action(new(AnalysisContext, x)));

    public void RegisterSemanticModelAction(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSemanticModelReportingContext> action) =>
        Context.RegisterSemanticModelAction(x => Execute(new(AnalysisContext, x), action, x.SemanticModel.SyntaxTree, generatedCodeRecognizer));

    public void RegisterTreeAction(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxTreeReportingContext> action) =>
        Context.RegisterSyntaxTreeAction(x => Execute(new(AnalysisContext, x, Context.Compilation), action, x.Tree, generatedCodeRecognizer));

#pragma warning disable HAA0303, HAA0302, HAA0301, HAA0502

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/8406", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    public void RegisterNodeAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] syntaxKinds)
        where TSyntaxKind : struct
    {
        if (this.HasMatchingScope(AnalysisContext.SupportedDiagnostics))
        {
            ConcurrentDictionary<SyntaxTree, bool> shouldAnalyzeCache = new();
            Context.RegisterSyntaxNodeAction(x =>
            // The hot path starts in the lambda below.
#pragma warning restore HAA0303, HAA0302, HAA0301, HAA0502
                {
                    if (!shouldAnalyzeCache.TryGetValue(x.Node.SyntaxTree, out var canProceedWithAnalysis))
                    {
                        canProceedWithAnalysis = GetOrAddCanProceedWithAnalysis(generatedCodeRecognizer, shouldAnalyzeCache, x.Node.SyntaxTree);
                    }

                    if (canProceedWithAnalysis)
                    {
#pragma warning disable HAA0502
                        // https://github.com/SonarSource/sonar-dotnet/issues/8425
                        action(new(AnalysisContext, x));
#pragma warning restore HAA0502
                    }
                },
                syntaxKinds);
        }
    }

    // Performance: Don't inline to avoid capture class and delegate allocations.
    private bool GetOrAddCanProceedWithAnalysis(GeneratedCodeRecognizer codeRecognizer, ConcurrentDictionary<SyntaxTree, bool> cache, SyntaxTree tree) =>
        cache.GetOrAdd(tree,
            x =>
                this.ShouldAnalyzeTree(x, codeRecognizer)
                && SonarAnalysisContext.LegacyIsRegisteredActionEnabled(AnalysisContext.SupportedDiagnostics, x)
                && AnalysisContext.ShouldAnalyzeRazorFile(x));

    private void Execute<TSonarContext>(TSonarContext context, Action<TSonarContext> action, SyntaxTree sourceTree, GeneratedCodeRecognizer generatedCodeRecognizer = null)
        where TSonarContext : IAnalysisContext
    {
        // For each action registered on context we need to do some pre-processing before actually calling the rule.
        // First, we need to ensure the rule does apply to the current scope (main vs test source).
        // Second, we call an external delegate (set by legacy SonarLint for VS) to ensure the rule should be run (usually
        // the decision is made on based on whether the project contains the analyzer as NuGet).
        if (context.ShouldAnalyzeTree(sourceTree, generatedCodeRecognizer)
            && SonarAnalysisContext.LegacyIsRegisteredActionEnabled(AnalysisContext.SupportedDiagnostics, sourceTree)
            && AnalysisContext.ShouldAnalyzeRazorFile(sourceTree))
        {
            action(context);
        }
    }
}
