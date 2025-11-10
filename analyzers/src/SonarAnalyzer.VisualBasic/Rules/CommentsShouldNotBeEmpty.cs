/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class CommentsShouldNotBeEmpty : CommentsShouldNotBeEmptyBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool IsValidTriviaType(SyntaxTrivia trivia) =>
        trivia.IsComment();

    protected override string GetCommentText(SyntaxTrivia trivia) =>
        trivia.Kind() switch
        {
            SyntaxKind.CommentTrivia => GetText(trivia),
            SyntaxKind.DocumentationCommentTrivia => GetDocumentationText(trivia),
        };

    // '
    private static string GetText(SyntaxTrivia trivia)
        => trivia.ToString().Trim().Substring(1);

    // '''
    private static string GetDocumentationText(SyntaxTrivia trivia)
    {
        var stringBuilder = new StringBuilder();
        foreach (var line in trivia.ToFullString().Split(Constants.LineTerminators, StringSplitOptions.None))
        {
            var trimmedLine = line.TrimStart(null);
            trimmedLine = trimmedLine.StartsWith("'''")
                ? trimmedLine.Substring(3).Trim()
                : trimmedLine.TrimEnd(null);
            stringBuilder.Append(trimmedLine);
        }
        return stringBuilder.ToString();
    }
}
