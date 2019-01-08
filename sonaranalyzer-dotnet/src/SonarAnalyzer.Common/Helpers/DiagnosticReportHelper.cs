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
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers
{
    public static class DiagnosticReportHelper
    {
        #region Line Number

        public static int GetLineNumberToReport(this SyntaxNode self)
        {
            return self.GetLocation().GetLineNumberToReport();
        }

        public static int GetLineNumberToReport(this Diagnostic self)
        {
            return self.Location.GetLineNumberToReport();
        }

        public static int GetLineNumberToReport(this Location self)
        {
            return self.GetLineSpan().StartLinePosition.GetLineNumberToReport();
        }

        public static int GetLineNumberToReport(this LinePosition self)
        {
            return self.Line + 1;
        }

        #endregion Line Number

        public static string ToSentence(this IEnumerable<string> words,
            bool quoteWords = false,
            LastJoiningWord lastJoiningWord = LastJoiningWord.And)
        {
            var wordCollection = words as ICollection<string> ?? words.ToList();
            var singleQuoteOrBlank = quoteWords ? "'" : "";

            switch (wordCollection.Count)
            {
                case 0:
                    return null;

                case 1:
                    return string.Concat(singleQuoteOrBlank, wordCollection.First(), singleQuoteOrBlank);

                default:
                    return new StringBuilder(singleQuoteOrBlank)
                        .Append(string.Join($"{singleQuoteOrBlank}, {singleQuoteOrBlank}",
                            wordCollection.Take(wordCollection.Count - 1)))
                        .Append(singleQuoteOrBlank)
                        .Append(" ")
                        .Append(lastJoiningWord.ToString().ToLower())
                        .Append(" ")
                        .Append(singleQuoteOrBlank)
                        .Append(wordCollection.Last())
                        .Append(singleQuoteOrBlank)
                        .ToString();
            }
        }
    }

    public enum LastJoiningWord { And, Or }
}
