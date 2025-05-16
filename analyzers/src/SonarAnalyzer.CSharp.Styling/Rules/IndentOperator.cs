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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IndentOperator : IndentBase
{
    public IndentOperator() : base("T0019", "operator") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c =>
            {
                var binary = (BinaryExpressionSyntax)c.Node;
                if (ExpectedPosition(binary) is { } expected)
                {
                    Verify(c, expected, binary.OperatorToken, binary.Right);
                }
            },
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.CoalesceExpression,
            SyntaxKind.AddExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftExpression,
            SyntaxKind.UnsignedRightShiftExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.AsExpression,
            SyntaxKind.IsExpression);
        context.RegisterNodeAction(c =>
            {
                var binary = (BinaryPatternSyntax)c.Node;
                if (ExpectedPosition(binary) is { } expected)
                {
                    Verify(c, expected, binary.OperatorToken, binary.Right);
                }
            },
            SyntaxKind.AndPattern,
            SyntaxKind.OrPattern);
        context.RegisterNodeAction(c =>
            {
                var isPattern = (IsPatternExpressionSyntax)c.Node;
                if (ExpectedPosition(isPattern) is { } expected)
                {
                    Verify(c, expected, isPattern.IsKeyword);
                }
            },
            SyntaxKind.IsPatternExpression);
        context.RegisterNodeAction(c =>
            {
                var range = (RangeExpressionSyntax)c.Node;
                if (ExpectedPosition(range) is { } expected)
                {
                    Verify(c, expected, range.OperatorToken, range.RightOperand);
                }
            },
            SyntaxKind.RangeExpression);
    }

    protected override int Offset(SyntaxNode node, SyntaxNode root) =>
        node == root
            ? 3     // When rooted from the same expression, align itself to the same expression from the previous line
            : 4;    // Otherwise force one tab, like in the base class

    protected override SyntaxNode NodeRoot(SyntaxNode node, SyntaxNode current)
    {
        if (current is ArrowExpressionClauseSyntax or LambdaExpressionSyntax or ConditionalExpressionSyntax or ForStatementSyntax)
        {
            return node;
        }
        else if (current is IfStatementSyntax { Parent: ElseClauseSyntax })
        {
            return null;
        }
        else if (current is ElseClauseSyntax)
        {
            return current;
        }
        else if (current is StatementSyntax or AssignmentExpressionSyntax or SwitchExpressionArmSyntax
            || (current is InvocationExpressionSyntax && current.GetFirstToken().IsFirstTokenOnLine()))
        {
            return current;
        }
        else if (current is ParenthesizedExpressionSyntax)
        {
            return current.GetSelfOrTopParenthesizedExpression();
        }
        else
        {
            return null;
        }
    }
}
