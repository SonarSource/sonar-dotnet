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

namespace SonarAnalyzer.CSharp.Metrics;

public static class CSharpCognitiveComplexityMetric
{
    public static CognitiveComplexity GetComplexity(SyntaxNode node) =>
        GetComplexity(node, false);

    public static CognitiveComplexity GetComplexity(SyntaxNode node, bool onlyGlobalStatements)
    {
        var walker = new CognitiveWalker(onlyGlobalStatements);
        if (node.IsKind(SyntaxKindEx.LocalFunctionStatement))
        {
            walker.VisitLocalFunction((LocalFunctionStatementSyntaxWrapper)node, true);
        }
        else
        {
            walker.SafeVisit(node);
        }

        return new CognitiveComplexity(walker.State.Complexity, walker.State.IncrementLocations.ToImmutableArray());
    }

    private sealed class CognitiveWalker : SafeCSharpSyntaxWalker
    {
        private readonly bool onlyGlobalStatements;

        public CognitiveComplexityWalkerState<MethodDeclarationSyntax> State { get; } = new();

        public CognitiveWalker(bool onlyGlobalStatements) =>
            this.onlyGlobalStatements = onlyGlobalStatements;

        public override void Visit(SyntaxNode node)
        {
            if (node.IsKind(SyntaxKindEx.LocalFunctionStatement))
            {
                VisitLocalFunction((LocalFunctionStatementSyntaxWrapper)node, false);
            }
            else if (SwitchExpressionSyntaxWrapper.IsInstance(node))
            {
                var switchExpression = (SwitchExpressionSyntaxWrapper)node;

                State.IncreaseComplexityByNestingPlusOne(switchExpression.SwitchKeyword);
                State.VisitWithNesting(node, base.Visit);
            }
            else if (BinaryPatternSyntaxWrapper.IsInstance(node))
            {
                var nodeKind = node.Kind();
                var binaryPatternNode = (BinaryPatternSyntaxWrapper)node;
                if ((nodeKind == SyntaxKindEx.AndPattern || nodeKind == SyntaxKindEx.OrPattern)
                    && !State.LogicalOperationsToIgnore.Contains(binaryPatternNode))
                {
                    var left = binaryPatternNode.Left.SyntaxNode.RemoveParentheses();
                    if (!left.IsKind(nodeKind))
                    {
                        State.IncreaseComplexityByOne(binaryPatternNode.OperatorToken);
                    }

                    var right = binaryPatternNode.Right.SyntaxNode.RemoveParentheses();
                    if (right.IsKind(nodeKind))
                    {
                        State.LogicalOperationsToIgnore.Add(right);
                    }
                }

                base.Visit(node);
            }
            else
            {
                base.Visit(node);
            }
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            foreach (var globalStatement in node.Members.Where(x => x.IsKind(SyntaxKind.GlobalStatement)))
            {
                base.Visit(globalStatement);
            }

            if (!onlyGlobalStatements)
            {
                base.VisitCompilationUnit(node);
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
            if (State.CurrentMethod != null
                && node.Expression is IdentifierNameSyntax identifierNameSyntax
                && node.HasExactlyNArguments(State.CurrentMethod.ParameterList.Parameters.Count)
                && string.Equals(identifierNameSyntax.Identifier.ValueText, State.CurrentMethod.Identifier.ValueText, StringComparison.Ordinal))
            {
                State.HasDirectRecursiveCall = true;
            }

            base.VisitInvocationExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var nodeKind = node.Kind();
            if ((nodeKind == SyntaxKind.LogicalAndExpression || nodeKind == SyntaxKind.LogicalOrExpression)
                && !State.LogicalOperationsToIgnore.Contains(node))
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

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) =>
            State.VisitWithNesting(node, base.VisitSimpleLambdaExpression);

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) =>
            State.VisitWithNesting(node, base.VisitParenthesizedLambdaExpression);

        public void VisitLocalFunction(LocalFunctionStatementSyntaxWrapper localFunction, bool visitStaticLocalFunctions)
        {
            if (visitStaticLocalFunctions || !localFunction.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                State.VisitWithNesting(localFunction.SyntaxNode, base.Visit);
            }
        }
    }
}
