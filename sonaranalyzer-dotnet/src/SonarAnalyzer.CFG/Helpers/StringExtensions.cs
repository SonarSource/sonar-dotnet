/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
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
