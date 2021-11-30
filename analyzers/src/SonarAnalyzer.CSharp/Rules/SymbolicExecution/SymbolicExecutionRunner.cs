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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.SymbolicExecution.Sonar;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.SymbolicExecution
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SymbolicExecutionRunner : SonarDiagnosticAnalyzer
    {
        private readonly SymbolicExecutionAnalyzerFactory symbolicExecutionAnalyzerFactory = new SymbolicExecutionAnalyzerFactory();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        protected override bool EnableConcurrentExecution => false;

        // This constructor is needed by the Roslyn framework. Please do not delete this.
        public SymbolicExecutionRunner() =>
            SupportedDiagnostics = symbolicExecutionAnalyzerFactory.SupportedDiagnostics;

        // Only testing purposes.
        internal SymbolicExecutionRunner(ISymbolicExecutionAnalyzer analyzer)
        {
            symbolicExecutionAnalyzerFactory = new SymbolicExecutionAnalyzerFactory(ImmutableArray.Create(analyzer));
            SupportedDiagnostics = symbolicExecutionAnalyzerFactory.SupportedDiagnostics;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => Analyze<BaseMethodDeclarationSyntax>(c, x => (CSharpSyntaxNode)x.Body ?? x.ExpressionBody()),
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => Analyze<PropertyDeclarationSyntax>(c, x => x.ExpressionBody),
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => Analyze<AccessorDeclarationSyntax>(c, x => (CSharpSyntaxNode)x.Body ?? x.ExpressionBody()),
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
                        Analyze(c, declaration.Body, symbol);
                    }
                },
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        private void Analyze<TNode>(SyntaxNodeAnalysisContext context, Func<TNode, CSharpSyntaxNode> getBody) where TNode : SyntaxNode
        {
            if (getBody((TNode)context.Node) is { } body && context.SemanticModel.GetDeclaredSymbol(context.Node) is { } symbol)
            {
                Analyze(context, body, symbol);
            }
        }

        private void Analyze(SyntaxNodeAnalysisContext context, CSharpSyntaxNode body, ISymbol symbol)
        {
            if (body == null
                || body.ContainsDiagnostics
                || !CSharpControlFlowGraph.TryGet(body, context.SemanticModel, out var cfg))
            {
                return;
            }

            var lva = new SonarCSharpLiveVariableAnalysis(cfg, symbol, context.SemanticModel);
            try
            {
                var explodedGraph = new SonarExplodedGraph(cfg, symbol, context.SemanticModel, lva);
                var analyzerContexts = symbolicExecutionAnalyzerFactory.GetEnabledAnalyzers(context).Select(x => x.CreateContext(explodedGraph, context)).ToList();
                try
                {
                    explodedGraph.ExplorationEnded += ExplorationEndedHandler;
                    explodedGraph.Walk();
                }
                finally
                {
                    explodedGraph.ExplorationEnded -= ExplorationEndedHandler;
                }

                // Some of the rules can return good results if the tree was only partially visited; others need to completely
                // walk the tree in order to avoid false positives.
                //
                // Due to this we split the rules in two sets and report the diagnostics in steps:
                // - When the tree is successfully visited and ExplorationEnded event is raised.
                // - When the tree visit ends (explodedGraph.Walk() returns). This will happen even if the maximum number of steps was
                // reached or if an exception was thrown during analysis.
                ReportDiagnostics(analyzerContexts, context, true);

                void ExplorationEndedHandler(object sender, EventArgs args) =>
                    ReportDiagnostics(analyzerContexts, context, false);
            }
            catch (Exception e)
            {
                // Roslyn/MSBuild is currently cutting exception message at the end of the line instead
                // of displaying the full message. As a workaround, we replace the line ending with ' ## '.
                // See https://github.com/dotnet/roslyn/issues/1455 and https://github.com/dotnet/roslyn/issues/24346
                var sb = new StringBuilder();
                sb.AppendLine($"Error processing method: {symbol?.Name ?? "{unknown}"}");
                sb.AppendLine($"Method file: {body.GetLocation()?.GetLineSpan().Path ?? "{unknown}"}");
                sb.AppendLine($"Method line: {body.GetLocation()?.GetLineSpan().StartLinePosition.ToString() ?? "{unknown}"}");
                sb.AppendLine($"Inner exception: {e}");

                throw new SymbolicExecutionException(sb.ToString().Replace(Environment.NewLine, " ## "), e);
            }
        }

        private static void ReportDiagnostics(IEnumerable<ISymbolicExecutionAnalysisContext> analyzerContexts, SyntaxNodeAnalysisContext context, bool supportsPartialResults)
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
