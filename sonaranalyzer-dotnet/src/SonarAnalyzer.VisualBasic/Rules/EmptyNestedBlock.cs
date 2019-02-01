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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class EmptyNestedBlock : EmptyNestedBlockBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var walker = new BlockWalker();
                    walker.SafeVisit(c.Node);

                    foreach (var node in walker.EmptySyntaxNodes)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation()));
                    }
                },
                // All DoLoop blocks are handled together
                SyntaxKind.SimpleDoLoopBlock,
                SyntaxKind.DoLoopUntilBlock,
                SyntaxKind.DoLoopWhileBlock,
                SyntaxKind.DoUntilLoopBlock,
                SyntaxKind.DoWhileLoopBlock,

                SyntaxKind.ForBlock,
                SyntaxKind.ForEachBlock,
                // The Else and ElseIf blocks are handled by the MultiLineIfBlock
                SyntaxKind.MultiLineIfBlock,
                SyntaxKind.SelectBlock,
                // The CatchBlock and FinallyBlock are handled by the TryBlock
                SyntaxKind.TryBlock,
                SyntaxKind.UsingBlock,
                SyntaxKind.WhileBlock,
                SyntaxKind.WithBlock);
        }

        /**
         * The BlockWalker verifies that the given block has no statements and no comments inside
         * (notable exception: the Select block)
         *
         * Note: Roslyn maps the comments which are inside a block as trivia of the following block
         * e.g. in the below snippet, the comment will be part of the Finally block
         *
         * Try
         *   ' my comment
         * Finally
         */

        private class BlockWalker : VisualBasicSyntaxWalker
        {
            public IEnumerable<SyntaxNode> EmptySyntaxNodes => emptyInnerBlocks;

            private readonly IList<SyntaxNode> emptyInnerBlocks = new List<SyntaxNode>();

            // This handles SimpleDoLoopBlock, DoLoop*Block, Do*LoopBlock
            public override void VisitDoLoopBlock(DoLoopBlockSyntax node)
            {
                if (!node.Statements.Any() && NoCommentsBefore(node.LoopStatement))
                {
                    emptyInnerBlocks.Add(node.DoStatement);
                }
            }

            public override void VisitForBlock(ForBlockSyntax node)
            {
                if (!node.Statements.Any() && NoCommentsBefore(node.NextStatement))
                {
                    emptyInnerBlocks.Add(node.ForStatement);
                }
            }

            public override void VisitForEachBlock(ForEachBlockSyntax node)
            {
                if (!node.Statements.Any() && NoCommentsBefore(node.NextStatement))
                {
                    emptyInnerBlocks.Add(node.ForEachStatement);
                }
            }

            public override void VisitMultiLineIfBlock(MultiLineIfBlockSyntax node)
            {
                if (node.ElseBlock == null)
                {
                    if (node.ElseIfBlocks.Any())
                    {
                        VerifyIfAndMostElseIfBlocks(node);
                        VerifyElseIfBlock(node.ElseIfBlocks[node.ElseIfBlocks.Count - 1], node.EndIfStatement);
                    }
                    else
                    {
                        VerifyIfBlock(node, node.EndIfStatement);
                    }
                }
                else
                {
                    if (node.ElseIfBlocks.Any())
                    {
                        VerifyIfAndMostElseIfBlocks(node);
                        VerifyElseIfBlock(node.ElseIfBlocks[node.ElseIfBlocks.Count - 1], node.ElseBlock);
                        VerifyElseBlock(node.ElseBlock, node.EndIfStatement);
                    }
                    else
                    {
                        VerifyIfBlock(node, node.ElseBlock);
                        VerifyElseBlock(node.ElseBlock, node.EndIfStatement);
                    }
                }
            }

            private void VerifyIfAndMostElseIfBlocks(MultiLineIfBlockSyntax ifBlock)
            {
                VerifyIfBlock(ifBlock, ifBlock.ElseIfBlocks[0]);
                // verify all ElseIf except the last one
                for (int i = 0; i < ifBlock.ElseIfBlocks.Count - 1; i++)
                {
                    VerifyElseIfBlock(ifBlock.ElseIfBlocks[i], ifBlock.ElseIfBlocks[i + 1]);
                }
            }

            private void VerifyIfBlock(MultiLineIfBlockSyntax ifBlock, SyntaxNode node)
            {
                if (!ifBlock.Statements.Any() && NoCommentsBefore(node))
                {
                    emptyInnerBlocks.Add(ifBlock.IfStatement);
                }
            }

            private void VerifyElseIfBlock(ElseIfBlockSyntax elseIfBlock, SyntaxNode node)
            {
                if (!elseIfBlock.Statements.Any() && NoCommentsBefore(node))
                {
                    emptyInnerBlocks.Add(elseIfBlock.ElseIfStatement);
                }
            }

            private void VerifyElseBlock(ElseBlockSyntax elseBlock, SyntaxNode node)
            {
                if (!elseBlock.Statements.Any() && NoCommentsBefore(node))
                {
                    emptyInnerBlocks.Add(elseBlock.ElseStatement);
                }
            }

            public override void VisitSelectBlock(SelectBlockSyntax node)
            {
                if (!node.CaseBlocks.Any())
                {
                    emptyInnerBlocks.Add(node.SelectStatement);
                }
            }

            public override void VisitTryBlock(TryBlockSyntax node)
            {
                if (node.CatchBlocks.Any() && node.FinallyBlock != null)
                {
                    VerifyTryAndMostCatches(node);
                    VerifyCatchBlock(node.CatchBlocks[node.CatchBlocks.Count - 1], node.FinallyBlock);
                    VerifyFinallyBlock(node.FinallyBlock, node.EndTryStatement);
                }
                else if (node.FinallyBlock != null)
                {
                    VerifyTryBlock(node, node.FinallyBlock);
                    VerifyFinallyBlock(node.FinallyBlock, node.EndTryStatement);
                }
                else if (node.CatchBlocks.Any())
                {
                    VerifyTryAndMostCatches(node);
                    VerifyCatchBlock(node.CatchBlocks[node.CatchBlocks.Count - 1], node.EndTryStatement);
                }
                else
                {
                    throw new InvalidOperationException("Try block must be followed by at least one catch or one finally block");
                }
            }

            public override void VisitUsingBlock(UsingBlockSyntax node)
            {
                if (!node.Statements.Any() && NoCommentsBefore(node.EndUsingStatement))
                {
                    emptyInnerBlocks.Add(node.UsingStatement);
                }
            }

            public override void VisitWhileBlock(WhileBlockSyntax node)
            {
                if (!node.Statements.Any() && NoCommentsBefore(node.EndWhileStatement))
                {
                    emptyInnerBlocks.Add(node.WhileStatement);
                }
            }

            public override void VisitWithBlock(WithBlockSyntax node)
            {
                if (!node.Statements.Any() && NoCommentsBefore(node.EndWithStatement))
                {
                    emptyInnerBlocks.Add(node.WithStatement);
                }
            }

            private void VerifyTryAndMostCatches(TryBlockSyntax node)
            {
                VerifyTryBlock(node, node.CatchBlocks[0]);
                // verify all catches except the last one
                for (int i = 0; i < node.CatchBlocks.Count - 1; i++)
                {
                    VerifyCatchBlock(node.CatchBlocks[i], node.CatchBlocks[i + 1]);
                }
            }

            private void VerifyTryBlock(TryBlockSyntax node, SyntaxNode nextBlock)
            {
                if (!node.Statements.Any() && NoCommentsBefore(nextBlock))
                {
                    emptyInnerBlocks.Add(node.TryStatement);
                }
            }

            private void VerifyCatchBlock(CatchBlockSyntax node, SyntaxNode nextBlock)
            {
                if (!node.Statements.Any() && NoCommentsBefore(nextBlock))
                {
                    emptyInnerBlocks.Add(node.CatchStatement);
                }
            }

            private void VerifyFinallyBlock(FinallyBlockSyntax node, SyntaxNode nextBlock)
            {
                if (!node.Statements.Any() && NoCommentsBefore(nextBlock))
                {
                    emptyInnerBlocks.Add(node.FinallyStatement);
                }
            }

            private static bool NoCommentsBefore(SyntaxNode node) => !node.GetLeadingTrivia().Any(t => t.IsComment());
        }
    }
}
