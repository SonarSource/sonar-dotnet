﻿/*
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

using System.Text;

namespace SonarAnalyzer.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// Splits the input string to the list of words.
        ///
        /// Sequence of upper case letters is considered as single word.
        ///
        /// For example:
        /// <list type="table">
        /// <item>thisIsAName => ["THIS", "IS", "A", "NAME"]</item>
        /// <item>ThisIsSMTPName => ["THIS", "IS", "SMTP", "NAME"]</item>
        /// <item>bin2hex => ["BIN", "HEX"]</item>
        /// <item>HTML => ["HTML"]</item>
        /// <item>SOME_value => ["SOME", "VALUE"]</item>
        /// <item>PEHeader => ["PE", "HEADER"]</item>
        /// </list>
        /// </summary>
        /// <param name="name">A string containing words.</param>
        /// <returns>A list of words (all uppercase) contained in the string.</returns>
        public static IEnumerable<string> SplitCamelCaseToWords(this string name)
        {
            if (name == null)
            {
                yield break;
            }

            var currentWord = new StringBuilder();
            var hasLower = false;

            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (!char.IsLetter(c))
                {
                    if (currentWord.Length > 0)
                    {
                        yield return currentWord.ToString();
                        currentWord.Clear();
                        hasLower = false;
                    }
                    continue;
                }

                if (char.IsUpper(c)
                    && currentWord.Length > 0
                    && (hasLower || IsFollowedByLower(i)))
                {
                    yield return currentWord.ToString();
                    currentWord.Clear();
                    hasLower = false;
                }

                currentWord.Append(char.ToUpperInvariant(c));
                hasLower = hasLower || char.IsLower(c);
            }

            if (currentWord.Length > 0)
            {
                yield return currentWord.ToString();
            }

            bool IsFollowedByLower(int i) => i + 1 < name.Length && char.IsLower(name[i + 1]);
        }
    }
}
