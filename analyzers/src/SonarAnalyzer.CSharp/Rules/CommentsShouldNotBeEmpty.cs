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

using System.Text;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommentsShouldNotBeEmpty : CommentsShouldNotBeEmptyBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override string GetCommentText(SyntaxTrivia trivia) =>
        trivia.Kind() switch
        {
            SyntaxKind.SingleLineCommentTrivia => GetSingleLineText(trivia),
            SyntaxKind.MultiLineCommentTrivia => GetMultiLineText(trivia),
            SyntaxKind.SingleLineDocumentationCommentTrivia => GetSingleLineDocumentationText(trivia),
            SyntaxKind.MultiLineDocumentationCommentTrivia => GetMultiLineDocumentationText(trivia),
        };

    // //
    private static string GetSingleLineText(SyntaxTrivia trivia) =>
        trivia.ToString().Trim().Substring(2);

    // ///
    private static string GetSingleLineDocumentationText(SyntaxTrivia trivia)
    {
        var stringBuilder = new StringBuilder();
        foreach (var line in trivia.ToFullString().Split(MetricsBase.LineTerminators, StringSplitOptions.None))
        {
            var trimmedLine = line.TrimStart(null);
            trimmedLine = trimmedLine.StartsWith("///")
                ? trimmedLine.Substring(3).Trim()
                : trimmedLine.TrimEnd(null);
            stringBuilder.Append(trimmedLine);
        }
        return stringBuilder.ToString();
    }

    // /* */
    private static string GetMultiLineText(SyntaxTrivia trivia) =>
        ParseMultiLine(trivia.ToString(), 2); // Length of "/*"

    // /** */
    private static string GetMultiLineDocumentationText(SyntaxTrivia trivia) =>
        ParseMultiLine(trivia.ToFullString(), 3); // Length of "/**"

    private static string ParseMultiLine(string commentText, int initialTrimSize)
    {
        commentText = commentText.Trim().Substring(initialTrimSize);
        if (commentText.EndsWith("*/", StringComparison.Ordinal)) // Might be unclosed, still reported
        {
            commentText = commentText.Substring(0, commentText.Length - 2);
        }

        var stringBuilder = new StringBuilder();
        foreach (var line in commentText.Split(MetricsBase.LineTerminators, StringSplitOptions.None))
        {
            var trimmedLine = line.TrimStart(null);
            if (trimmedLine.StartsWith("*", StringComparison.Ordinal))
            {
                trimmedLine = trimmedLine.TrimStart('*');
            }

            stringBuilder.Append(trimmedLine.Trim());
        }
        return stringBuilder.ToString();
    }
}
