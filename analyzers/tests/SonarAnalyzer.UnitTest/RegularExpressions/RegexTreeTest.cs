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
using System.Text.RegularExpressions;
using SonarAnalyzer.RegularExpressions;

namespace SonarAnalyzer.UnitTest.RegularExpressions;

[TestClass]
public class RegexTreeTest
{
    [DataTestMethod]
    [DataRow("[A", RegexOptions.None)]
#if NET7_0_OR_GREATER
    [DataRow(@"^([0-9]{2})(?<!00)$", RegexOptions.NonBacktracking)]
#endif
    public void Invalid_input_is_detected(string pattern, RegexOptions options) =>
        RegexTree.Parse(pattern, options).ParseError.Should().NotBeNull();

    [DataTestMethod]
    [DataRow(".*")]
    [DataRow("A?")]
    [DataRow("A{0,}")]
    [DataRow("(?:)*")]
    public void Matches_empty_string(string pattern) =>
        RegexTree.Parse(pattern, RegexOptions.None).MatchesEmptyString().Should().BeTrue();

    [DataTestMethod]
    [DataRow(".+")]
    [DataRow("A?B")]
    [DataRow("A{1,}")]
    [DataRow("A(?:)+")]
    public void Does_not_match_empty_string(string pattern) =>
       RegexTree.Parse(pattern, RegexOptions.None).MatchesEmptyString().Should().BeFalse();

    [DataTestMethod]
    [DataRow("A  B")]
    [DataRow("A   B")]
    [DataRow(" A B C  ")]
    public void Contains_adjecent_whitespace(string pattern) =>
       RegexTree.Parse(pattern, RegexOptions.None).ContainsAdjecentWhitespace().Should().BeTrue();

    [DataTestMethod]
    [DataRow("A B")]
    [DataRow("A  B", RegexOptions.IgnorePatternWhitespace)]
    [DataRow("A   B", RegexOptions.IgnorePatternWhitespace)]
    [DataRow(" A B C")]
    public void Does_not_contain_adjecent_whitespace(string pattern, RegexOptions options = default) =>
        RegexTree.Parse(pattern, options).ContainsAdjecentWhitespace().Should().BeFalse();
}
