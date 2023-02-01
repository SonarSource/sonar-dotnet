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
#nullable enable

using System.Text.RegularExpressions;

namespace SonarAnalyzer.RegularExpressions;

public sealed class RegexTree
{
    // NonBacktracking was added in .NET 7
    // See: https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regexoptions?view=net-7.0
    public static readonly RegexOptions NonBacktracking = (RegexOptions)1024;

    private static readonly RegexOptions TestMask = RegexOptions.None
        | RegexOptions.IgnoreCase
        | RegexOptions.Multiline
        | RegexOptions.ExplicitCapture
        | RegexOptions.Singleline
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.RightToLeft
        | RegexOptions.ECMAScript
        | RegexOptions.CultureInvariant
        | NonBacktracking;

    private RegexTree(Regex? regex, string pattern, RegexOptions? options, Exception? parseError)
    {
        Regex = regex;
        Pattern = pattern;
        Options = options;
        ParseError = parseError;
    }

    public Regex? Regex { get; }
    public string Pattern { get; }
    public RegexOptions? Options { get; }
    public Exception? ParseError { get; }

    public bool MatchesEmptyString() =>
        Regex is { } && Regex.IsMatch(string.Empty);

    public bool AllowsBacktracking() =>
        Options.HasValue && !Options.Value.HasFlag(NonBacktracking);

    public bool ContainsAdjecentWhitespace() =>
        Regex is { }
        && !Options.GetValueOrDefault().HasFlag(RegexOptions.IgnorePatternWhitespace)
        && Pattern.Contains("  ");

    public static RegexTree Parse(string pattern, RegexOptions? options)
    {
        try
        {
            return new RegexTree(new(pattern, options.GetValueOrDefault() & TestMask), pattern, options, null);
        }
        catch (Exception x)
        {
            return new RegexTree(null, pattern, options, x);
        }
    }
}
