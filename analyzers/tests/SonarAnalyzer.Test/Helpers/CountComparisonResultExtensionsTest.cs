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

namespace SonarAnalyzer.Test.Helpers
{
    [TestClass]
    public class CountComparisonResultExtensionsTest
    {
        [TestMethod]
        [DataRow(ComparisonKind.Equals, -1, CountComparisonResult.AlwaysFalse)]
        [DataRow(ComparisonKind.Equals, +0, CountComparisonResult.Empty)]
        [DataRow(ComparisonKind.Equals, +1, CountComparisonResult.SizeDepedendent)]
        [DataRow(ComparisonKind.Equals, +9, CountComparisonResult.SizeDepedendent)]
        [DataRow(ComparisonKind.NotEquals, -1, CountComparisonResult.AlwaysTrue)]
        [DataRow(ComparisonKind.NotEquals, +0, CountComparisonResult.NotEmpty)]
        [DataRow(ComparisonKind.NotEquals, +1, CountComparisonResult.SizeDepedendent)]
        [DataRow(ComparisonKind.NotEquals, +9, CountComparisonResult.SizeDepedendent)]
        [DataRow(ComparisonKind.GreaterThan, -1, CountComparisonResult.AlwaysTrue)]
        [DataRow(ComparisonKind.GreaterThan, +0, CountComparisonResult.NotEmpty)]
        [DataRow(ComparisonKind.GreaterThan, +1, CountComparisonResult.SizeDepedendent)]
        [DataRow(ComparisonKind.GreaterThan, +9, CountComparisonResult.SizeDepedendent)]
        [DataRow(ComparisonKind.LessThan, -1, CountComparisonResult.AlwaysFalse)]
        [DataRow(ComparisonKind.LessThan, +0, CountComparisonResult.AlwaysFalse)]
        [DataRow(ComparisonKind.LessThan, +1, CountComparisonResult.Empty)]
        [DataRow(ComparisonKind.LessThan, +2, CountComparisonResult.SizeDepedendent)]
        [DataRow(ComparisonKind.GreaterThanOrEqual, -1, CountComparisonResult.AlwaysTrue)]
        [DataRow(ComparisonKind.GreaterThanOrEqual, +0, CountComparisonResult.AlwaysTrue)]
        [DataRow(ComparisonKind.GreaterThanOrEqual, +1, CountComparisonResult.NotEmpty)]
        [DataRow(ComparisonKind.GreaterThanOrEqual, +2, CountComparisonResult.SizeDepedendent)]
        [DataRow(ComparisonKind.LessThanOrEqual, -9, CountComparisonResult.AlwaysFalse)]
        [DataRow(ComparisonKind.LessThanOrEqual, -1, CountComparisonResult.AlwaysFalse)]
        [DataRow(ComparisonKind.LessThanOrEqual, +0, CountComparisonResult.Empty)]
        [DataRow(ComparisonKind.LessThanOrEqual, +1, CountComparisonResult.SizeDepedendent)]
        public void Compare(ComparisonKind comparison, int count, CountComparisonResult expected) =>
            comparison.Compare(count).Should().Be(expected);
    }
}
