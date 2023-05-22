/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Numerics;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed partial class Binary : BranchingProcessor<IBinaryOperationWrapper>
{
    private static NumberConstraint Calculate(BinaryOperatorKind kind, NumberConstraint left, NumberConstraint right) => kind switch
    {
        BinaryOperatorKind.Add => NumberConstraint.From(left.Min + right.Min, left.Max + right.Max),
        BinaryOperatorKind.Subtract => NumberConstraint.From(left.Min - right.Max, left.Max - right.Min),
        BinaryOperatorKind.Multiply => CalculateMultiply(left, right),
        BinaryOperatorKind.And when left.IsSingleValue && right.IsSingleValue => NumberConstraint.From(left.Min.Value & right.Min.Value),
        BinaryOperatorKind.And => CalculateAnd(left, right),
        BinaryOperatorKind.Or when left.IsSingleValue && right.IsSingleValue => NumberConstraint.From(left.Min.Value | right.Min.Value),
        BinaryOperatorKind.Or => CalculateOr(left, right),
        _ => null
    };

    private static NumberConstraint CalculateMultiply(NumberConstraint left, NumberConstraint right)
    {
        var products = new[] { left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max };
        var min = products.Min();
        var max = products.Max();

        if ((left.Min is null && right.CanBePositive) || (right.Min is null && left.CanBePositive)
            || (left.Max is null && right.CanBeNegative) || (right.Max is null && left.CanBeNegative))
        {
            min = null;
        }
        if ((left.Min is null && right.CanBeNegative) || (right.Min is null && left.CanBeNegative)
            || (left.Max is null && right.CanBePositive) || (right.Max is null && left.CanBePositive))
        {
            max = null;
        }

        return NumberConstraint.From(min, max);
    }

    private static NumberConstraint CalculateAnd(NumberConstraint left, NumberConstraint right)
    {
        // The result can only be negative if both operands are negative => The result must be >= 0 unless both ranges include negative numbers.
        BigInteger? min = 0;
        if (left.CanBeNegative && right.CanBeNegative)
        {
            min = left.Min.HasValue && right.Min.HasValue
                ? NegativeMagnitude(BigInteger.Min(left.Min.Value, right.Min.Value))
                : null;
        }
        return NumberConstraint.From(min, CalculateAndMax(left, right));
    }

    private static BigInteger? CalculateAndMax(NumberConstraint left, NumberConstraint right)
    {
        // BitAnd can only turn 1s into 0s, not the other way around => If both operands have the same sign, the result cannot be bigger than the smaller of the two.
        if ((left.IsNegative && right.IsNegative)
            || (left.IsPositive && right.IsPositive))
        {
            return SmallestMaximum(left, right);
        }
        // -1 == 0b11111111 => a & -1 == a =>
        // If one operand can be -1 and one is positive, the result will be positive, but not bigger than the positive operand.
        else if (left.IsPositive)
        {
            return left.Max;
        }
        else if (right.IsPositive)
        {
            return right.Max;
        }
        else
        {
            // If both operands can be -1 and both can have positive values => The result cannot be bigger than the bigger of the two maxima.
            // If one operand is negative and one can have positive values => The result cannot be bigger than the positive maximum.
            return BiggestMaximum(left, right);
        }
    }

    private static BigInteger? NegativeMagnitude(BigInteger value)
    {
        // For various inputs, we're looking for the longest chain of 1 from the MSB side
        // 11000000 - 64
        // 11000001 - 63
        // 11000010 - 62
        // ------------ -
        // 11000000 - 64 expected result
        // ^^
        BigInteger magnitude = -1;
        while (value < magnitude)
        {
            magnitude *= 2;
        }
        return magnitude;
    }

    private static BigInteger? SmallestMaximum(NumberConstraint left, NumberConstraint right)
    {
        if (left.Max is null)
        {
            return right?.Max;
        }
        else if (right?.Max is null)
        {
            return left.Max;
        }
        else
        {
            return BigInteger.Min(left.Max.Value, right.Max.Value);
        }
    }

    private static BigInteger? BiggestMaximum(NumberConstraint left, NumberConstraint right)
    {
        if (left.Max is null || right.Max is null)
        {
            return null;
        }
        else
        {
            return BigInteger.Max(left.Max.Value, right.Max.Value);
        }
    }

    private static BigInteger? SmallestMinimum(NumberConstraint left, NumberConstraint right)
    {
        if (left.Min is null || right.Min is null)
        {
            return null;
        }
        else
        {
            return BigInteger.Min(left.Min.Value, right.Min.Value);
        }
    }

    private static BigInteger? BiggestMinimum(NumberConstraint left, NumberConstraint right)
    {
        if (left.Min is null)
        {
            return right?.Min;
        }
        else if (right?.Min is null)
        {
            return left.Min;
        }
        else
        {
            return BigInteger.Max(left.Min.Value, right.Min.Value);
        }
    }

    private static NumberConstraint CalculateOr(NumberConstraint left, NumberConstraint right) =>
        NumberConstraint.From(CalculateOrMin(left, right), CalculateOrMax(left, right));

    private static BigInteger? CalculateOrMin(NumberConstraint left, NumberConstraint right)
    {
        // BitOr can only turn 0s into 1s, not the other way around => If both operands have the same sign, the result cannot be smaller than the bigger of the two.
        if ((left.IsNegative && right.IsNegative) || (left.IsPositive && right.IsPositive))
        {
            return BiggestMinimum(left, right);
        }
        // -1 == 0b11111111 => a | -1 == -1 =>
        // If one operand can be -1 and one is positive, the result will be negative, but not smaller than the negative operand.
        else if (left.IsNegative)
        {
            return left.Min;
        }
        else if (right.IsNegative)
        {
            return right.Min;
        }
        else
        {
            // If both operands can be -1 and both can have positive values => The result cannot be smaller than the smaller of the two minima.
            // If one operand is negative and one can have positive values => The result cannot be smaller than the negative minimum.
            return SmallestMinimum(left, right);
        }
    }

    private static BigInteger? CalculateOrMax(NumberConstraint left, NumberConstraint right)
    {
        // The result can only be positive if both operands are positive => The result must be < 0 unless both ranges include positive numbers.
        if (left.CanBePositive && right.CanBePositive)
        {
            return left.Max.HasValue && right.Max.HasValue ? left.Max.Value | right.Max.Value : null;
        }
        else
        {
            return -1;
        }
    }
}
