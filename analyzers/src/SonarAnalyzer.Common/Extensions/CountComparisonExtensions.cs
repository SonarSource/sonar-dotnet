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

namespace SonarAnalyzer.Helpers
{
    public static class CountComparisonExtensions
    {
        public static bool AnyOrNotAny(this CountComparisonKind comparison) =>
            comparison == CountComparisonKind.Any || comparison == CountComparisonKind.NotAny;

        public static CountComparisonKind Count(this ComparisonKind comparison, int count) =>
            comparison switch
            {
                ComparisonKind.Equals => Equals(count),
                ComparisonKind.NotEquals => NotEquals(count),

                ComparisonKind.GreaterThanOrEqual => GreaterThanOrEqual(count),
                ComparisonKind.GreaterThan => GreaterThan(count),

                ComparisonKind.LessThan => LessThan(count),
                ComparisonKind.LessThanOrEqual => LessThanOrEqual(count),

                _ => CountComparisonKind.None,
            };

        private static CountComparisonKind Equals(int count) =>
            count > 0
            ? CountComparisonKind.SizeDepedendent
            : count == 0
                ? CountComparisonKind.NotAny
                : CountComparisonKind.AlwaysFalse;

        private static CountComparisonKind NotEquals(int count) =>
            count > 0
            ? CountComparisonKind.SizeDepedendent
            : count == 0
                ? CountComparisonKind.Any
                : CountComparisonKind.AlwaysTrue;

        private static CountComparisonKind GreaterThan(int count) =>
            count > 0
            ? CountComparisonKind.SizeDepedendent
            : count == 0
                ? CountComparisonKind.Any
                : CountComparisonKind.AlwaysTrue;

        private static CountComparisonKind GreaterThanOrEqual(int count) =>
            count > 1
            ? CountComparisonKind.SizeDepedendent
            : count == 1
                ? CountComparisonKind.Any
                : CountComparisonKind.AlwaysTrue;

        private static CountComparisonKind LessThan(int count) =>
            count > 1
            ? CountComparisonKind.SizeDepedendent
            : count == 1
                ? CountComparisonKind.NotAny
                : CountComparisonKind.AlwaysFalse;

        private static CountComparisonKind LessThanOrEqual(int count) =>
            count > 0
            ? CountComparisonKind.SizeDepedendent
            : count == 0
                ? CountComparisonKind.NotAny
                : CountComparisonKind.AlwaysFalse;
    }
}
