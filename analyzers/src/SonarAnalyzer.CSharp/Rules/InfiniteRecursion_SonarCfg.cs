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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    public partial class InfiniteRecursion
    {
        public class InfiniteRecursion_SonarCfg : IInfiniteRecursion
        {
            public void CheckForNoExitProperty(SyntaxNodeAnalysisContext c, PropertyDeclarationSyntax property, IPropertySymbol propertySymbol)
            {
                IControlFlowGraph cfg;
                if (property.ExpressionBody?.Expression != null)
                {
                    if (CSharpControlFlowGraph.TryGet(property.ExpressionBody.Expression, c.SemanticModel, out cfg))
                    {
                        var walker = new CfgWalkerForProperty(
                            new RecursionAnalysisContext<IControlFlowGraph>(cfg, propertySymbol, property.Identifier.GetLocation(), c),
                            "property's recursion",
                            isSetAccessor: false);
                        walker.CheckPaths();
                    }

                    return;
                }

                var accessors = property.AccessorList?.Accessors.Where(a => a.HasBodyOrExpressionBody());
                if (accessors != null)
                {
                    foreach (var accessor in accessors)
                    {
                        var bodyNode = (CSharpSyntaxNode)accessor.Body ?? accessor.ExpressionBody();
                        if (CSharpControlFlowGraph.TryGet(bodyNode, c.SemanticModel, out cfg))
                        {
                            var walker = new CfgWalkerForProperty(
                                new RecursionAnalysisContext<IControlFlowGraph>(cfg, propertySymbol, accessor.Keyword.GetLocation(), c),
                                "property accessor's recursion",
                                isSetAccessor: accessor.Keyword.IsKind(SyntaxKind.SetKeyword));
                            walker.CheckPaths();

                            CheckInfiniteJumpLoop(bodyNode, cfg, "property accessor", c);
                        }
                    }
                }
            }

            public void CheckForNoExitMethod(SyntaxNodeAnalysisContext c, CSharpSyntaxNode body, SyntaxToken identifier, ISymbol symbol)
            {
                if (CSharpControlFlowGraph.TryGet(body, c.SemanticModel, out var cfg))
                {
                    var walker = new CfgWalkerForMethod(new RecursionAnalysisContext<IControlFlowGraph>(cfg, symbol, identifier.GetLocation(), c));
                    walker.CheckPaths();
                    CheckInfiniteJumpLoop(body, cfg, "method", c);
                }
            }

            private static void CheckInfiniteJumpLoop(SyntaxNode body, IControlFlowGraph cfg, string declarationType, SyntaxNodeAnalysisContext analysisContext)
            {
                if (body == null)
                {
                    return;
                }

                var reachableFromBlock = cfg.Blocks.Except(new[] { cfg.ExitBlock }).ToDictionary(
                    b => b,
                    b => b.AllSuccessorBlocks);

                var alreadyProcessed = new HashSet<Block>();

                foreach (var reachable in reachableFromBlock)
                {
                    if (!reachable.Key.AllPredecessorBlocks.Contains(cfg.EntryBlock)
                        || alreadyProcessed.Contains(reachable.Key)
                        || reachable.Value.Contains(cfg.ExitBlock))
                    {
                        continue;
                    }

                    alreadyProcessed.UnionWith(reachable.Value);
                    alreadyProcessed.Add(reachable.Key);

                    var reportOnOptions = reachable.Value.OfType<JumpBlock>()
                                                   .Where(jb => jb.JumpNode is GotoStatementSyntax)
                                                   .ToList();

                    if (!reportOnOptions.Any())
                    {
                        continue;
                    }

                    // Calculate stable report location:
                    var lastJumpLocation = reportOnOptions.Max(b => b.JumpNode.SpanStart);
                    var reportOn = reportOnOptions.First(b => b.JumpNode.SpanStart == lastJumpLocation);

                    analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, reportOn.JumpNode.GetLocation(), declarationType));
                }
            }

            private class CfgWalkerForMethod : CfgRecursionSearcher
            {
                public CfgWalkerForMethod(RecursionAnalysisContext<IControlFlowGraph> context)
                    : base(
                        context.ControlFlowGraph,
                        context.AnalyzedSymbol,
                        context.SemanticModel,
                        () => context.AnalysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.IssueLocation, "method's recursion")))
                {
                }

                protected override bool BlockHasReferenceToDeclaringSymbol(Block block) =>
                    block.Instructions.Any(i =>
                    {
                        if (!(i is InvocationExpressionSyntax invocation))
                        {
                            return false;
                        }

                        return IsInstructionOnThisAndMatchesDeclaringSymbol(invocation.Expression, declaringSymbol, semanticModel);
                    });
            }

            private class CfgWalkerForProperty : CfgRecursionSearcher
            {
                private readonly bool isSet;

                public CfgWalkerForProperty(RecursionAnalysisContext<IControlFlowGraph> context, string reportOn, bool isSetAccessor)
                    : base(
                        context.ControlFlowGraph,
                        context.AnalyzedSymbol,
                        context.SemanticModel,
                        () => context.AnalysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.IssueLocation, reportOn))) =>
                    isSet = isSetAccessor;

                private static readonly ISet<Type> TypesForReference = new HashSet<Type> { typeof(IdentifierNameSyntax), typeof(MemberAccessExpressionSyntax) };

                protected override bool BlockHasReferenceToDeclaringSymbol(Block block) =>
                    block.Instructions.Any(i =>
                        TypesForReference.Contains(i.GetType())
                        && MatchesAccessor(i)
                        && IsInstructionOnThisAndMatchesDeclaringSymbol(i, declaringSymbol, semanticModel));

                private bool MatchesAccessor(SyntaxNode node)
                {
                    var expr = (ExpressionSyntax)node;
                    var propertyAccess = expr.GetSelfOrTopParenthesizedExpression();
                    var isNodeASet = propertyAccess.Parent is AssignmentExpressionSyntax assignment && assignment.Left == propertyAccess;
                    return isNodeASet == isSet;
                }
            }

            private abstract class CfgRecursionSearcher : CfgAllPathValidator
            {
                protected readonly SemanticModel semanticModel;
                protected readonly ISymbol declaringSymbol;
                private readonly Action reportIssue;

                protected CfgRecursionSearcher(IControlFlowGraph cfg, ISymbol declaringSymbol, SemanticModel semanticModel, Action reportIssue)
                    : base(cfg)
                {
                    this.declaringSymbol = declaringSymbol;
                    this.semanticModel = semanticModel;
                    this.reportIssue = reportIssue;
                }

                public void CheckPaths()
                {
                    if (CheckAllPaths())
                    {
                        reportIssue();
                    }
                }

                protected override bool IsBlockValid(Block block) =>
                    BlockHasReferenceToDeclaringSymbol(block);

                protected abstract bool BlockHasReferenceToDeclaringSymbol(Block block);
            }
        }
    }
}
