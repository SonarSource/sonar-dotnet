﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Helpers
{
    internal class VisualBasicExpressionNumericConverter : ExpressionNumericConverterBase<LiteralExpressionSyntax, UnaryExpressionSyntax>
    {
        private static readonly ISet<SyntaxKind> SupportedOperatorTokens = new HashSet<SyntaxKind>
        {
            SyntaxKind.MinusToken,
            SyntaxKind.PlusToken
        };

        protected override object TokenValue(LiteralExpressionSyntax literalExpression) =>
            literalExpression.Token.Value;

        protected override SyntaxNode Operand(UnaryExpressionSyntax unaryExpression) =>
            unaryExpression.Operand;

        protected override bool IsSupportedOperator(UnaryExpressionSyntax unaryExpression) =>
            SupportedOperatorTokens.Contains(unaryExpression.OperatorToken.Kind());

        protected override bool IsMinusOperator(UnaryExpressionSyntax unaryExpression) =>
            unaryExpression.OperatorToken.Kind() == SyntaxKind.MinusToken;

        protected override SyntaxNode RemoveParentheses(SyntaxNode expression) =>
            expression.RemoveParentheses();
    }
}
