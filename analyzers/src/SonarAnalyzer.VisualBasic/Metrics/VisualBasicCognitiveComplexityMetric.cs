/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Common;

namespace SonarAnalyzer.VisualBasic.Metrics;

public static class VisualBasicCognitiveComplexityMetric
{
    public static CognitiveComplexity GetComplexity(SyntaxNode node)
    {
        var walker = new CognitiveWalker();
        walker.SafeVisit(node);

        return new CognitiveComplexity(walker.State.Complexity, walker.State.IncrementLocations.ToImmutableArray());
    }

    private class CognitiveWalker : SafeVisualBasicSyntaxWalker
    {
        public CognitiveComplexityWalkerState<MethodStatementSyntax> State { get; } = new();

        public override void VisitBinaryConditionalExpression(BinaryConditionalExpressionSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.IfKeyword);
            State.VisitWithNesting(node, base.VisitBinaryConditionalExpression);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var nodeKind = node.Kind();
            if (!State.LogicalOperationsToIgnore.Contains(node)
                && (nodeKind == SyntaxKind.AndExpression
                    || nodeKind == SyntaxKind.AndAlsoExpression
                    || nodeKind == SyntaxKind.OrExpression
                    || nodeKind == SyntaxKind.OrElseExpression))
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

        public override void VisitCatchBlock(CatchBlockSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.CatchStatement.CatchKeyword);
            State.VisitWithNesting(node, base.VisitCatchBlock);
        }

        public override void VisitDoLoopBlock(DoLoopBlockSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.DoStatement.DoKeyword);
            State.VisitWithNesting(node, base.VisitDoLoopBlock);
        }

        public override void VisitElseIfStatement(ElseIfStatementSyntax node)
        {
            State.IncreaseComplexityByOne(node.ElseIfKeyword);
            base.VisitElseIfStatement(node);
        }

        public override void VisitElseStatement(ElseStatementSyntax node)
        {
            State.IncreaseComplexityByOne(node.ElseKeyword);
            base.VisitElseStatement(node);
        }

        public override void VisitForEachBlock(ForEachBlockSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.ForEachStatement.ForKeyword);
            State.VisitWithNesting(node, base.VisitForEachBlock);
        }

        public override void VisitForBlock(ForBlockSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.ForStatement.ForKeyword);
            State.VisitWithNesting(node, base.VisitForBlock);
        }

        public override void VisitGoToStatement(GoToStatementSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.GoToKeyword);
            base.VisitGoToStatement(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression == null
                || node.ArgumentList == null
                || State.CurrentMethod == null
                || node.ArgumentList.Arguments.Count != State.CurrentMethod.ParameterList?.Parameters.Count)
            {
                return;
            }
            State.HasDirectRecursiveCall = string.Equals(GetIdentifierName(node.Expression), State.CurrentMethod.Identifier.ValueText, StringComparison.Ordinal);
            base.VisitInvocationExpression(node);

            static string GetIdentifierName(ExpressionSyntax expression)
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

        public override void VisitMethodBlock(MethodBlockSyntax node)
        {
            State.CurrentMethod = node.SubOrFunctionStatement;
            base.VisitMethodBlock(node);

            if (State.HasDirectRecursiveCall)
            {
                State.HasDirectRecursiveCall = false;
                State.IncreaseComplexity(node.SubOrFunctionStatement.Identifier, 1, "+1 (recursion)");
            }
        }

        public override void VisitMultiLineIfBlock(MultiLineIfBlockSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.IfStatement.IfKeyword);
            State.VisitWithNesting(node, base.VisitMultiLineIfBlock);
        }

        public override void VisitMultiLineLambdaExpression(MultiLineLambdaExpressionSyntax node) =>
            State.VisitWithNesting(node, base.VisitMultiLineLambdaExpression);

        public override void VisitSelectBlock(SelectBlockSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.SelectStatement.SelectKeyword);
            State.VisitWithNesting(node, base.VisitSelectBlock);
        }

        public override void VisitSingleLineIfStatement(SingleLineIfStatementSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.IfKeyword);
            State.VisitWithNesting(node, base.VisitSingleLineIfStatement);
        }

        public override void VisitSingleLineLambdaExpression(SingleLineLambdaExpressionSyntax node) =>
            State.VisitWithNesting(node, base.VisitSingleLineLambdaExpression);

        public override void VisitTernaryConditionalExpression(TernaryConditionalExpressionSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.IfKeyword);
            State.VisitWithNesting(node, base.VisitTernaryConditionalExpression);
        }

        public override void VisitWhileBlock(WhileBlockSyntax node)
        {
            State.IncreaseComplexityByNestingPlusOne(node.WhileStatement.WhileKeyword);
            State.VisitWithNesting(node, base.VisitWhileBlock);
        }
    }
}
