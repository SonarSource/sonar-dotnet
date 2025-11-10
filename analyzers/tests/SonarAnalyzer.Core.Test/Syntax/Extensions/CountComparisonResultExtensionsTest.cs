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

using SonarAnalyzer.Core.Syntax.Utilities;

namespace SonarAnalyzer.Core.Syntax.Extensions.Test;

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
