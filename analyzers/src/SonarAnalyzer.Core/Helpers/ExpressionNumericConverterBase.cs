/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Helpers;

public abstract class ExpressionNumericConverterBase<TLiteralExpressionSyntax, TUnaryExpressionSyntax> : IExpressionNumericConverter
    where TLiteralExpressionSyntax : SyntaxNode
    where TUnaryExpressionSyntax : SyntaxNode
{
    protected abstract object TokenValue(TLiteralExpressionSyntax literalExpression);
    protected abstract SyntaxNode Operand(TUnaryExpressionSyntax unaryExpression);
    protected abstract bool IsSupportedOperator(TUnaryExpressionSyntax unaryExpression);
    protected abstract bool IsMinusOperator(TUnaryExpressionSyntax unaryExpression);
    protected abstract SyntaxNode RemoveParentheses(SyntaxNode expression);

    public bool TryGetConstantIntValue(SemanticModel semanticModel, SyntaxNode expression, out int value) =>
        TryGetConstantValue(semanticModel, expression, Convert.ToInt32, (multiplier, v) => multiplier * v, out value);

    public bool TryGetConstantIntValue(SyntaxNode expression, out int value) =>
        TryGetConstantValue(null, expression, Convert.ToInt32, (multiplier, v) => multiplier * v, out value);

    public bool TryGetConstantDoubleValue(SyntaxNode expression, out double value) =>
        TryGetConstantValue(null, expression, Convert.ToDouble, (multiplier, v) => multiplier * v, out value);

    public bool TryGetConstantDecimalValue(SyntaxNode expression, out decimal value) =>
        TryGetConstantValue(null, expression, Convert.ToDecimal, (multiplier, v) => multiplier * v, out value);

    private bool TryGetConstantValue<T>(SemanticModel semanticModel, SyntaxNode expression, Func<object, T> converter, Func<int, T, T> multiplierCalculator, out T value)
        where T : struct
    {
        expression = RemoveParentheses(expression);

        if (GetMultiplier(expression, out var internalExpression) is int multiplier
            && internalExpression is TLiteralExpressionSyntax literalExpression
            && ConversionHelper.TryConvertWith(TokenValue(literalExpression), converter, out value))
        {
            value = multiplierCalculator(multiplier, value);
            return true;
        }
        else if (semanticModel is { }
            && semanticModel.GetConstantValue(expression) is { HasValue: true } optional
            && optional.Value is T val)
        {
            value = val;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
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
