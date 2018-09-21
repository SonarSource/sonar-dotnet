/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Common.VisualBasic
{
    public sealed class CognitiveComplexityWalker : CognitiveComplexityWalkerBase
    {
        private MethodStatementSyntax currentMethod;
        private readonly List<ExpressionSyntax> logicalOperationsToIgnore = new List<ExpressionSyntax>();

        private InnerWalker walker => new InnerWalker(this);

        public override void Visit(SyntaxNode node)
        {
            walker.Visit(node);
        }

        private class InnerWalker : VisualBasicSyntaxWalker
        {
            private readonly CognitiveComplexityWalker parent;
            public InnerWalker(CognitiveComplexityWalker parent)
            {
                this.parent = parent;
            }

            public override void VisitMethodBlock(MethodBlockSyntax node)
            {
                parent.currentMethod = node.SubOrFunctionStatement;
                base.VisitMethodBlock(node);

                if (parent.hasDirectRecursiveCall)
                {
                    parent.IncreaseComplexity(node.SubOrFunctionStatement.Identifier, 1, "+1 (recursion)");
                }
            }

            public override void VisitSingleLineIfStatement(SingleLineIfStatementSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.IfKeyword);
                parent.VisitWithNesting(node, base.VisitSingleLineIfStatement);
            }

            public override void VisitMultiLineIfBlock(MultiLineIfBlockSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.IfStatement.IfKeyword);
                parent.VisitWithNesting(node, base.VisitMultiLineIfBlock);
            }

            public override void VisitElseIfStatement(ElseIfStatementSyntax node)
            {
                parent.IncreaseComplexityByOne(node.ElseIfKeyword);
                base.VisitElseIfStatement(node);
            }

            public override void VisitElseStatement(ElseStatementSyntax node)
            {
                parent.IncreaseComplexityByOne(node.ElseKeyword);
                base.VisitElseStatement(node);
            }

            public override void VisitBinaryConditionalExpression(BinaryConditionalExpressionSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.IfKeyword);
                parent.VisitWithNesting(node, base.VisitBinaryConditionalExpression);
            }

            public override void VisitTernaryConditionalExpression(TernaryConditionalExpressionSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.IfKeyword);
                parent.VisitWithNesting(node, base.VisitTernaryConditionalExpression);
            }

            public override void VisitSelectBlock(SelectBlockSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.SelectStatement.SelectKeyword);
                parent.VisitWithNesting(node, base.VisitSelectBlock);
            }

            public override void VisitForStatement(ForStatementSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.ForKeyword);
                parent.VisitWithNesting(node, base.VisitForStatement);
            }

            public override void VisitWhileStatement(WhileStatementSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.WhileKeyword);
                parent.VisitWithNesting(node, base.VisitWhileStatement);
            }

            public override void VisitDoStatement(DoStatementSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.DoKeyword);
                parent.VisitWithNesting(node, base.VisitDoStatement);
            }

            public override void VisitForEachStatement(ForEachStatementSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.ForKeyword);
                parent.VisitWithNesting(node, base.VisitForEachStatement);
            }

            public override void VisitCatchBlock(CatchBlockSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.CatchStatement.CatchKeyword);
                parent.VisitWithNesting(node, base.VisitCatchBlock);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression == null ||
                    node.ArgumentList == null ||
                    parent.currentMethod == null ||
                    node.ArgumentList.Arguments.Count != parent.currentMethod.ParameterList.Parameters.Count)
                {
                    return;
                }
                parent.hasDirectRecursiveCall = string.Equals(GetIdentifierName(node.Expression),
                    parent.currentMethod.Identifier.ValueText,
                    StringComparison.Ordinal);
                base.VisitInvocationExpression(node);

                string GetIdentifierName(ExpressionSyntax expression)
                {
                    if (expression.IsKind(SyntaxKind.IdentifierName))
                    {
                        return (expression as IdentifierNameSyntax).Identifier.ValueText;
                    }
                    else if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        return (expression as MemberAccessExpressionSyntax).Name.Identifier.ValueText;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                var nodeKind = node.Kind();
                if (!parent.logicalOperationsToIgnore.Contains(node) &&
                    (nodeKind == SyntaxKind.AndExpression ||
                     nodeKind == SyntaxKind.AndAlsoExpression ||
                     nodeKind == SyntaxKind.OrExpression ||
                     nodeKind == SyntaxKind.OrElseExpression))
                {
                    var left = node.Left.RemoveParentheses();
                    if (!left.IsKind(nodeKind))
                    {
                        parent.IncreaseComplexityByOne(node.OperatorToken);
                    }

                    var right = node.Right.RemoveParentheses();
                    if (right.IsKind(nodeKind))
                    {
                        parent.logicalOperationsToIgnore.Add(right);
                    }
                }

                base.VisitBinaryExpression(node);
            }

            public override void VisitGoToStatement(GoToStatementSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.GoToKeyword);
                base.VisitGoToStatement(node);
            }

            public override void VisitSingleLineLambdaExpression(SingleLineLambdaExpressionSyntax node)
            {
                parent.VisitWithNesting(node, base.VisitSingleLineLambdaExpression);
            }

            public override void VisitMultiLineLambdaExpression(MultiLineLambdaExpressionSyntax node)
            {
                parent.VisitWithNesting(node, base.VisitMultiLineLambdaExpression);
            }
        }
    }
}
