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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Metrics.CSharp
{
    public sealed class CognitiveComplexityWalker : CognitiveComplexityWalkerBase
    {
        private MethodDeclarationSyntax currentMethod;
        private readonly List<ExpressionSyntax> logicalOperationsToIgnore = new List<ExpressionSyntax>();

        private InnerWalker walker => new InnerWalker(this);

        public override void Visit(SyntaxNode node)
        {
            walker.Visit(node);
        }

        private class InnerWalker : CSharpSyntaxWalker
        {
            private readonly CognitiveComplexityWalker parent;
            public InnerWalker(CognitiveComplexityWalker parent)
            {
                this.parent = parent;
            }

            public override void Visit(SyntaxNode node)
            {
                if (node.IsKind(SyntaxKindEx.LocalFunctionStatement))
                {
                    parent.VisitWithNesting(node, base.Visit);
                }
                else
                {
                    base.Visit(node);
                }
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                parent.currentMethod = node;
                base.VisitMethodDeclaration(node);

                if (parent.hasDirectRecursiveCall)
                {
                    parent.IncreaseComplexity(node.Identifier, 1, "+1 (recursion)");
                }
            }

            public override void VisitIfStatement(IfStatementSyntax node)
            {
                if (node.Parent.IsKind(SyntaxKind.ElseClause))
                {
                    base.VisitIfStatement(node);
                }
                else
                {
                    parent.IncreaseComplexityByNestingPlusOne(node.IfKeyword);
                    parent.VisitWithNesting(node, base.VisitIfStatement);
                }
            }

            public override void VisitElseClause(ElseClauseSyntax node)
            {
                parent.IncreaseComplexityByOne(node.ElseKeyword);
                base.VisitElseClause(node);
            }

            public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.QuestionToken);
                parent.VisitWithNesting(node, base.VisitConditionalExpression);
            }

            public override void VisitSwitchStatement(SwitchStatementSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.SwitchKeyword);
                parent.VisitWithNesting(node, base.VisitSwitchStatement);
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
                parent.IncreaseComplexityByNestingPlusOne(node.ForEachKeyword);
                parent.VisitWithNesting(node, base.VisitForEachStatement);
            }

            public override void VisitCatchClause(CatchClauseSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.CatchKeyword);
                parent.VisitWithNesting(node, base.VisitCatchClause);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var identifierNameSyntax = node.Expression as IdentifierNameSyntax;
                if (parent.currentMethod != null &&
                    identifierNameSyntax != null &&
                    node.HasExactlyNArguments(parent.currentMethod.ParameterList.Parameters.Count) &&
                    string.Equals(identifierNameSyntax.Identifier.ValueText,
                        parent.currentMethod.Identifier.ValueText, StringComparison.Ordinal))
                {
                    parent.hasDirectRecursiveCall = true;
                }

                base.VisitInvocationExpression(node);
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                var nodeKind = node.Kind();
                if (!parent.logicalOperationsToIgnore.Contains(node) &&
                    (nodeKind == SyntaxKind.LogicalAndExpression ||
                     nodeKind == SyntaxKind.LogicalOrExpression))
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

            public override void VisitGotoStatement(GotoStatementSyntax node)
            {
                parent.IncreaseComplexityByNestingPlusOne(node.GotoKeyword);
                base.VisitGotoStatement(node);
            }

            public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            {
                parent.VisitWithNesting(node, base.VisitSimpleLambdaExpression);
            }

            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
            {
                parent.VisitWithNesting(node, base.VisitParenthesizedLambdaExpression);
            }
        }
    }
}
