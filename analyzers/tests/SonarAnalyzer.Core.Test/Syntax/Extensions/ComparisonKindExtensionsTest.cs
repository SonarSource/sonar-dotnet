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
public class ComparisonKindExtensionsTest
{
    [TestMethod]
    [DataRow(ComparisonKind.Equals, "==")]
    [DataRow(ComparisonKind.NotEquals, "!=")]
    [DataRow(ComparisonKind.LessThan, "<")]
    [DataRow(ComparisonKind.LessThanOrEqual, "<=")]
    [DataRow(ComparisonKind.GreaterThan, ">")]
    [DataRow(ComparisonKind.GreaterThanOrEqual, ">=")]
    public void ToDisplayString_CSharp(ComparisonKind kind, string expected) =>
        kind.ToDisplayString(AnalyzerLanguage.CSharp).Should().Be(expected);

    [TestMethod]
    [DataRow(ComparisonKind.Equals, "=")]
    [DataRow(ComparisonKind.NotEquals, "<>")]
    [DataRow(ComparisonKind.LessThan, "<")]
    [DataRow(ComparisonKind.LessThanOrEqual, "<=")]
    [DataRow(ComparisonKind.GreaterThan, ">")]
    [DataRow(ComparisonKind.GreaterThanOrEqual, ">=")]
    public void ToDisplayString_VisualBasic(ComparisonKind kind, string expected) =>
        kind.ToDisplayString(AnalyzerLanguage.VisualBasic).Should().Be(expected);

    [TestMethod]
    public void ToDisplayString_None_CSharp_Throws() =>
        ComparisonKind.None.Invoking(x => x.ToDisplayString(AnalyzerLanguage.CSharp)).Should().Throw<InvalidOperationException>();

    [TestMethod]
    public void ToDisplayString_None_VisualBasic_Throws() =>
        ComparisonKind.None.Invoking(x => x.ToDisplayString(AnalyzerLanguage.VisualBasic)).Should().Throw<InvalidOperationException>();

    [TestMethod]
    public void ToDisplayString_UnsupportedLanguage_Throws() =>
        ComparisonKind.Equals.Invoking(x => x.ToDisplayString(null)).Should().Throw<NotSupportedException>();
}
