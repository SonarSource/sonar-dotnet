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

    public void RegisterSymbolAction(Action<SonarSymbolReportingContext> action, params SymbolKind[] symbolKinds) =>
        Context.RegisterSymbolAction(x => action(new(AnalysisContext, x)), symbolKinds);

    public void RegisterSymbolStartAction(Action<SonarSymbolStartAnalysisContext> action, SymbolKind symbolKind) =>
        Context.RegisterSymbolStartAction(x => action(new(AnalysisContext, x)), symbolKind);

    public void RegisterCompilationEndAction(Action<SonarCompilationReportingContext> action) =>
        Context.RegisterCompilationEndAction(x => action(new(AnalysisContext, x)));

    public void RegisterSemanticModelAction(Action<SonarSemanticModelReportingContext> action) =>
        Context.RegisterSemanticModelAction(x => action(new(AnalysisContext, x)));

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
}
