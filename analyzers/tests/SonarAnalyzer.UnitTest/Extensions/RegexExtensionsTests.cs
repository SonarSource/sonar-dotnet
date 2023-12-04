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
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions;

[TestClass]
public class RegexExtensionsTest
{
    private const string TimeoutPattern =
        "^((?<DRIVE>[a-zA-Z]):\\\\)*((?<DIR>[a-zA-Z0-9_]+(([a-zA-Z0-9_\\s_\\-\\.]*[a-zA-Z0-9_]+)|([a-zA-Z0-9_]+)))\\\\)*(?<FILE>([a-zA-Z0-9_]+(([a-zA-Z0-9_\\s_\\-\\.]*[a-zA-Z0-9_]+)|([a-zA-Z0-9_]+))\\.(?<EXTENSION>[a-zA-Z0-9]{1,6})$))";

    [TestMethod]
    public void IsMatchSilent_Timeout_ReturnsFalse()
    {
        var regex = new Regex(TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(1));

        regex.IsMatchSilent(@"C:\Users\username\AppData\Local\Temp\00af5451-626f-40db-af1d-89d376dc5ef6\SomeFile.csproj").Should().BeFalse();
    }

    [TestMethod]
    [DataRow("a", "a", true)]
    [DataRow("a", "b", false)]
    [DataRow("\\w+", "abc123", true)]
    [DataRow("\\W+", "abc123", false)]
    public void IsMatchSilent_NoTimeout_ReturnsExpectedResult(string pattern, string input, bool expectedResult)
    {
        var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(500));

        regex.IsMatchSilent(input).Should().Be(expectedResult);
    }

    [TestMethod]
    public void MatchSilent_Timeout_ReturnsEmpty()
    {
        var regex = new Regex(TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(1));

        regex.MatchSilent(@"C:\Users\username\AppData\Local\Temp\00af5451-626f-40db-af1d-89d376dc5ef6\SomeFile.csproj").Should().Be(Match.Empty);
    }

    [TestMethod]
    [DataRow("a", "a", true)]
    [DataRow("a", "b", false)]
    [DataRow("\\w+", "abc123", true)]
    [DataRow("\\W+", "abc123", false)]
    public void MatchSilent_NoTimeout_ReturnsExpectedResult(string pattern, string input, bool expectedResult)
    {
        var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(500));

        regex.MatchSilent(input).Success.Should().Be(expectedResult);
    }
}
