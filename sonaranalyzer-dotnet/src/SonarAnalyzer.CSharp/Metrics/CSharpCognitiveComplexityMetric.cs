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
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Metrics.CSharp
{
    public static class CSharpCognitiveComplexityMetric
    {
        public static CognitiveComplexity GetComplexity(SyntaxNode node)
        {
            var walker = new CognitiveWalker();
            walker.SafeVisit(node);

            return new CognitiveComplexity(walker.State.Complexity, walker.State.IncrementLocations.ToImmutableArray());
        }

        private class CognitiveWalker : CSharpSyntaxWalker
        {
            public CognitiveComplexityWalkerState<MethodDeclarationSyntax> State { get; }
                = new CognitiveComplexityWalkerState<MethodDeclarationSyntax>();

            public override void Visit(SyntaxNode node)
            {
                if (node.IsKind(SyntaxKindEx.LocalFunctionStatement))
                {
                    State.VisitWithNesting(node, base.Visit);
                }
                else
                {
                    base.Visit(node);
                }
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                State.CurrentMethod = node;
                base.VisitMethodDeclaration(node);

                if (State.HasDirectRecursiveCall)
                {
                    State.HasDirectRecursiveCall = false;
                    State.IncreaseComplexity(node.Identifier, 1, "+1 (recursion)");
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
                    State.IncreaseComplexityByNestingPlusOne(node.IfKeyword);
                    State.VisitWithNesting(node, base.VisitIfStatement);
                }
            }

            public override void VisitElseClause(ElseClauseSyntax node)
            {
                State.IncreaseComplexityByOne(node.ElseKeyword);
                base.VisitElseClause(node);
            }

            public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
            {
                State.IncreaseComplexityByNestingPlusOne(node.QuestionToken);
                State.VisitWithNesting(node, base.VisitConditionalExpression);
            }

            public override void VisitSwitchStatement(SwitchStatementSyntax node)
            {
                State.IncreaseComplexityByNestingPlusOne(node.SwitchKeyword);
                State.VisitWithNesting(node, base.VisitSwitchStatement);
            }

            public override void VisitForStatement(ForStatementSyntax node)
            {
                State.IncreaseComplexityByNestingPlusOne(node.ForKeyword);
                State.VisitWithNesting(node, base.VisitForStatement);
            }

            public override void VisitWhileStatement(WhileStatementSyntax node)
            {
                State.IncreaseComplexityByNestingPlusOne(node.WhileKeyword);
                State.VisitWithNesting(node, base.VisitWhileStatement);
            }

            public override void VisitDoStatement(DoStatementSyntax node)
            {
                State.IncreaseComplexityByNestingPlusOne(node.DoKeyword);
                State.VisitWithNesting(node, base.VisitDoStatement);
            }

            public override void VisitForEachStatement(ForEachStatementSyntax node)
            {
                State.IncreaseComplexityByNestingPlusOne(node.ForEachKeyword);
                State.VisitWithNesting(node, base.VisitForEachStatement);
            }

            public override void VisitCatchClause(CatchClauseSyntax node)
            {
                State.IncreaseComplexityByNestingPlusOne(node.CatchKeyword);
                State.VisitWithNesting(node, base.VisitCatchClause);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var identifierNameSyntax = node.Expression as IdentifierNameSyntax;
                if (State.CurrentMethod != null &&
                    identifierNameSyntax != null &&
                    node.HasExactlyNArguments(State.CurrentMethod.ParameterList.Parameters.Count) &&
                    string.Equals(identifierNameSyntax.Identifier.ValueText,
                        State.CurrentMethod.Identifier.ValueText, StringComparison.Ordinal))
                {
                    State.HasDirectRecursiveCall = true;
                }

                base.VisitInvocationExpression(node);
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                var nodeKind = node.Kind();
                if (!State.LogicalOperationsToIgnore.Contains(node) &&
                    (nodeKind == SyntaxKind.LogicalAndExpression ||
                     nodeKind == SyntaxKind.LogicalOrExpression))
                {
                    var left = node.Left.RemoveParentheses();
                    if (!left.IsKind(nodeKind))
                    {
                        State.IncreaseComplexityByOne(node.OperatorToken);
                    }

                    var right = node.Right.RemoveParentheses();
                    if (right.IsKind(nodeKind))
                    {
                        State.LogicalOperationsToIgnore.Add(right);
                    }
                }

                base.VisitBinaryExpression(node);
            }

            public override void VisitGotoStatement(GotoStatementSyntax node)
            {
                State.IncreaseComplexityByNestingPlusOne(node.GotoKeyword);
                base.VisitGotoStatement(node);
            }

            public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            {
                State.VisitWithNesting(node, base.VisitSimpleLambdaExpression);
            }

            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
            {
                State.VisitWithNesting(node, base.VisitParenthesizedLambdaExpression);
            }
        }
    }
}
