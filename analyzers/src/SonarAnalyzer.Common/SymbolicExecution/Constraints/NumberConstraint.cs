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

namespace SonarAnalyzer.SymbolicExecution.Constraints;

public sealed class NumberConstraint : SymbolicConstraint
{
    public static readonly NumberConstraint Empty = new(null, null);
    public static readonly NumberConstraint Zero = new(0, 0);
    public static readonly NumberConstraint One = new(1, 1);

    public BigInteger? Min { get; }
    public BigInteger? Max { get; }

    public override SymbolicConstraint Opposite =>
        null;

    private NumberConstraint(BigInteger? min, BigInteger? max) : base(ConstraintKind.Number)
    {
        if (min.HasValue && max.HasValue && min.Value > max.Value)
        {
            Max = min;  // Swap
            Min = max;
        }
        else
        {
            Min = min;
            Max = max;
        }
    }

    public static NumberConstraint From(BigInteger value)
    {
        if (value.IsZero)
        {
            return Zero;
        }
        else if (value.IsOne)
        {
            return One;
        }
        else
        {
            return new(value, value);
        }
    }

    public static NumberConstraint From(BigInteger? min, BigInteger? max)
    {
        if (min.HasValue && max.HasValue && min.Value == max.Value)
        {
            return From(min.Value);
        }
        else if (min.HasValue || max.HasValue)
        {
            return new(min, max);
        }
        else
        {
            return Empty;
        }
    }

    public override bool Equals(object obj) =>
        obj is NumberConstraint other && other.Min == Min && other.Max == Max;

    public override int GetHashCode() =>
        HashCode.Combine(Min?.GetHashCode() ?? 0, Max?.GetHashCode() ?? 0);

    public override string ToString() =>
        Min.HasValue && Min == Max ? $"{Kind} {Min}" : $"{Kind} from {Serialize(Min)} to {Serialize(Max)}";

    private static string Serialize(BigInteger? value) =>
        value.HasValue ? value.Value.ToString() : "*";
}
