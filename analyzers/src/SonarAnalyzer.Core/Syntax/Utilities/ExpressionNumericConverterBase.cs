/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Syntax.Utilities;

public abstract class ExpressionNumericConverterBase<TLiteralExpressionSyntax, TUnaryExpressionSyntax> : IExpressionNumericConverter
    where TLiteralExpressionSyntax : SyntaxNode
    where TUnaryExpressionSyntax : SyntaxNode
{
    protected abstract object TokenValue(TLiteralExpressionSyntax literalExpression);
    protected abstract SyntaxNode Operand(TUnaryExpressionSyntax unaryExpression);
    protected abstract bool IsSupportedOperator(TUnaryExpressionSyntax unaryExpression);
    protected abstract bool IsMinusOperator(TUnaryExpressionSyntax unaryExpression);
    protected abstract SyntaxNode RemoveParentheses(SyntaxNode expression);

    public int? ConstantIntValue(SemanticModel model, SyntaxNode expression) =>
        ConstantValue(model, expression, Convert.ToInt32, (multiplier, v) => multiplier * v);

    public int? ConstantIntValue(SyntaxNode expression) =>
        ConstantValue(null, expression, Convert.ToInt32, (multiplier, v) => multiplier * v);

    public double? ConstantDoubleValue(SyntaxNode expression) =>
        ConstantValue(null, expression, Convert.ToDouble, (multiplier, v) => multiplier * v);

    public decimal? ConstantDecimalValue(SyntaxNode expression) =>
        ConstantValue(null, expression, Convert.ToDecimal, (multiplier, v) => multiplier * v);

    private T? ConstantValue<T>(SemanticModel model, SyntaxNode expression, Func<object, T> converter, Func<int, T, T> multiplierCalculator)
        where T : struct
    {
        expression = RemoveParentheses(expression);
        if (Multiplier(expression, out var internalExpression) is int multiplier
            && internalExpression is TLiteralExpressionSyntax literalExpression
            && Conversions.ConvertWith(TokenValue(literalExpression), converter) is { } value)
        {
            return multiplierCalculator(multiplier, value);
        }
        else if (model?.GetConstantValue(expression) is { HasValue: true } optional && optional.Value is T constantValue)
        {
            return constantValue;
        }
        else
        {
            return null;
        }
    }

    private int? Multiplier(SyntaxNode expression, out SyntaxNode internalExpression)
    {
        var multiplier = 1;
        internalExpression = expression;
        var unary = internalExpression as TUnaryExpressionSyntax;
        while (unary is not null)
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
