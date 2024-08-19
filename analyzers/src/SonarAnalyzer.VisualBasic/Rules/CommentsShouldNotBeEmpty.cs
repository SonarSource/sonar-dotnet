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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class CommentsShouldNotBeEmpty : CommentsShouldNotBeEmptyBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

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
        foreach (var line in trivia.ToFullString().Split(MetricsBase.LineTerminators, StringSplitOptions.None))
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
