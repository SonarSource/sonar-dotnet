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

using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.SymbolicExecution.Roslyn.CSharp;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using SonarAnalyzer.SymbolicExecution.Sonar;
using SonarRules = SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SymbolicExecutionRunner : SymbolicExecutionRunnerBase
{
    protected override ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules { get; } = ImmutableDictionary<DiagnosticDescriptor, RuleFactory>.Empty
        .Add(InvalidCastToInterface.S1944, CreateFactory<EmptyRuleCheck, SonarRules.InvalidCastToInterfaceSymbolicExecution>()) // This old SE rule is part of S3655.
        .Add(HashesShouldHaveUnpredictableSalt.S2053, CreateFactory<HashesShouldHaveUnpredictableSalt, SonarRules.HashesShouldHaveUnpredictableSalt>())
        .Add(LocksReleasedAllPaths.S2222, CreateFactory<LocksReleasedAllPaths>())
        .Add(NullPointerDereference.S2259, CreateFactory<NullPointerDereference, SonarRules.NullPointerDereference>())
        .Add(ConditionEvaluatesToConstant.S2583, CreateFactory<ConditionEvaluatesToConstant, SonarRules.ConditionEvaluatesToConstant>())
        .Add(ConditionEvaluatesToConstant.S2589, CreateFactory<ConditionEvaluatesToConstant, SonarRules.ConditionEvaluatesToConstant>())
        .Add(InitializationVectorShouldBeRandom.S3329, CreateFactory<InitializationVectorShouldBeRandom, SonarRules.InitializationVectorShouldBeRandom>())
        .Add(EmptyNullableValueAccess.S3655, CreateFactory<EmptyNullableValueAccess, SonarRules.EmptyNullableValueAccess>())
        .Add(PublicMethodArgumentsShouldBeCheckedForNull.S3900, CreateFactory<PublicMethodArgumentsShouldBeCheckedForNull, SonarRules.PublicMethodArgumentsShouldBeCheckedForNull>())
        .Add(CalculationsShouldNotOverflow.S3949, CreateFactory<CalculationsShouldNotOverflow>())
        .Add(ObjectsShouldNotBeDisposedMoreThanOnce.S3966, CreateFactory<ObjectsShouldNotBeDisposedMoreThanOnce, SonarRules.ObjectsShouldNotBeDisposedMoreThanOnce>())
        .Add(EmptyCollectionsShouldNotBeEnumerated.S4158, CreateFactory<EmptyCollectionsShouldNotBeEnumerated, SonarRules.EmptyCollectionsShouldNotBeEnumerated>())
        .Add(RestrictDeserializedTypes.S5773, CreateFactory<RestrictDeserializedTypes, SonarRules.RestrictDeserializedTypes>())
        .Add(SecureRandomSeedsShouldNotBePredictable.S4347, CreateFactory<SecureRandomSeedsShouldNotBePredictable>());

    protected override SyntaxClassifierBase SyntaxClassifier => CSharpSyntaxClassifier.Instance;
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => base.SupportedDiagnostics.ToImmutableArray();

    public SymbolicExecutionRunner() : this(AnalyzerConfiguration.AlwaysEnabled) { }
    internal /* for testing */ SymbolicExecutionRunner(IAnalyzerConfiguration configuration) : base(configuration) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var compilationUnit = (CompilationUnitSyntax)c.Node;
                if (compilationUnit.IsTopLevelMain() && c.SemanticModel.GetDeclaredSymbol(compilationUnit) is { } symbol)
                {
                    Analyze(context, c, symbol);
                }
            },
            SyntaxKind.CompilationUnit);

        context.RegisterNodeAction(
            c => Analyze(context, c),
            SyntaxKind.AddAccessorDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKindEx.InitAccessorDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.RemoveAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration);

        context.RegisterNodeAction(
            c =>
            {
                if (c.SemanticModel.GetSymbolInfo(c.Node).Symbol is { } symbol && !c.IsInExpressionTree())
                {
                    Analyze(context, c, symbol);
                }
            },
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.SimpleLambdaExpression);
    }

    protected override ControlFlowGraph CreateCfg(SemanticModel model, SyntaxNode node, CancellationToken cancel) =>
        node.CreateCfg(model, cancel);

    protected override void AnalyzeSonar(SonarSyntaxNodeReportingContext context, ISymbol symbol)
    {
        var enabledAnalyzers = AllRules.GroupBy(x => x.Value.Type)         // Multiple DiagnosticDescriptors (S2583, S2589) can share the same check type
                                       .Select(x => x.First().Value.CreateSonarFallback(Configuration))
                                       .WhereNotNull()
                                       .Cast<ISymbolicExecutionAnalyzer>() // ISymbolicExecutionAnalyzer should be passed as TSonarFallback to CreateFactory. Have you passed a Roslyn rule instead?
                                       .Where(x => x.SupportedDiagnostics.Any(descriptor => descriptor.IsEnabled(context)))
                                       .ToList();
        if (enabledAnalyzers.Any() && CSharpControlFlowGraph.TryGet(context.Node, context.SemanticModel, out var cfg))
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
                throw new SymbolicExecutionException(ex, symbol, context.Node.GetLocation());
            }
        }
    }

    private static void ReportDiagnosticsSonar(SonarSyntaxNodeReportingContext context, IEnumerable<ISymbolicExecutionAnalysisContext> analyzerContexts, bool supportsPartialResults)
    {
        foreach (var analyzerContext in analyzerContexts.Where(x => x.SupportsPartialResults == supportsPartialResults))
        {
            foreach (var diagnostic in analyzerContext.GetDiagnostics())
            {
                context.ReportIssue(diagnostic);
            }
            analyzerContext.Dispose();
        }
    }
}
