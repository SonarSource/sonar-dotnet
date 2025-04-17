/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class RegexExtensionsTest
{
    // https://stackoverflow.com/questions/3403512/regex-match-take-a-very-long-time-to-execute
    // Regular expression with catastrophic backtracking to ensure timeout exception when a small timeout is set
    private const string TimeoutPattern =
        @"^((?<DRIVE>[a-zA-Z]):\\)*((?<DIR>[a-zA-Z0-9_]+(([a-zA-Z0-9_\s_\-\.]*[a-zA-Z0-9_]+)|([a-zA-Z0-9_]+)))\\)*(?<FILE>([a-zA-Z0-9_]+(([a-zA-Z0-9_\s_\-\.]*[a-zA-Z0-9_]+)|([a-zA-Z0-9_]+))\.(?<EXTENSION>[a-zA-Z0-9]{1,6})$))";

    private const string MatchingPath = @"C:\Users\username\AppData\Local\Temp\00af5451-626f-40db-af1d-89d376dc5ef6\SomeFile.csproj";

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void SafeIsMatch_Timeout_Fallback(bool timeoutFallback)
    {
        var regex = new Regex(TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(1));
        regex.SafeIsMatch(MatchingPath, timeoutFallback).Should().Be(timeoutFallback);
    }

    [DataTestMethod]
    [DataRow(MatchingPath, 1, false)]
    [DataRow(MatchingPath, 1_000_000, true)]
    [DataRow("äöü", 1, false)]
    [DataRow("äöü", 1_000_000, false)]
    public void SafeMatch_Timeout(string input, long timeoutTicks, bool matchSucceed)
    {
        var regex = new Regex(TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(timeoutTicks));
        regex.SafeMatch(input).Success.Should().Be(matchSucceed);
    }

    [DataTestMethod]
    [DataRow(MatchingPath, 1, 0)]
    [DataRow(MatchingPath, 1_000_000, 1)]
    [DataRow("äöü", 1, 0)]
    [DataRow("äöü", 1_000_000, 0)]
    public void SafeMatches_Timeout(string input, long timeoutTicks, int matchCount)
    {
        var regex = new Regex(TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(timeoutTicks));
        var actual = regex.SafeMatches(input);
        actual.Count.Should().Be(matchCount);
        if (matchCount > 0)
        {
            var access = () => actual[0];
            access.Should().NotThrow().Which.Index.Should().Be(0);
        }
    }

    [DataTestMethod]
    [DataRow(MatchingPath, 1, MatchingPath)]
    [DataRow(MatchingPath, 1_000_000, "Replaced")]
    [DataRow("äöü", 1, "äöü")]
    [DataRow("äöü", 1_000_000, "äöü")]
    public void SafeReplace_Timeout(string input, long timeoutTicks, string expected)
    {
        var regex = new Regex(TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(timeoutTicks));
        var actual = regex.SafeReplace(input, "Replaced");
        actual.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow(MatchingPath, 1, false)]
    [DataRow(MatchingPath, 1_000_000, true)]
    [DataRow("äöü", 1, false)]
    [DataRow("äöü", 1_000_000, false)]
    public void SafeRegex_IsMatch_Timeout(string input, long timeoutTicks, bool isMatch)
    {
        var actual = SafeRegex.IsMatch(input, TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(timeoutTicks));
        actual.Should().Be(isMatch);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void SafeRegex_IsMatch_TimeoutFallback(bool fallback)
    {
        var actual = SafeRegex.IsMatch(MatchingPath, TimeoutPattern, RegexOptions.None, TimeSpan.FromTicks(1), fallback);
        actual.Should().Be(fallback);
    }
}
