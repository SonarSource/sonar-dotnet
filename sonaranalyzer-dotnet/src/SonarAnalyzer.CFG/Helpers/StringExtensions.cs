/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Text;

namespace SonarAnalyzer.Helpers
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Splits the input string to the list of words.
        ///
        /// Letters and consecutive capital letters are ignored.
        /// For example:
        /// thisIsAName => this is a name
        /// ThisIsIt => this is it
        /// bin2hex => bin hex
        /// HTML => h t m l
        /// PEHeader => p e header
        /// </summary>
        /// <param name="name">A string containing words.</param>
        /// <returns>A list of words (all lowercase) contained in the string.</returns>
        public static IEnumerable<string> SplitCamelCaseToWords(this string name)
        {
            if (name == null)
            {
                yield break;
            }

            var currentWord = new StringBuilder();

            foreach (var c in name)
            {
                if (!char.IsLetter(c))
                {
                    if (currentWord.Length > 0)
                    {
                        yield return currentWord.ToString();
                        currentWord.Clear();
                    }
                    continue;
                }

                if (char.IsUpper(c) && currentWord.Length > 0)
                {
                    yield return currentWord.ToString();
                    currentWord.Clear();
                }

                currentWord.Append(char.ToUpperInvariant(c));
            }

            if (currentWord.Length > 0)
            {
                yield return currentWord.ToString();
            }
        }
    }
}
