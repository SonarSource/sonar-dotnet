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

namespace SonarAnalyzer.Core.Extensions;

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

    /// <summary>
    /// Replaces <paramref name="input"/> with <paramref name="replacement"/> in <paramref name="regex"/>.
    /// Returns the original <paramref name="input"/> in case of an <see cref="RegexMatchTimeoutException" />.
    /// </summary>
    public static string SafeReplace(this Regex regex, string input, string replacement)
    {
        try
        {
            return regex.Replace(input, replacement);
        }
        catch (RegexMatchTimeoutException)
        {
            return input;
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
