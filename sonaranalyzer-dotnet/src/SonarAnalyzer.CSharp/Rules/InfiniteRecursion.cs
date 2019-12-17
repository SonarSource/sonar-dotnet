/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class InfiniteRecursion : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2190";
        private const string MessageFormat = "Add a way to break out of this {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var method = (MethodDeclarationSyntax)c.Node;
                    CheckForNoExitMethod(c, (CSharpSyntaxNode)method.Body ?? method.ExpressionBody, method.Identifier);
                },
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var function = (LocalFunctionStatementSyntaxWrapper)c.Node;
                    CheckForNoExitMethod(c, (CSharpSyntaxNode)function.Body ?? function.ExpressionBody, function.Identifier);
                },
                SyntaxKindEx.LocalFunctionStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckForNoExitProperty,
                SyntaxKind.PropertyDeclaration);
        }

        private static void CheckForNoExitProperty(SyntaxNodeAnalysisContext c)
        {
            var property = (PropertyDeclarationSyntax)c.Node;
            var propertySymbol = c.SemanticModel.GetDeclaredSymbol(property);
            if (propertySymbol == null)
            {
                return;
            }

            IControlFlowGraph cfg;
            if (property.ExpressionBody?.Expression != null)
            {
                if (CSharpControlFlowGraph.TryGet(property.ExpressionBody.Expression, c.SemanticModel, out cfg))
                {
                    var walker = new CfgWalkerForProperty(
                         new RecursionAnalysisContext(cfg, propertySymbol, property.Identifier.GetLocation(), c),
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
                            new RecursionAnalysisContext(cfg, propertySymbol, accessor.Keyword.GetLocation(), c),
                            "property accessor's recursion",
                            isSetAccessor: accessor.Keyword.IsKind(SyntaxKind.SetKeyword));
                        walker.CheckPaths();

                        CheckInfiniteJumpLoop(bodyNode, cfg, "property accessor", c);
                    }
                }
            }
        }

        private static void CheckForNoExitMethod(SyntaxNodeAnalysisContext c, CSharpSyntaxNode body, SyntaxToken identifier)
        {
            var symbol = c.SemanticModel.GetDeclaredSymbol(c.Node);
            if (symbol != null && body != null && CSharpControlFlowGraph.TryGet(body, c.SemanticModel, out var cfg))
            {
                var walker = new CfgWalkerForMethod(new RecursionAnalysisContext(cfg, symbol, identifier.GetLocation(), c));
                walker.CheckPaths();
                CheckInfiniteJumpLoop(body, cfg, "method", c);
            }
        }

        private static void CheckInfiniteJumpLoop(SyntaxNode body, IControlFlowGraph cfg, string declarationType,
            SyntaxNodeAnalysisContext analysisContext)
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
                if (!reachable.Key.AllPredecessorBlocks.Contains(cfg.EntryBlock) ||
                    alreadyProcessed.Contains(reachable.Key) ||
                    reachable.Value.Contains(cfg.ExitBlock))
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

                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, reportOn.JumpNode.GetLocation(), declarationType));
            }
        }

        #region CFG walkers for call recursion

        private class RecursionAnalysisContext
        {
            public IControlFlowGraph ControlFlowGraph { get; }
            public ISymbol AnalyzedSymbol { get; }
            public SemanticModel SemanticModel { get; }
            public Location IssueLocation { get; }
            public SyntaxNodeAnalysisContext AnalysisContext { get; }

            public RecursionAnalysisContext(IControlFlowGraph controlFlowGraph, ISymbol analyzedSymbol, Location issueLocation,
                SyntaxNodeAnalysisContext analysisContext)
            {
                ControlFlowGraph = controlFlowGraph;
                AnalyzedSymbol = analyzedSymbol;
                IssueLocation = issueLocation;
                AnalysisContext = analysisContext;

                SemanticModel = analysisContext.SemanticModel;
            }
        }

        private class CfgWalkerForMethod : CfgRecursionSearcher
        {
            public CfgWalkerForMethod(RecursionAnalysisContext context)
                : base(context.ControlFlowGraph, context.AnalyzedSymbol, context.SemanticModel,
                      () => context.AnalysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, context.IssueLocation, "method's recursion")))
            {
            }

            protected override bool BlockHasReferenceToDeclaringSymbol(Block block)
            {
                return block.Instructions.Any(i =>
                {
                    if (!(i is InvocationExpressionSyntax invocation))
                    {
                        return false;
                    }

                    return IsInstructionOnThisAndMatchesDeclaringSymbol(invocation.Expression);
                });
            }
        }

        private class CfgWalkerForProperty : CfgRecursionSearcher
        {
            private readonly bool isSet;

            public CfgWalkerForProperty(RecursionAnalysisContext context, string reportOn, bool isSetAccessor)
                : base(context.ControlFlowGraph, context.AnalyzedSymbol, context.SemanticModel,
                      () => context.AnalysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, context.IssueLocation, reportOn)))
            {
                this.isSet = isSetAccessor;
            }

            private static readonly ISet<Type> TypesForReference = new HashSet<Type> { typeof(IdentifierNameSyntax), typeof(MemberAccessExpressionSyntax) };

            protected override bool BlockHasReferenceToDeclaringSymbol(Block block)
            {
                return block.Instructions.Any(i =>
                    TypesForReference.Contains(i.GetType()) &&
                    MatchesAccessor(i) &&
                    IsInstructionOnThisAndMatchesDeclaringSymbol(i));
            }

            private bool MatchesAccessor(SyntaxNode node)
            {
                if (!(node is ExpressionSyntax expr))
                {
                    return false;
                }

                var propertyAccess = expr.GetSelfOrTopParenthesizedExpression();
                if (propertyAccess.IsInNameofCall(this.semanticModel))
                {
                    return false;
                }

                var isNodeASet = propertyAccess.Parent is AssignmentExpressionSyntax assignment && assignment.Left == propertyAccess;
                return isNodeASet == this.isSet;
            }
        }

        private abstract class CfgRecursionSearcher : CfgAllPathValidator
        {
            protected readonly ISymbol declaringSymbol;
            protected readonly SemanticModel semanticModel;
            protected readonly Action reportIssue;

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
                    this.reportIssue();
                }
            }

            protected override bool IsBlockValid(Block block)
            {
                return BlockHasReferenceToDeclaringSymbol(block);
            }

            protected abstract bool BlockHasReferenceToDeclaringSymbol(Block block);

            protected bool IsInstructionOnThisAndMatchesDeclaringSymbol(SyntaxNode node)
            {
                if (!(node is ExpressionSyntax expression))
                {
                    return false;
                }

                var name = expression as NameSyntax;

                if (expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                {
                    name = memberAccess.Name as IdentifierNameSyntax;
                }

                if (expression is ConditionalAccessExpressionSyntax conditionalAccess &&
                    conditionalAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                {
                    name = (conditionalAccess.WhenNotNull as MemberBindingExpressionSyntax)?.Name as IdentifierNameSyntax;
                }

                if (name == null)
                {
                    return false;
                }

                var assignedSymbol = this.semanticModel.GetSymbolInfo(name).Symbol;

                return this.declaringSymbol.Equals(assignedSymbol);
            }
        }

        #endregion
    }
}
