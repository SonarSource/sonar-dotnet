/*
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

using System.Numerics;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal static class ArithmeticCalculator
{
    public static NumberConstraint Calculate(BinaryOperatorKind kind, NumberConstraint left, NumberConstraint right, bool isInLoop)
    {
        if (isInLoop)
        {
            return kind switch
            {
                BinaryOperatorKind.Add when left.IsPositive && right.IsPositive => NumberConstraint.From(left.Min + right.Min, null),
                BinaryOperatorKind.Add when left.IsNegative && right.IsNegative => NumberConstraint.From(null, left.Max + right.Max),
                _ => null
            };
        }
        else
        {
            return kind switch
            {
                BinaryOperatorKind.Add => NumberConstraint.From(left.Min + right.Min, left.Max + right.Max),
                BinaryOperatorKind.Subtract => NumberConstraint.From(left.Min - right.Max, left.Max - right.Min),
                BinaryOperatorKind.Multiply => CalculateMultiply(left, right),
                BinaryOperatorKind.Divide => CalculateDivide(left, right),
                BinaryOperatorKind.Remainder when left.IsSingleValue && right.IsSingleValue && right.Min != 0 => NumberConstraint.From(left.Min.Value % right.Min.Value),
                BinaryOperatorKind.Remainder => CalculateRemainder(left, right),
                BinaryOperatorKind.And when left.IsSingleValue && right.IsSingleValue => NumberConstraint.From(left.Min.Value & right.Min.Value),
                BinaryOperatorKind.And => NumberConstraint.From(CalculateAndMin(left, right), CalculateAndMax(left, right)),
                BinaryOperatorKind.Or when left.IsSingleValue && right.IsSingleValue => NumberConstraint.From(left.Min.Value | right.Min.Value),
                BinaryOperatorKind.Or => NumberConstraint.From(CalculateOrMin(left, right), CalculateOrMax(left, right)),
                BinaryOperatorKind.ExclusiveOr when left.IsSingleValue && right.IsSingleValue => NumberConstraint.From(left.Min.Value ^ right.Min.Value),
                BinaryOperatorKind.ExclusiveOr => NumberConstraint.From(CalculateXorMin(left, right), CalculateXorMax(left, right)),
                _ => null
            };
        }
    }

    public static BigInteger? BiggestMinimum(NumberConstraint left, NumberConstraint right)
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

    public static BigInteger? SmallestMaximum(NumberConstraint left, NumberConstraint right)
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

    private static NumberConstraint CalculateDivide(NumberConstraint left, NumberConstraint right)
    {
        if (right.Min == 0 && right.Max == 0)
        {
            return null;
        }
        right = AccountForZero(right);

        BigInteger? min, max;
        if (left.IsPositive && right.IsPositive)
        {
            min = left.Min / right.Max ?? 0;
            max = left.Max / right.Min;
        }
        else if (left.IsNegative && right.IsNegative)
        {
            min = left.Max / right.Min ?? 0;
            max = left.Min / right.Max;
        }
        else if (left.IsPositive && right.IsNegative)
        {
            min = left.Max / right.Max;
            max = left.Min / right.Min ?? 0;
        }
        else if (left.IsNegative && right.IsPositive)
        {
            min = left.Min / right.Min;
            max = left.Max / right.Max ?? 0;
        }
        else if (right.IsPositive)
        {
            min = left.Min / right.Min;
            max = left.Max / right.Min;
        }
        else if (right.IsNegative)
        {
            min = left.Max / right.Max;
            max = left.Min / right.Max;
        }
        else if (left.Min is not null && left.Max is not null)
        {
            // We ignore division by zero, so the result can never be absolutely bigger than the absolute value of the dividend.
            // a / 1 = a && a / -1 = -a => |result| <= |dividend|
            var absMax = BigInteger.Max(BigInteger.Abs(left.Min.Value), left.Max.Value);
            min = -absMax;
            max = absMax;
        }
        else
        {
            min = null;
            max = null;
        }
        return NumberConstraint.From(min, max);
    }

    private static NumberConstraint CalculateRemainder(NumberConstraint left, NumberConstraint right)
    {
        if (right.Min == 0 && right.Max == 0)
        {
            return null;
        }
        else
        {
            right = AccountForZero(right);
            return NumberConstraint.From(CalculateRemainderMin(left, right), CalculateRemainderMax(left, right));
        }
    }

    private static BigInteger? CalculateRemainderMin(NumberConstraint left, NumberConstraint right)
    {
        // If the absolute value of the divisor is bigger than the absolute value of the dividend, the result is equal to the dividend =>
        // If the absolute value of every divisor is bigger than the absolute value of any dividend => resulting range == dividend range.
        // Otherwise, the result is between 0 and the absolute value of the divisor - 1 for positive dividends or in the same range multiplied by -1 for negative dividends.
        if (left.CanBeNegative || left.Max < MinOfAbsoluteValues(right))
        {
            if (right.Min is null || right.Max is null)
            {
                // If right is not finite, there will always be a divisor for which its absolute value is bigger than the absolute value of any value in left => resulting range == dividend range.
                return left.Min;
            }
            else
            {
                // Otherwise, the result value is limited by the dividend and the divisor.
                var minDerivedFromRight = -MaxOfAbsoluteValues(right) + 1;
                return left.Min is null ? minDerivedFromRight : BigInteger.Max(left.Min.Value, minDerivedFromRight);
            }
        }
        else
        {
            return 0;
        }
    }

    private static BigInteger? CalculateRemainderMax(NumberConstraint left, NumberConstraint right)
    {
        // If the absolute value of the divisor is bigger than the absolute value of the dividend, the result is equal to the dividend =>
        // If the absolute value of every divisor is bigger than the absolute value of any dividend => resulting range == dividend range.
        // Otherwise, the result is between 0 and the absolute value of the divisor - 1 for positive dividends or in the same range multiplied by -1 for negative dividends.
        if (left.CanBePositive || -left.Min < MinOfAbsoluteValues(right))
        {
            if (right.Min is null || right.Max is null)
            {
                // If right is not finite, there will always be a divisor for which its absolute value is bigger than the absolute value of any value in left => resulting range == dividend range.
                return left.Max;
            }
            else
            {
                // Otherwise, the result value is limited by the dividend and the divisor.
                var maxDerivedFromRight = MaxOfAbsoluteValues(right) - 1;
                return left.Max is null ? maxDerivedFromRight : BigInteger.Min(left.Max.Value, maxDerivedFromRight);
            }
        }
        else
        {
            return 0;
        }
    }

    private static NumberConstraint AccountForZero(NumberConstraint constraint)
    {
        if (constraint.Min == 0)
        {
            return NumberConstraint.From(1, constraint.Max);
        }
        else if (constraint.Max == 0)
        {
            return NumberConstraint.From(constraint.Min, -1);
        }
        else
        {
            return constraint;
        }
    }

    private static BigInteger MaxOfAbsoluteValues(NumberConstraint constraint) =>
        BigInteger.Max(BigInteger.Abs(constraint.Min.Value), BigInteger.Abs(constraint.Max.Value));

    private static BigInteger MinOfAbsoluteValues(NumberConstraint constraint)
    {
        if (constraint.IsPositive)
        {
            return constraint.Min.Value;
        }
        else if (constraint.IsNegative)
        {
            return -constraint.Max.Value;
        }
        else
        {
            return 0;
        }
    }

    private static BigInteger? CalculateAndMin(NumberConstraint left, NumberConstraint right)
    {
        // The result can only be negative if both operands are negative => The result must be >= 0 unless both ranges include negative numbers.
        if (left.CanBeNegative && right.CanBeNegative)
        {
            return left.Min.HasValue && right.Min.HasValue ? NegativeMagnitude(BigInteger.Min(left.Min.Value, right.Min.Value)) : null;
        }
        else
        {
            return 0;
        }
    }

    private static BigInteger? CalculateAndMax(NumberConstraint left, NumberConstraint right)
    {
        // BitAnd can only turn 1s into 0s, not the other way around => If both operands have the same sign, the result cannot be bigger than the smaller of the two.
        if ((left.IsNegative && right.IsNegative) || (left.IsPositive && right.IsPositive))
        {
            return SmallestMaximum(left, right);
        }
        else if (left.IsPositive)
        {
            // At this point, right range R can be:
            // - negative => it can contain -1 or other negative numbers, which can at most make the result equal to max of L.
            // - mixed => it's the union of a negative sub-range Rn and a positive one Rp.
            //   - L & Rn can go as high as max of L, because Rn contains -1 (all ones).
            //   - L & Rp, because both are positive, cannot be bigger than the smaller of max of L and Rp, so smaller of max of L and R.
            // Therefore, L & R <= max(max of L, smaller of max of L and R) = max of L.
            return left.Max;
        }
        else if (right.IsPositive)
        {
            // Same reasoning as above
            return right.Max;
        }
        else
        {
            // For all other cases we exploit the fact that & cannot make the result bigger than any of its inputs
            return BiggestMaximum(left, right);
        }
    }

    private static BigInteger? CalculateOrMin(NumberConstraint left, NumberConstraint right)
    {
        // BitOr can only turn 0s into 1s, not the other way around => If both operands have the same sign, the result cannot be smaller than the bigger of the two.
        if ((left.IsNegative && right.IsNegative) || (left.IsPositive && right.IsPositive))
        {
            return BiggestMinimum(left, right);
        }
        else if (left.IsNegative)
        {
            // At this point, right range R can be:
            // - positive => it can contain 0 or other positive numbers, which can at most make the result as small as min of L.
            // - mixed => it's the union of a negative sub-range Rn and a positive one Rp.
            //   - L & Rn, because both are negative, cannot be smaller than the bigger of min of L and Rn, so bigger of min of L and R.
            //   - L & Rp can go as small as min of L, because Rp contains 0 (all zeros).
            // Therefore, L & R >= min(bigger of min of L and R, min of L) = min of L.
            return left.Min;
        }
        else if (right.IsNegative)
        {
            // Same reasoning as above
            return right.Min;
        }
        else
        {
            // For all other cases we exploit the fact that | cannot make the result smaller than any of its inputs
            return SmallestMinimum(left, right);
        }
    }

    private static BigInteger? CalculateOrMax(NumberConstraint left, NumberConstraint right)
    {
        // The result can only be positive if both operands are positive => The result must be < 0 unless both ranges include positive numbers.
        if (left.CanBePositive && right.CanBePositive)
        {
            return left.Max.HasValue && right.Max.HasValue ? PositiveMagnitude(BigInteger.Max(left.Max.Value, right.Max.Value)) : null;
        }
        else
        {
            return -1;
        }
    }

    private static BigInteger? CalculateXorMin(NumberConstraint left, NumberConstraint right)
    {
        if (left.IsPositive && right.IsPositive)
        {
            return SameSign(left, right);
        }
        else if (left.IsNegative && right.IsNegative)
        {
            return SameSign(right, left);
        }
        // Positive numbers start with Zeroes. Negative numbers start with Ones. XOR them, and the result will start with Ones and thus will be negative.
        // By taking a look at the number of starting Zeroes and Ones, we can also learn a limit for the number of starting Ones of the result.
        // Note: When passing a positive limit to NegativeMagnitude, it needs to be increased by 1 and then multiplied by -1 to get the expected result.
        else if ((left.IsPositive || right.IsNegative) && left.Max.HasValue && right.Min.HasValue)
        {
            return NegativeMagnitude(-BigInteger.Max(left.Max.Value + 1, BigInteger.Abs(right.Min.Value)));
        }
        else if ((left.IsNegative || right.IsPositive) && left.Min.HasValue && right.Max.HasValue)
        {
            return NegativeMagnitude(-BigInteger.Max(BigInteger.Abs(left.Min.Value), right.Max.Value + 1));
        }
        else if (left.Min.HasValue && left.Max.HasValue && right.Min.HasValue && right.Max.HasValue)
        {
            return NegativeMagnitude(-Max(BigInteger.Abs(left.Min.Value), left.Max.Value + 1, BigInteger.Abs(right.Min.Value), right.Max.Value + 1));
        }
        else
        {
            return null;
        }

        static BigInteger? SameSign(NumberConstraint range1, NumberConstraint range2)
        {
            // Takes advantage of the property a - b <= a ^ b for all a >= 0 and b >= 0
            // If ranges overlap => at least 1 value belongs to both ranges => xor can yield 0
            if (range1.Min > range2.Max)
            {
                return range1.Min.Value - range2.Max.Value;
            }
            else if (range2.Min > range1.Max)
            {
                return range2.Min.Value - range1.Max.Value;
            }
            else
            {
                return 0;
            }
        }
    }

    private static BigInteger? CalculateXorMax(NumberConstraint left, NumberConstraint right)
    {
        if ((left.IsPositive && right.IsNegative) || (left.IsNegative && right.IsPositive))
        {
            return -1;
        }
        else if ((left.IsPositive || right.IsPositive) && left.Max.HasValue && right.Max.HasValue)
        {
            return PositiveMagnitude(BigInteger.Max(left.Max.Value, right.Max.Value));
        }
        else if ((left.IsNegative || right.IsNegative) && left.Min.HasValue && right.Min.HasValue)
        {
            return PositiveMagnitude(BigInteger.Max(BigInteger.Abs(left.Min.Value), BigInteger.Abs(right.Min.Value)));
        }
        else if (left.Min.HasValue && left.Max.HasValue && right.Min.HasValue && right.Max.HasValue)
        {
            return PositiveMagnitude(Max(BigInteger.Abs(left.Min.Value), left.Max.Value, BigInteger.Abs(right.Min.Value), right.Max.Value));
        }
        else
        {
            return null;
        }
    }

    private static BigInteger Max(params BigInteger[] values) => values.Max();

    private static BigInteger? NegativeMagnitude(BigInteger value)
    {
        // For increasing powers of 2 with negative sign, we're looking for the longest chain of 1 from the MSB side
        // For example, given value = 0b1101000:
        // 0b11111111 > 0b1101000 -> continue
        // 0b11111110 > 0b1101000 -> continue
        // ...
        // 0b11000000 <= 0b1101000 -> stop!
        //   ^^            ^^
        BigInteger magnitude = -1;
        while (magnitude > value)
        {
            magnitude <<= 1;
        }
        return magnitude;
    }

    private static BigInteger? PositiveMagnitude(BigInteger value)
    {
        // For increasing precendents of powers of 2, we're looking for the longest chain of 0 from the MSB side
        // For example, given value = 0b0011010:
        // 0b00000001 < 0b0011010 -> continue
        // 0b00000011 < 0b0011010 -> continue
        // ...
        // 0b00111111 >= 0b0011010 -> stop!
        //   ^^            ^^
        BigInteger magnitude = 1;
        while (magnitude < value)
        {
            magnitude = (magnitude << 1) | 1;
        }
        return magnitude;
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
}
