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

using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.Helpers;

public interface IGlobPatternMatcher
{
    bool IsMatch(string pattern, string input);
}

public class GlobPatternMatcher : IGlobPatternMatcher
{
    public bool IsMatch(string pattern, string input) =>
        WildcardPattern.Create(pattern).Match(input);

    /// <summary>
    /// Copied from https://github.com/SonarSource/sonar-plugin-api/blob/a9bd7ff48f0f77811ed909070030678c443c975a/sonar-plugin-api/src/main/java/org/sonar/api/utils/WildcardPattern.java
    /// </summary>
    private sealed class WildcardPattern
    {
        private const string SpecialChars = "()[]^$.{}+|";
        private static readonly ConcurrentDictionary<string, WildcardPattern> Cache = new();
        internal readonly Regex Pattern;

        private WildcardPattern(string pattern, string directorySeparator) =>
            Pattern = new Regex(ToRegexp(pattern, directorySeparator), RegexOptions.None, TimeSpan.FromMilliseconds(100));

        private static string ToRegexp(string antPattern, string directorySeparator)
        {
            var escapedDirectorySeparator = '\\' + directorySeparator;
            var sb = new StringBuilder(antPattern.Length);

            sb.Append('^');

            var i = antPattern.StartsWith("/") || antPattern.StartsWith("\\") ? 1 : 0;
            while (i < antPattern.Length)
            {
                var ch = antPattern[i];

                if (SpecialChars.IndexOf(ch) != -1)
                {
                    // Escape regexp-specific characters
                    sb.Append('\\').Append(ch);
                }
                else if (ch == '*')
                {
                    if (i + 1 < antPattern.Length && antPattern[i + 1] == '*')
                    {
                        // Double asterisk
                        // Zero or more directories
                        if (i + 2 < antPattern.Length && IsSlash(antPattern[i + 2]))
                        {
                            sb.Append("(?:.*").Append(escapedDirectorySeparator).Append("|)");
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
                        // Single asterisk
                        // Zero or more characters excluding directory separator
                        sb.Append("[^").Append(escapedDirectorySeparator).Append("]*?");
                    }
                }
                else if (ch == '?')
                {
                    // Any single character excluding directory separator
                    sb.Append("[^").Append(escapedDirectorySeparator).Append("]");
                }
                else if (IsSlash(ch))
                {
                    // Directory separator
                    sb.Append(escapedDirectorySeparator);
                }
                else
                {
                    // Single character
                    sb.Append(ch);
                }

                i++;
            }

            sb.Append('$');

            return sb.ToString();
        }

        private static bool IsSlash(char ch) =>
            ch == '/' || ch == '\\';

        /**
         * Returns true if specified value matches this pattern.
         */
        public bool Match(string value)
        {
            value = value.TrimStart('/');
            value = value.TrimEnd('/');
            return Pattern.IsMatch(value);
        }

        /**
         * Creates pattern with "/" as a directory separator.
         * @see #create(string, string)
         */
        public static WildcardPattern Create(string pattern) =>
            Create(pattern, "/");

        /**
         * Creates pattern with specified separator for directories.
         * <p>
         * This is used to match Java-classes, i.e. <code>org.foo.Bar</code> against <code>org/**</code>.
         * <b>However usage of character other than "/" as a directory separator is misleading and should be avoided,
         * so method {@link #create(string)} is preferred over this one.</b>
         * <p>
         * Also note that no matter whether forward or backward slashes were used in the <code>antPattern</code>
         * the returned pattern will use <code>directorySeparator</code>.
         * Thus to match Windows-style path "dir\file.ext" against pattern "dir/file.ext" normalization should be performed.
         */
        public static WildcardPattern Create(string pattern, string directorySeparator) =>
            Cache.GetOrAdd(pattern + directorySeparator, k => new WildcardPattern(pattern, directorySeparator));
    }
}
