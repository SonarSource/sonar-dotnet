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

using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.Helpers;

internal static class WildcardPatternMatcher
{
    private static readonly ConcurrentDictionary<string, Regex> Cache = new();

    public static bool IsMatch(string pattern, string input, bool timeoutFallbackResult) =>
        !(string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(input))
        && Cache.GetOrAdd(pattern, _ => new Regex(ToRegex(pattern), RegexOptions.None, RegexConstants.DefaultTimeout)) is var regex
        && regex.SafeIsMatch(input.Trim('/'), timeoutFallbackResult);

    /// <summary>
    /// Copied from https://github.com/SonarSource/sonar-plugin-api/blob/a9bd7ff48f0f77811ed909070030678c443c975a/sonar-plugin-api/src/main/java/org/sonar/api/utils/WildcardPattern.java.
    /// </summary>
    private static string ToRegex(string wildcardPattern)
    {
        var escapedDirectorySeparator = Regex.Escape(Path.DirectorySeparatorChar.ToString());
        var sb = new StringBuilder("^", wildcardPattern.Length);
        var i = IsSlash(wildcardPattern[0]) ? 1 : 0;
        while (i < wildcardPattern.Length)
        {
            var ch = wildcardPattern[i];
            if (ch == '*')
            {
                if (i + 1 < wildcardPattern.Length && wildcardPattern[i + 1] == '*')
                {
                    // Double asterisk - Zero or more directories
                    if (i + 2 < wildcardPattern.Length && IsSlash(wildcardPattern[i + 2]))
                    {
                        sb.Append($"(.*{escapedDirectorySeparator}|)");
                        i += 2;
                    }
                    else
                    {
                        sb.Append(".*");
                        i += 1;
                    }
                }
                else
                {
                    // Single asterisk - Zero or more characters excluding directory separator
                    sb.Append($"[^{escapedDirectorySeparator}]*?");
                }
            }
            else if (ch == '?')
            {
                // Any single character excluding directory separator
                sb.Append($"[^{escapedDirectorySeparator}]");
            }
            else if (IsSlash(ch))
            {
                sb.Append(escapedDirectorySeparator);
            }
            else
            {
                sb.Append(Regex.Escape(ch.ToString()));
            }
            i++;
        }
        return sb.Append('$').ToString();
    }

    private static bool IsSlash(char ch) =>
        ch == '/' || ch == '\\';
}
