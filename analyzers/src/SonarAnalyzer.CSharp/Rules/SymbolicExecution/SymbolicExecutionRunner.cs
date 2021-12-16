/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.SymbolicExecution.Roslyn.Checks;
using SonarAnalyzer.SymbolicExecution.Sonar;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.SymbolicExecution
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(ConditionEvaluatesToConstant.S2583DiagnosticId, LanguageNames.CSharp)]
    [Rule(ConditionEvaluatesToConstant.S2589DiagnosticId, LanguageNames.CSharp)]
    [Rule(EmptyCollectionsShouldNotBeEnumerated.DiagnosticId, LanguageNames.CSharp)]
    [Rule(EmptyNullableValueAccess.DiagnosticId, LanguageNames.CSharp)]
    [Rule(InitializationVectorShouldBeRandom.DiagnosticId, LanguageNames.CSharp)]
    [Rule(InvalidCastToInterfaceSymbolicExecution.DiagnosticId, LanguageNames.CSharp)]
    [Rule(LocksReleasedAllPathsBase.DiagnosticId, LanguageNames.CSharp)]
    [Rule(HashesShouldHaveUnpredictableSalt.DiagnosticId, LanguageNames.CSharp)]
    [Rule(NullPointerDereference.DiagnosticId, LanguageNames.CSharp)]
    [Rule(ObjectsShouldNotBeDisposedMoreThanOnce.DiagnosticId, LanguageNames.CSharp)]
    [Rule(PublicMethodArgumentsShouldBeCheckedForNull.DiagnosticId, LanguageNames.CSharp)]
    [Rule(RestrictDeserializedTypes.DiagnosticId, LanguageNames.CSharp)]
    public sealed partial class SymbolicExecutionRunner : SonarDiagnosticAnalyzer
    {
        private static readonly ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules = ImmutableDictionary<DiagnosticDescriptor, RuleFactory>.Empty
            .Add(LocksReleasedAllPaths.S2222, CreateFactory<LocksReleasedAllPaths>());
        private readonly SymbolicExecutionAnalyzerFactory analyzerFactory;  // ToDo: This should be eventually removed
        private readonly Dictionary<DiagnosticDescriptor, RuleFactory> additionalTestRules = new();
        private ImmutableArray<DiagnosticDescriptor> supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => supportedDiagnostics;
        protected override bool EnableConcurrentExecution => false;

        public SymbolicExecutionRunner() : this(new SymbolicExecutionAnalyzerFactory()) { }

        private SymbolicExecutionRunner(SymbolicExecutionAnalyzerFactory analyzerFactory)
        {
            this.analyzerFactory = analyzerFactory;
            supportedDiagnostics = analyzerFactory.SupportedDiagnostics.Concat(AllRules.Keys).ToImmutableArray();  // ToDo: This should be eventually moved to the property itself
        }

        internal /* for testing */ void RegisterRule<TRuleCheck>(DiagnosticDescriptor descriptor) where TRuleCheck : SymbolicRuleCheck, new ()
        {
            additionalTestRules.Add(descriptor, CreateFactory<TRuleCheck>());
            supportedDiagnostics = supportedDiagnostics.Concat(new[] { descriptor }).ToImmutableArray();
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => Analyze<BaseMethodDeclarationSyntax>(context, c, x => (CSharpSyntaxNode)x.Body ?? x.ExpressionBody()),
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => Analyze<PropertyDeclarationSyntax>(context, c, x => x.ExpressionBody?.Expression),
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => Analyze<AccessorDeclarationSyntax>(context, c, x => (CSharpSyntaxNode)x.Body ?? x.ExpressionBody()),
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKindEx.InitAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (AnonymousFunctionExpressionSyntax)c.Node;
                    if (c.SemanticModel.GetSymbolInfo(declaration).Symbol is { } symbol)
                    {
                        Analyze(context, c, declaration.Body, symbol);
                    }
                },
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        private void Analyze<TNode>(SonarAnalysisContext analysisContext, SyntaxNodeAnalysisContext context, Func<TNode, CSharpSyntaxNode> getBody) where TNode : SyntaxNode
        {
            if (getBody((TNode)context.Node) is { } body && context.SemanticModel.GetDeclaredSymbol(context.Node) is { } symbol)
            {
                Analyze(analysisContext, context, body, symbol);
            }
        }

        private void Analyze(SonarAnalysisContext sonarContext, SyntaxNodeAnalysisContext nodeContext, CSharpSyntaxNode body, ISymbol symbol)
        {
            if (body != null && !body.ContainsDiagnostics)
            {
                var isTestProject = sonarContext.IsTestProject(nodeContext.Compilation, nodeContext.Options);
                var isScannerRun = sonarContext.IsScannerRun(nodeContext.Options);
                AnalyzeSonar(nodeContext, isTestProject, isScannerRun, body, symbol);
                if (ControlFlowGraph.IsAvailable)   // ToDo: Make this configurable for UTs when migrating other rules, see DeadStores
                {
                    AnalyzeRoslyn(sonarContext, nodeContext, isTestProject, isScannerRun, body, symbol);
                }
            }
        }

        private void AnalyzeRoslyn(SonarAnalysisContext sonarContext, SyntaxNodeAnalysisContext nodeContext, bool isTestProject, bool isScannerRun, CSharpSyntaxNode body, ISymbol symbol)
        {
            var checks = AllRules.Concat(additionalTestRules)
                .Where(x => SymbolicExecutionAnalyzerFactory.IsEnabled(nodeContext, isTestProject, isScannerRun, x.Key))
                .GroupBy(x => x.Value.Type)                             // Multiple DiagnosticDescriptors (S2583, S2589) can share the same check type
                .Select(x => x.First().Value.CreateInstance(sonarContext, nodeContext))   // We need just one instance in that case
                .Where(x => x.ShouldExecute())
                .ToArray();
            if (checks.Any())
            {
                try
                {
                    var cfg = body.CreateCfg(nodeContext.SemanticModel);
                    var engine = new RoslynSymbolicExecution(cfg, checks);
                    engine.Execute();
                }
                catch (Exception ex)
                {
                    throw new SymbolicExecutionException(ex, symbol, body.GetLocation());
                }
            }
        }

        private void AnalyzeSonar(SyntaxNodeAnalysisContext context, bool isTestProject, bool isScannerRun, CSharpSyntaxNode body, ISymbol symbol)
        {
            var enabledAnalyzers = analyzerFactory.GetEnabledAnalyzers(context, isTestProject, isScannerRun);
            if (enabledAnalyzers.Any() && CSharpControlFlowGraph.TryGet(body, context.SemanticModel, out var cfg))
            {
                var lva = new SonarCSharpLiveVariableAnalysis(cfg, symbol, context.SemanticModel);
                try
                {
                    var explodedGraph = new SonarExplodedGraph(cfg, symbol, context.SemanticModel, lva);
                    var analyzerContexts = enabledAnalyzers.Select(x => x.CreateContext(explodedGraph, context)).ToList();
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
                    ReportDiagnosticsSonar(analyzerContexts, context, true);

                    void ExplorationEndedHandlerSonar(object sender, EventArgs args) =>
                        ReportDiagnosticsSonar(analyzerContexts, context, false);
                }
                catch (Exception ex)
                {
                    throw new SymbolicExecutionException(ex, symbol, body.GetLocation());
                }
            }
        }

        private static void ReportDiagnosticsSonar(IEnumerable<ISymbolicExecutionAnalysisContext> analyzerContexts, SyntaxNodeAnalysisContext context, bool supportsPartialResults)
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
}
