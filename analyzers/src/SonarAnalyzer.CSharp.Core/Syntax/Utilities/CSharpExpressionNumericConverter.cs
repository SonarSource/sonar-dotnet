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

namespace SonarAnalyzer.CSharp.Core.Syntax.Utilities;

public class CSharpExpressionNumericConverter : ExpressionNumericConverterBase<LiteralExpressionSyntax, PrefixUnaryExpressionSyntax>
{
    private static readonly ISet<SyntaxKind> SupportedOperatorTokens = new HashSet<SyntaxKind>
    {
        SyntaxKind.MinusToken,
        SyntaxKind.PlusToken
    };

    protected override object TokenValue(LiteralExpressionSyntax literalExpression) =>
        literalExpression.Token.Value;

    protected override SyntaxNode Operand(PrefixUnaryExpressionSyntax unaryExpression) =>
        unaryExpression.Operand;

    protected override bool IsSupportedOperator(PrefixUnaryExpressionSyntax unaryExpression) =>
        SupportedOperatorTokens.Contains(unaryExpression.OperatorToken.Kind());

    protected override bool IsMinusOperator(PrefixUnaryExpressionSyntax unaryExpression) =>
        unaryExpression.OperatorToken.IsKind(SyntaxKind.MinusToken);

    protected override SyntaxNode RemoveParentheses(SyntaxNode expression) =>
        expression.RemoveParentheses();
}
