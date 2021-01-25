/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public abstract class ExpressionNumericConverterBase<TLiteralExpressionSyntax, TUnaryExpressionSyntax> : IExpressionNumericConverter
        where TLiteralExpressionSyntax : SyntaxNode
        where TUnaryExpressionSyntax : SyntaxNode
    {
        protected abstract object TokenValue(TLiteralExpressionSyntax literalExpression);
        protected abstract SyntaxNode Operand(TUnaryExpressionSyntax unaryExpression);
        protected abstract bool IsSupportedOperator(TUnaryExpressionSyntax unaryExpression);
        protected abstract bool IsMinusOperator(TUnaryExpressionSyntax unaryExpression);

        public bool TryGetConstantIntValue(SyntaxNode expression, out int value) =>
            TryGetConstantValue(expression, Convert.ToInt32, (multiplier, v) => multiplier * v, out value);

        public bool TryGetConstantDoubleValue(SyntaxNode expression, out double value) =>
            TryGetConstantValue(expression, Convert.ToDouble, (multiplier, v) => multiplier * v, out value);

        private bool TryGetConstantValue<T>(SyntaxNode expression, Func<object, T> converter, Func<int, T, T> multiplierCalculator, out T value)
            where T : struct
        {
            var multiplier = GetMultiplier(expression, out var internalExpression);
            if (multiplier == null)
            {
                value = default;
                return false;
            }

            if (internalExpression is TLiteralExpressionSyntax literalExpression &&
                ConversionHelper.TryConvertWith(TokenValue(literalExpression), converter, out value))
            {
                value = multiplierCalculator(multiplier.Value, value);
                return true;
            }

            value = default;
            return false;
        }

        private int? GetMultiplier(SyntaxNode expression, out SyntaxNode internalExpression)
        {
            var multiplier = 1;
            internalExpression = expression;
            var unary = internalExpression as TUnaryExpressionSyntax;
            while (unary != null)
            {
                if (!IsSupportedOperator(unary))
                {
                    return null;
                }

                if (IsMinusOperator(unary))
                {
                    multiplier *= -1;
                }

                internalExpression = Operand(unary);
                unary = internalExpression as TUnaryExpressionSyntax;
            }

            return multiplier;
        }
    }
}
