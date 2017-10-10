/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

namespace SonarAnalyzer.Helpers
{
    public sealed class CognitiveComplexityWalker : CSharpSyntaxWalker
    {
        private readonly List<SecondaryLocation> incrementLocations = new List<SecondaryLocation>();
        private readonly List<ExpressionSyntax> logicalOperationsToIgnore = new List<ExpressionSyntax>();

        private string currentMethodName;
        private int nestingLevel = 0;
        private bool hasDirectRecursiveCall = false;

        public int Complexity { get; private set; } = 0;

        public bool VisitEndedCorrectly => nestingLevel == 0;

        public IEnumerable<SecondaryLocation> IncrementLocations => incrementLocations;

        public void EnsureVisitEndedCorrectly()
        {
            if (!VisitEndedCorrectly)
            {
                throw new InvalidOperationException("There is a problem with the cognitive complexity walker. " +
                    $"Expecting ending nesting to be '0' got '{nestingLevel}'");
            }
        }

        public void Walk(SyntaxNode node)
        {
            try
            {
                Visit(node);
            }
            catch (InsufficientExecutionStackException)
            {
                // TODO: trace this exception

                // Roslyn walker overflows the stack when the depth of the call is around 2050.
                // See ticket #727.

                // Reset nesting level, so the problem with the walker is not reported.
                nestingLevel = 0;
            }
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            currentMethodName = node.Identifier.ValueText;
            base.VisitMethodDeclaration(node);

            if (hasDirectRecursiveCall)
            {
                IncreaseComplexity(node.Identifier, 1, "+1 (recursion)");
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
                IncreaseComplexityByNestingPlusOne(node.IfKeyword);
                VisitWithNesting(node, base.VisitIfStatement);
            }
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            IncreaseComplexityByOne(node.ElseKeyword);
            base.VisitElseClause(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            IncreaseComplexityByNestingPlusOne(node.QuestionToken);
            VisitWithNesting(node, base.VisitConditionalExpression);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            IncreaseComplexityByNestingPlusOne(node.SwitchKeyword);
            VisitWithNesting(node, base.VisitSwitchStatement);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            IncreaseComplexityByNestingPlusOne(node.ForKeyword);
            VisitWithNesting(node, base.VisitForStatement);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            IncreaseComplexityByNestingPlusOne(node.WhileKeyword);
            VisitWithNesting(node, base.VisitWhileStatement);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            IncreaseComplexityByNestingPlusOne(node.DoKeyword);
            VisitWithNesting(node, base.VisitDoStatement);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            IncreaseComplexityByNestingPlusOne(node.ForEachKeyword);
            VisitWithNesting(node, base.VisitForEachStatement);
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            IncreaseComplexityByNestingPlusOne(node.CatchKeyword);
            VisitWithNesting(node, base.VisitCatchClause);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var identifierNameSyntax = node.Expression as IdentifierNameSyntax;
            if (identifierNameSyntax != null &&
                identifierNameSyntax.Identifier.ValueText.Equals(currentMethodName, StringComparison.Ordinal))
            {
                hasDirectRecursiveCall = true;
            }

            base.VisitInvocationExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var nodeKind = node.Kind();
            if (!logicalOperationsToIgnore.Contains(node) &&
                (nodeKind == SyntaxKind.LogicalAndExpression ||
                 nodeKind == SyntaxKind.LogicalOrExpression))
            {
                var left = node.Left.RemoveParentheses();
                if (!left.IsKind(nodeKind))
                {
                    IncreaseComplexityByOne(node.OperatorToken);
                }

                var right = node.Right.RemoveParentheses();
                if (right.IsKind(nodeKind))
                {
                    logicalOperationsToIgnore.Add(right);
                }
            }

            base.VisitBinaryExpression(node);
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            IncreaseComplexityByNestingPlusOne(node.GotoKeyword);
            base.VisitGotoStatement(node);
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            VisitWithNesting(node, base.VisitSimpleLambdaExpression);
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            VisitWithNesting(node, base.VisitParenthesizedLambdaExpression);
        }

        private void VisitWithNesting<TSyntaxNode>(TSyntaxNode node, Action<TSyntaxNode> visit)
        {
            nestingLevel++;
            visit(node);
            nestingLevel--;
        }

        private void IncreaseComplexityByOne(SyntaxToken token)
        {
            IncreaseComplexity(token, 1, "+1");
        }

        private void IncreaseComplexityByNestingPlusOne(SyntaxToken token)
        {
            var increment = nestingLevel + 1;
            var message = increment == 1
                ? "+1"
                : $"+{increment} (incl {increment - 1} for nesting)";
            IncreaseComplexity(token, increment, message);
        }

        private void IncreaseComplexity(SyntaxToken token, int increment, string message)
        {
            Complexity += increment;
            incrementLocations.Add(new SecondaryLocation(token.GetLocation(), message));
        }
    }
}
