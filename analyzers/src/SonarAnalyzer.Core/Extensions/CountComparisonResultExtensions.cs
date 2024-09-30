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

namespace SonarAnalyzer.Core.Extensions;

public static class CountComparisonResultExtensions
{
    public static bool IsEmptyOrNotEmpty(this CountComparisonResult comparison) =>
        comparison == CountComparisonResult.Empty || comparison == CountComparisonResult.NotEmpty;

    public static bool IsInvalid(this CountComparisonResult comparison) =>
        comparison == CountComparisonResult.AlwaysFalse || comparison == CountComparisonResult.AlwaysTrue;

    public static CountComparisonResult Compare(this ComparisonKind comparison, int count) =>
        comparison switch
        {
            ComparisonKind.Equals => Equals(count),
            ComparisonKind.NotEquals => NotEquals(count),
            ComparisonKind.GreaterThanOrEqual => GreaterThanOrEqual(count),
            ComparisonKind.GreaterThan => GreaterThan(count),
            ComparisonKind.LessThan => LessThan(count),
            ComparisonKind.LessThanOrEqual => LessThanOrEqual(count),
            _ => CountComparisonResult.None,
        };

    private static CountComparisonResult Equals(int count) =>
        Check(count, 0, CountComparisonResult.AlwaysFalse, CountComparisonResult.Empty);

    private static CountComparisonResult NotEquals(int count) =>
        Check(count, 0, CountComparisonResult.AlwaysTrue, CountComparisonResult.NotEmpty);

    private static CountComparisonResult GreaterThan(int count) =>
        Check(count, 0, CountComparisonResult.AlwaysTrue, CountComparisonResult.NotEmpty);

    private static CountComparisonResult GreaterThanOrEqual(int count) =>
        Check(count, 1, CountComparisonResult.AlwaysTrue, CountComparisonResult.NotEmpty);

    private static CountComparisonResult LessThan(int count) =>
        Check(count, 1, CountComparisonResult.AlwaysFalse, CountComparisonResult.Empty);

    private static CountComparisonResult LessThanOrEqual(int count) =>
        Check(count, 0, CountComparisonResult.AlwaysFalse, CountComparisonResult.Empty);

    private static CountComparisonResult Check(int count, int threshold, CountComparisonResult belowThreshold, CountComparisonResult onThreshold)
    {
        if (count == threshold)
        {
            return onThreshold;
        }
        else
        {
            return count < threshold ? belowThreshold : CountComparisonResult.SizeDepedendent;
        }
    }
}
