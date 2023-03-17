/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using SonarAnalyzer.SymbolicExecution.Sonar;
using SonarRules = SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SymbolicExecutionRunner : SymbolicExecutionRunnerBase
    {
        // ToDo: This should be migrated to SymbolicExecutionRunnerBase.AllRules.
        private static readonly ImmutableArray<ISymbolicExecutionAnalyzer> SonarRules = ImmutableArray.Create<ISymbolicExecutionAnalyzer>(
            new SonarRules.ObjectsShouldNotBeDisposedMoreThanOnce(),
            new SonarRules.EmptyCollectionsShouldNotBeEnumerated(),
            new SonarRules.ConditionEvaluatesToConstant(),
            new SonarRules.InvalidCastToInterfaceSymbolicExecution(),
            new SonarRules.RestrictDeserializedTypes(),
            new SonarRules.InitializationVectorShouldBeRandom(),
            new SonarRules.HashesShouldHaveUnpredictableSalt());

        public SymbolicExecutionRunner() : this(AnalyzerConfiguration.AlwaysEnabled) { }

        internal /* for testing */ SymbolicExecutionRunner(IAnalyzerConfiguration configuration) : base(configuration) { }

        protected override ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules { get; } = ImmutableDictionary<DiagnosticDescriptor, RuleFactory>.Empty
            .Add(LocksReleasedAllPaths.S2222, CreateFactory<LocksReleasedAllPaths>())
            .Add(NullPointerDereference.S2259, CreateFactory<NullPointerDereference, SonarRules.NullPointerDereference>())
            .Add(EmptyNullableValueAccess.S3655, CreateFactory<EmptyNullableValueAccess, SonarRules.EmptyNullableValueAccess>())
            .Add(PublicMethodArgumentsShouldBeCheckedForNull.S3900, CreateFactory<PublicMethodArgumentsShouldBeCheckedForNull, SonarRules.PublicMethodArgumentsShouldBeCheckedForNull>());

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => base.SupportedDiagnostics.Concat(SonarRules.SelectMany(x => x.SupportedDiagnostics)).ToImmutableArray();

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => Analyze<BaseMethodDeclarationSyntax>(context, c, x => (SyntaxNode)x.Body ?? x.ExpressionBody()),
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.MethodDeclaration);

            context.RegisterNodeAction(
                c => Analyze<PropertyDeclarationSyntax>(context, c, x => x.ExpressionBody?.Expression),
                SyntaxKind.PropertyDeclaration);

            context.RegisterNodeAction(
                c => Analyze<IndexerDeclarationSyntax>(context, c, x => x.ExpressionBody?.Expression),
                SyntaxKind.IndexerDeclaration);

            context.RegisterNodeAction(
                c => Analyze<AccessorDeclarationSyntax>(context, c, x => (SyntaxNode)x.Body ?? x.ExpressionBody()),
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKindEx.InitAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterNodeAction(
                c =>
                {
                    var declaration = (AnonymousFunctionExpressionSyntax)c.Node;
                    if (c.SemanticModel.GetSymbolInfo(declaration).Symbol is { } symbol && !c.IsInExpressionTree())
                    {
                        Analyze(context, c, declaration.Body, symbol);
                    }
                },
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        protected override ControlFlowGraph CreateCfg(SemanticModel model, SyntaxNode node, CancellationToken cancel) =>
            node.CreateCfg(model, cancel);

        protected override void AnalyzeSonar(SonarSyntaxNodeReportingContext context, SyntaxNode body, ISymbol symbol)
        {
            var enabledAnalyzers = AllRules.Select(x => x.Value.CreateSonarFallback(Configuration))
                                           .WhereNotNull()
                                           .Cast<ISymbolicExecutionAnalyzer>() // ISymbolicExecutionAnalyzer should be passed as TSonarFallback to CreateFactory. Have you passed a Roslyn rule instead?
                                           .Union(SonarRules)
                                           .Where(x => x.SupportedDiagnostics.Any(descriptor => IsEnabled(context, descriptor)))
                                           .ToList();
            if (enabledAnalyzers.Any() && CSharpControlFlowGraph.TryGet((CSharpSyntaxNode)body, context.SemanticModel, out var cfg))
            {
                var lva = new SonarCSharpLiveVariableAnalysis(cfg, symbol, context.SemanticModel, context.Cancel);
                try
                {
                    var explodedGraph = new SonarExplodedGraph(cfg, symbol, context.SemanticModel, lva);
                    var analyzerContexts = enabledAnalyzers.Select(x => x.CreateContext(context, explodedGraph)).ToList();
                    try
                    {
                        explodedGraph.ExplorationEnded += ExplorationEndedHandlerSonar;
                        explodedGraph.Walk();
                    }
                    finally
                    {
                        explodedGraph.ExplorationEnded -= ExplorationEndedHandlerSonar;
                    }

                    // Some of the rules can return good results if the tree was only partially visited; others need to completely
                    // walk the tree in order to avoid false positives.
                    //
                    // Due to this we split the rules in two sets and report the diagnostics in steps:
                    // - When the tree is successfully visited and ExplorationEnded event is raised.
                    // - When the tree visit ends (explodedGraph.Walk() returns). This will happen even if the maximum number of steps was
                    // reached or if an exception was thrown during analysis.
                    ReportDiagnosticsSonar(context, analyzerContexts, true);

                    void ExplorationEndedHandlerSonar(object sender, EventArgs args) =>
                        ReportDiagnosticsSonar(context, analyzerContexts, false);
                }
                catch (Exception ex)
                {
                    throw new SymbolicExecutionException(ex, symbol, body.GetLocation());
                }
            }
        }

        private static void ReportDiagnosticsSonar(SonarSyntaxNodeReportingContext context, IEnumerable<ISymbolicExecutionAnalysisContext> analyzerContexts, bool supportsPartialResults)
        {
            foreach (var analyzerContext in analyzerContexts.Where(x => x.SupportsPartialResults == supportsPartialResults))
            {
                foreach (var diagnostic in analyzerContext.GetDiagnostics(context.Compilation))
                {
                    context.ReportIssue(diagnostic);
                }
                analyzerContext.Dispose();
            }
        }
    }
}
