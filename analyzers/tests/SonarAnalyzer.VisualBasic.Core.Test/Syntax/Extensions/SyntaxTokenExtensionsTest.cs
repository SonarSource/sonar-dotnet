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
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions.Test;

[TestClass]
public class SyntaxTokenExtensionsTest
{
    [TestMethod]
    [DataRow(SyntaxKind.EqualsToken, ComparisonKind.Equals)]
    [DataRow(SyntaxKind.LessThanGreaterThanToken, ComparisonKind.NotEquals)]
    [DataRow(SyntaxKind.LessThanToken, ComparisonKind.LessThan)]
    [DataRow(SyntaxKind.LessThanEqualsToken, ComparisonKind.LessThanOrEqual)]
    [DataRow(SyntaxKind.GreaterThanToken, ComparisonKind.GreaterThan)]
    [DataRow(SyntaxKind.GreaterThanEqualsToken, ComparisonKind.GreaterThanOrEqual)]
    [DataRow(SyntaxKind.PlusToken, ComparisonKind.None)]
    public void ToComparisonKind(SyntaxKind tokenKind, ComparisonKind expected) =>
        Token(tokenKind).ToComparisonKind().Should().Be(expected);
}
