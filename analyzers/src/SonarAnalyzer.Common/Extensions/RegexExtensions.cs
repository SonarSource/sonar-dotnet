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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Extensions;

public static class RegexExtensions
{
    private static readonly MatchCollection EmptyMatchCollection = Regex.Matches(string.Empty, "a", RegexOptions.None, RegexConstants.DefaultTimeout);

    /// <summary>
    /// Matches the input to the regex. Returns <see cref="Match.Empty" /> in case of an <see cref="RegexMatchTimeoutException" />.
    /// </summary>
    public static Match SafeMatch(this Regex regex, string input)
    {
        try
        {
            return regex.Match(input);
        }
        catch (RegexMatchTimeoutException)
        {
            return Match.Empty;
        }
    }

    /// <summary>
    /// Matches the input to the regex. Returns <see langword="false" /> in case of an <see cref="RegexMatchTimeoutException" />.
    /// </summary>
    public static bool SafeIsMatch(this Regex regex, string input) =>
        regex.SafeIsMatch(input, false);

    /// <summary>
    /// Matches the input to the regex. Returns <paramref name="timeoutFallback"/> in case of an <see cref="RegexMatchTimeoutException" />.
    /// </summary>
    public static bool SafeIsMatch(this Regex regex, string input, bool timeoutFallback)
    {
        try
        {
            return regex.IsMatch(input);
        }
        catch (RegexMatchTimeoutException)
        {
            return timeoutFallback;
        }
    }

    /// <summary>
    /// Matches the input to the regex. Returns an empty <see cref="MatchCollection" /> in case of an <see cref="RegexMatchTimeoutException" />.
    /// </summary>
    public static MatchCollection SafeMatches(this Regex regex, string input)
    {
        try
        {
            var res = regex.Matches(input);
            _ = res.Count; // MatchCollection is lazy. Accessing "Count" executes the regex and caches the result
            return res;
        }
        catch (RegexMatchTimeoutException)
        {
            return EmptyMatchCollection;
        }
    }
}

public static class SafeRegex
{
    /// <summary>
    /// Matches the input to the regex. Returns <see langword="false" /> in case of an <see cref="RegexMatchTimeoutException" />.
    /// </summary>
    public static bool IsMatch(string input, string pattern) =>
        IsMatch(input, pattern, RegexOptions.None);

    /// <inheritdoc cref="IsMatch(string, string)"/>
    public static bool IsMatch(string input, string pattern, RegexOptions options) =>
        IsMatch(input, pattern, options, RegexConstants.DefaultTimeout);

    /// <inheritdoc cref="IsMatch(string, string)"/>
    public static bool IsMatch(string input, string pattern, RegexOptions options, TimeSpan matchTimeout)
    {
        try
        {
            return Regex.IsMatch(input, pattern, options, matchTimeout);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
