/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommentsShouldNotBeEmpty : CommentsShouldNotBeEmptyBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool IsValidTriviaType(SyntaxTrivia trivia) =>
        trivia.IsComment();

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
        foreach (var line in trivia.ToFullString().Split(Constants.LineTerminators, StringSplitOptions.None))
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
        foreach (var line in commentText.Split(Constants.LineTerminators, StringSplitOptions.None))
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
