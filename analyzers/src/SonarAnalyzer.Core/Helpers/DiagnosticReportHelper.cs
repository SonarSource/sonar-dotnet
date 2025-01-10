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

using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers;

public static class DiagnosticReportHelper
{
    public static int GetLineNumberToReport(this SyntaxNode self) =>
        self.GetLocation().GetLineNumberToReport();

    public static int GetLineNumberToReport(this Diagnostic self) =>
        self.Location.GetLineNumberToReport();

    public static int GetLineNumberToReport(this Location self) =>
        self.GetMappedLineSpan().StartLinePosition.GetLineNumberToReport();

    public static int GetLineNumberToReport(this LinePosition self) =>
        self.Line + 1;

    public static int GetLineNumberToReport(this FileLinePositionSpan self) =>
        self.StartLinePosition.GetLineNumberToReport();

    public static string ToSentence(this IEnumerable<string> words,
        bool quoteWords = false,
        LastJoiningWord lastJoiningWord = LastJoiningWord.And)
    {
        var wordCollection = words as ICollection<string> ?? words.ToList();
        var singleQuoteOrBlank = quoteWords ? "'" : string.Empty;

        return wordCollection.Count switch
        {
            0 => null,
            1 => string.Concat(singleQuoteOrBlank, wordCollection.First(), singleQuoteOrBlank),
            _ => new StringBuilder(singleQuoteOrBlank)
                    .Append(string.Join($"{singleQuoteOrBlank}, {singleQuoteOrBlank}", wordCollection.Take(wordCollection.Count - 1)))
                    .Append(singleQuoteOrBlank)
                    .Append(" ")
                    .Append(lastJoiningWord.ToString().ToLower())
                    .Append(" ")
                    .Append(singleQuoteOrBlank)
                    .Append(wordCollection.Last())
                    .Append(singleQuoteOrBlank)
                    .ToString(),
        };
    }
}

public enum LastJoiningWord
{
    And,
    Or
}
