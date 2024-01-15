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

using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.Rules.CSharp
{
    public partial class InfiniteRecursion
    {
        public class SonarChecker : IChecker
        {
            public void CheckForNoExitProperty(SonarSyntaxNodeReportingContext c, PropertyDeclarationSyntax property, IPropertySymbol propertySymbol)
            {
                IControlFlowGraph cfg;
                if (property.ExpressionBody?.Expression != null)
                {
                    if (CSharpControlFlowGraph.TryGet(property, c.SemanticModel, out cfg))
                    {
                        var walker = new RecursionSearcherForProperty(
                            new RecursionContext<IControlFlowGraph>(c, cfg, propertySymbol, property.Identifier.GetLocation(), "property's recursion"),
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
                        if (CSharpControlFlowGraph.TryGet(accessor, c.SemanticModel, out cfg))
                        {
                            var walker = new RecursionSearcherForProperty(
                                new RecursionContext<IControlFlowGraph>(c, cfg, propertySymbol, accessor.Keyword.GetLocation(), "property accessor's recursion"),
                                isSetAccessor: accessor.Keyword.IsKind(SyntaxKind.SetKeyword));
                            walker.CheckPaths();

                            CheckInfiniteJumpLoop(c, accessor, cfg, "property accessor");
                        }
                    }
                }
            }

            public void CheckForNoExitIndexer(SonarSyntaxNodeReportingContext c, IndexerDeclarationSyntax indexer, IPropertySymbol propertySymbol)
            {
                // SonarCFG is out of support
            }

            public void CheckForNoExitEvent(SonarSyntaxNodeReportingContext c, EventDeclarationSyntax eventDeclaration, IEventSymbol eventSymbol)
            {
                // SonarCFG is out of support
            }

            public void CheckForNoExitMethod(SonarSyntaxNodeReportingContext c, SyntaxNode body, SyntaxToken identifier, IMethodSymbol symbol)
            {
                if (CSharpControlFlowGraph.TryGet(body, c.SemanticModel, out var cfg))
                {
                    var walker = new RecursionSearcherForMethod(new RecursionContext<IControlFlowGraph>(c, cfg, symbol, identifier.GetLocation(), "method's recursion"));
                    walker.CheckPaths();
                    CheckInfiniteJumpLoop(c, body, cfg, "method");
                }
            }

            private static void CheckInfiniteJumpLoop(SonarSyntaxNodeReportingContext context, SyntaxNode body, IControlFlowGraph cfg, string declarationType)
            {
                if (body is null)
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

                    context.ReportIssue(Diagnostic.Create(Rule, reportOn.JumpNode.GetLocation(), declarationType));
                }
            }

            private class RecursionSearcherForMethod : RecursionSearcher
            {
                public RecursionSearcherForMethod(RecursionContext<IControlFlowGraph> context)
                    : base(context)
                {
                }

                protected override bool HasReferenceToDeclaringSymbol(Block block) =>
                    block.Instructions.Any(x =>
                        x is InvocationExpressionSyntax invocation
                        && IsInstructionOnThisAndMatchesDeclaringSymbol(invocation.Expression, context.AnalyzedSymbol, context.SemanticModel));
            }

            private class RecursionSearcherForProperty : RecursionSearcher
            {
                private readonly bool isSet;

                public RecursionSearcherForProperty(RecursionContext<IControlFlowGraph> context, bool isSetAccessor)
                    : base(context) =>
                    isSet = isSetAccessor;

                private static readonly ISet<Type> TypesForReference = new HashSet<Type> { typeof(IdentifierNameSyntax), typeof(MemberAccessExpressionSyntax) };

                protected override bool HasReferenceToDeclaringSymbol(Block block) =>
                    block.Instructions.Any(x =>
                        TypesForReference.Contains(x.GetType())
                        && MatchesAccessor(x)
                        && IsInstructionOnThisAndMatchesDeclaringSymbol(x, context.AnalyzedSymbol, context.SemanticModel));

                private bool MatchesAccessor(SyntaxNode node)
                {
                    var propertyAccess = ((ExpressionSyntax)node).GetSelfOrTopParenthesizedExpression();
                    var isNodeASet = propertyAccess.Parent is AssignmentExpressionSyntax assignment && assignment.Left == propertyAccess;
                    return isNodeASet == isSet;
                }
            }

            private abstract class RecursionSearcher : CfgAllPathValidator
            {
                protected readonly RecursionContext<IControlFlowGraph> context;

                protected abstract bool HasReferenceToDeclaringSymbol(Block block);

                protected RecursionSearcher(RecursionContext<IControlFlowGraph> context)
                    : base(context.ControlFlowGraph) =>
                    this.context = context;

                public void CheckPaths()
                {
                    if (CheckAllPaths())
                    {
                        context.ReportIssue();
                    }
                }

                protected override bool IsBlockValid(Block block) =>
                    HasReferenceToDeclaringSymbol(block);
            }
        }
    }
}
