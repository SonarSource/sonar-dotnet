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
                    foreach (var node in VerifyEmptyBlocks(c.Node))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation()));
                    }
                },
                SyntaxKind.SimpleDoLoopBlock,
                SyntaxKind.DoLoopUntilBlock,
                SyntaxKind.DoLoopWhileBlock,
                SyntaxKind.DoUntilLoopBlock,
                SyntaxKind.DoWhileLoopBlock,
                SyntaxKind.ForBlock,
                SyntaxKind.ForEachBlock,
                // The Else and ElseIf blocks are inside the MultiLineIfBlock
                SyntaxKind.MultiLineIfBlock,
                SyntaxKind.SelectBlock,
                // The CatchBlock and FinallyBlock are inside the TryBlock
                SyntaxKind.TryBlock,
                SyntaxKind.UsingBlock,
                SyntaxKind.WhileBlock,
                SyntaxKind.WithBlock);
        }

        /**
         * Verify that the given block has no statements and no comments inside
         * (notable exception: the Select block)
         *
         * Note: Roslyn maps the comments which are inside a block as trivia of the following block
         * e.g. in the below snippet, the comment will be part of the Finally block
         *
         * Try
         *   ' my comment
         * Finally
         * End Try
         */

        private IEnumerable<SyntaxNode> VerifyEmptyBlocks(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.SimpleDoLoopBlock:
                case SyntaxKind.DoLoopUntilBlock:
                case SyntaxKind.DoLoopWhileBlock:
                case SyntaxKind.DoUntilLoopBlock:
                case SyntaxKind.DoWhileLoopBlock:
                    return VisitDoLoopBlock((DoLoopBlockSyntax)node);

                case SyntaxKind.ForBlock:
                    return VisitForBlock((ForBlockSyntax)node);

                case SyntaxKind.ForEachBlock:
                    return VisitForEachBlock((ForEachBlockSyntax)node);

                case SyntaxKind.MultiLineIfBlock:
                    return VisitMultiLineIfBlock((MultiLineIfBlockSyntax)node);

                case SyntaxKind.SelectBlock:
                    return VisitSelectBlock((SelectBlockSyntax)node);

                case SyntaxKind.TryBlock:
                    return VisitTryBlock((TryBlockSyntax)node);

                case SyntaxKind.UsingBlock:
                    return VisitUsingBlock((UsingBlockSyntax)node);

                case SyntaxKind.WhileBlock:
                    return VisitWhileBlock((WhileBlockSyntax)node);

                case SyntaxKind.WithBlock:
                    return VisitWithBlock((WithBlockSyntax)node);

                default:
                    // we do not throw an exception as the language can evolve over time
                    return Enumerable.Empty<SyntaxNode>();
            }
        }

        private IEnumerable<SyntaxNode> VisitDoLoopBlock(DoLoopBlockSyntax node)
        {
            if (!node.Statements.Any() && NoCommentsBefore(node.LoopStatement))
            {
                yield return node.DoStatement;
            }
        }

        private IEnumerable<SyntaxNode> VisitForBlock(ForBlockSyntax node)
        {
            if (!node.Statements.Any() && NoCommentsBefore(node.NextStatement))
            {
                yield return node.ForStatement;
            }
        }

        private IEnumerable<SyntaxNode> VisitForEachBlock(ForEachBlockSyntax node)
        {
            if (!node.Statements.Any() && NoCommentsBefore(node.NextStatement))
            {
                yield return node.ForEachStatement;
            }
        }

        private IEnumerable<SyntaxNode> VisitMultiLineIfBlock(MultiLineIfBlockSyntax node)
        {
            var result = new List<SyntaxNode>();
            if (node.ElseBlock == null)
            {
                if (node.ElseIfBlocks.Any())
                {
                    result.AddRange(VerifyIfAndMostElseIfBlocks(node));
                    result.AddRange(VerifyElseIfBlock(node.ElseIfBlocks[node.ElseIfBlocks.Count - 1], node.EndIfStatement));
                }
                else
                {
                    result.AddRange(VerifyIfBlock(node, node.EndIfStatement));
                }
            }
            else
            {
                if (node.ElseIfBlocks.Any())
                {
                    result.AddRange(VerifyIfAndMostElseIfBlocks(node));
                    result.AddRange(VerifyElseIfBlock(node.ElseIfBlocks[node.ElseIfBlocks.Count - 1], node.ElseBlock));
                    result.AddRange(VerifyElseBlock(node.ElseBlock, node.EndIfStatement));
                }
                else
                {
                    result.AddRange(VerifyIfBlock(node, node.ElseBlock));
                    result.AddRange(VerifyElseBlock(node.ElseBlock, node.EndIfStatement));
                }
            }
            return result;
        }

        private IEnumerable<SyntaxNode> VisitSelectBlock(SelectBlockSyntax node)
        {
            if (!node.CaseBlocks.Any())
            {
                yield return node.SelectStatement;
            }
        }

        private IEnumerable<SyntaxNode> VisitTryBlock(TryBlockSyntax node)
        {
            var result = new List<SyntaxNode>();
            if (node.CatchBlocks.Any() && node.FinallyBlock != null)
            {
                result.AddRange(VerifyTryAndMostCatches(node));
                result.AddRange(VerifyCatchBlock(node.CatchBlocks[node.CatchBlocks.Count - 1], node.FinallyBlock));
                result.AddRange(VerifyFinallyBlock(node.FinallyBlock, node.EndTryStatement));
            }
            else if (node.FinallyBlock != null)
            {
                result.AddRange(VerifyTryBlock(node, node.FinallyBlock));
                result.AddRange(VerifyFinallyBlock(node.FinallyBlock, node.EndTryStatement));
            }
            else if (node.CatchBlocks.Any())
            {
                result.AddRange(VerifyTryAndMostCatches(node));
                result.AddRange(VerifyCatchBlock(node.CatchBlocks[node.CatchBlocks.Count - 1], node.EndTryStatement));
            }
            else
            {
                throw new InvalidOperationException("Try block must be followed by at least one catch or one finally block");
            }
            return result;
        }

        private IEnumerable<SyntaxNode> VisitUsingBlock(UsingBlockSyntax node)
        {
            if (!node.Statements.Any() && NoCommentsBefore(node.EndUsingStatement))
            {
                yield return node.UsingStatement;
            }
        }

        private IEnumerable<SyntaxNode> VisitWhileBlock(WhileBlockSyntax node)
        {
            if (!node.Statements.Any() && NoCommentsBefore(node.EndWhileStatement))
            {
                yield return node.WhileStatement;
            }
        }

        private IEnumerable<SyntaxNode> VisitWithBlock(WithBlockSyntax node)
        {
            if (!node.Statements.Any() && NoCommentsBefore(node.EndWithStatement))
            {
                yield return node.WithStatement;
            }
        }

        private IEnumerable<SyntaxNode> VerifyIfAndMostElseIfBlocks(MultiLineIfBlockSyntax ifBlock)
        {
            var result = new List<SyntaxNode>();
            result.AddRange(VerifyIfBlock(ifBlock, ifBlock.ElseIfBlocks[0]));
            // verify all ElseIf except the last one
            for (int i = 0; i < ifBlock.ElseIfBlocks.Count - 1; i++)
            {
                result.AddRange(VerifyElseIfBlock(ifBlock.ElseIfBlocks[i], ifBlock.ElseIfBlocks[i + 1]));
            }
            return result;
        }

        private IEnumerable<SyntaxNode> VerifyIfBlock(MultiLineIfBlockSyntax ifBlock, SyntaxNode node)
        {
            if (!ifBlock.Statements.Any() && NoCommentsBefore(node))
            {
                yield return ifBlock.IfStatement;
            }
        }

        private IEnumerable<SyntaxNode> VerifyElseIfBlock(ElseIfBlockSyntax elseIfBlock, SyntaxNode node)
        {
            if (!elseIfBlock.Statements.Any() && NoCommentsBefore(node))
            {
                yield return elseIfBlock.ElseIfStatement;
            }
        }

        private IEnumerable<SyntaxNode> VerifyElseBlock(ElseBlockSyntax elseBlock, SyntaxNode node)
        {
            if (!elseBlock.Statements.Any() && NoCommentsBefore(node))
            {
                yield return elseBlock.ElseStatement;
            }
        }

        private IEnumerable<SyntaxNode> VerifyTryAndMostCatches(TryBlockSyntax node)
        {
            var result = new List<SyntaxNode>();
            result.AddRange(VerifyTryBlock(node, node.CatchBlocks[0]));
            // verify all catches except the last one
            for (int i = 0; i < node.CatchBlocks.Count - 1; i++)
            {
                result.AddRange(VerifyCatchBlock(node.CatchBlocks[i], node.CatchBlocks[i + 1]));
            }
            return result;
        }

        private IEnumerable<SyntaxNode> VerifyTryBlock(TryBlockSyntax node, SyntaxNode nextBlock)
        {
            if (!node.Statements.Any() && NoCommentsBefore(nextBlock))
            {
                yield return node.TryStatement;
            }
        }

        private IEnumerable<SyntaxNode> VerifyCatchBlock(CatchBlockSyntax node, SyntaxNode nextBlock)
        {
            if (!node.Statements.Any() && NoCommentsBefore(nextBlock))
            {
                yield return node.CatchStatement;
            }
        }

        private IEnumerable<SyntaxNode> VerifyFinallyBlock(FinallyBlockSyntax node, SyntaxNode nextBlock)
        {
            if (!node.Statements.Any() && NoCommentsBefore(nextBlock))
            {
                yield return node.FinallyStatement;
            }
        }

        private static bool NoCommentsBefore(SyntaxNode node) => !node.GetLeadingTrivia().Any(t => t.IsComment());
    }
}
