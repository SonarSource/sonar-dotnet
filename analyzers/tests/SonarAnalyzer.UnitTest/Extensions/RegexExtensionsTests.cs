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
    // https://stackoverflow.com/questions/3403512/regex-match-take-a-very-long-time-to-execute
    // Regular expression with catastrophic backtracking to ensure timeout exception when a small timeout is set
    private const string TimeoutPattern =
        @"^((?<DRIVE>[a-zA-Z]):\\)*((?<DIR>[a-zA-Z0-9_]+(([a-zA-Z0-9_\s_\-\.]*[a-zA-Z0-9_]+)|([a-zA-Z0-9_]+)))\\)*(?<FILE>([a-zA-Z0-9_]+(([a-zA-Z0-9_\s_\-\.]*[a-zA-Z0-9_]+)|([a-zA-Z0-9_]+))\.(?<EXTENSION>[a-zA-Z0-9]{1,6})$))";

    [TestMethod]
    [DataRow(@"C:\Users\username\AppData\Local\Temp\00af5451-626f-40db-af1d-89d376dc5ef6\SomeFile.csproj", 1, false)]
    [DataRow(@"C:\Users\username\AppData\Local\Temp\00af5451-626f-40db-af1d-89d376dc5ef6\SomeFile.csproj", 1_000_000, true)]
    [DataRow(@"äöü", 1, false)]
    [DataRow(@"äöü", 1_000_000, false)]
    public void SafeIsMatch_Timeout(string input, long timeoutTicks, bool matchSucceed)
    {
        var regex = new Regex(TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(timeoutTicks));

        regex.SafeIsMatch(input).Should().Be(matchSucceed);
    }

    [TestMethod]
    [DataRow(@"C:\Users\username\AppData\Local\Temp\00af5451-626f-40db-af1d-89d376dc5ef6\SomeFile.csproj", 1, false)]
    [DataRow(@"C:\Users\username\AppData\Local\Temp\00af5451-626f-40db-af1d-89d376dc5ef6\SomeFile.csproj", 1_000_000, true)]
    [DataRow(@"äöü", 1, false)]
    [DataRow(@"äöü", 1_000_000, false)]
    public void SafeMatch_Timeout(string input, long timeoutTicks, bool matchSucceed)
    {
        var regex = new Regex(TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(timeoutTicks));

        regex.SafeMatch(input).Success.Should().Be(matchSucceed);
    }
}
