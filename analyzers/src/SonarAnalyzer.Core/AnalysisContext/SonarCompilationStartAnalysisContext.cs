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

using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using SonarAnalyzer.ShimLayer.AnalysisContext;

namespace SonarAnalyzer.AnalysisContext;

public sealed class SonarCompilationStartAnalysisContext : SonarAnalysisContextBase<CompilationStartAnalysisContext>
{
    private static readonly SyntaxTreeValueProvider<ShouldAnalyzeTreeCache> ShouldAnalyzeValueProvider = new(x => new ShouldAnalyzeTreeCache(x));

    public override Compilation Compilation => Context.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;
    internal SonarCompilationStartAnalysisContext(SonarAnalysisContext analysisContext, CompilationStartAnalysisContext context) : base(analysisContext, context) { }

    /// <inheritdoc cref="CompilationStartAnalysisContext.TryGetValue{TValue}(SourceText, SourceTextValueProvider{TValue}, out TValue)"/>
    public bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value) =>
        Context.TryGetValue(text, valueProvider, out value);

    /// <inheritdoc cref="CompilationStartAnalysisContext.TryGetValue{TValue}(SyntaxTree, SyntaxTreeValueProvider{TValue}, out TValue)"/>
    public bool TryGetValue<TValue>(SyntaxTree tree, SyntaxTreeValueProvider<TValue> valueProvider, out TValue value) =>
        Context.TryGetValue(tree, valueProvider, out value);

    public void RegisterCodeBlockStartAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarCodeBlockStartAnalysisContext<TSyntaxKind>> action)
        where TSyntaxKind : struct =>
        Context.RegisterCodeBlockStartAction<TSyntaxKind>(x => Execute(new(AnalysisContext, x), action, x.CodeBlock.SyntaxTree, generatedCodeRecognizer));

    public void RegisterSymbolAction(Action<SonarSymbolReportingContext> action, params SymbolKind[] symbolKinds) =>
        Context.RegisterSymbolAction(x => action(new(AnalysisContext, x)), symbolKinds);

    public void RegisterSymbolStartAction(Action<SonarSymbolStartAnalysisContext> action, SymbolKind symbolKind) =>
        Context.RegisterSymbolStartAction(x => action(new(AnalysisContext, x)), symbolKind);

    public void RegisterCompilationEndAction(Action<SonarCompilationReportingContext> action) =>
        Context.RegisterCompilationEndAction(x => action(new(AnalysisContext, x)));

    public void RegisterSemanticModelActionInAllFiles(Action<SonarSemanticModelReportingContext> action) =>
        Context.RegisterSemanticModelAction(x => action(new(AnalysisContext, x)));

    public void RegisterSemanticModelAction(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSemanticModelReportingContext> action) =>
        Context.RegisterSemanticModelAction(x => Execute(new(AnalysisContext, x), action, x.SemanticModel.SyntaxTree, generatedCodeRecognizer));

    public void RegisterTreeAction(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxTreeReportingContext> action) =>
        Context.RegisterSyntaxTreeAction(x => Execute(new(AnalysisContext, x, Context.Compilation), action, x.Tree, generatedCodeRecognizer));

#pragma warning disable HAA0303, HAA0302, HAA0301, HAA0502

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/8406", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    public void RegisterNodeAction<TSyntaxKind>(GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] syntaxKinds)
        where TSyntaxKind : struct =>
        Context.RegisterSyntaxNodeAction(x => Execute(new(AnalysisContext, x), action, x.Node.SyntaxTree, generatedCodeRecognizer), syntaxKinds);

    private void Execute<TSonarContext>(TSonarContext context, Action<TSonarContext> action, SyntaxTree sourceTree, GeneratedCodeRecognizer generatedCodeRecognizer = null)
        where TSonarContext : IAnalysisContext
    {
        Debug.Assert(context.HasMatchingScope(AnalysisContext.SupportedDiagnostics), "SonarAnalysisContext.Execute does this check. It should never be needed here.");
        if (!TryGetValue(sourceTree, ShouldAnalyzeValueProvider, out var shouldAnalyzeTree))
        {
            shouldAnalyzeTree = new ShouldAnalyzeTreeCache(sourceTree);
        }
        if (shouldAnalyzeTree.ShouldAnalyze(context, generatedCodeRecognizer))
        {
            action(context);
        }
    }
}
