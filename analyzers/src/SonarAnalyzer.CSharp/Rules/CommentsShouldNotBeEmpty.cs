/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommentsShouldNotBeEmpty : CommentsShouldNotBeEmptyBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override void CheckTrivia(SonarSyntaxTreeReportingContext context, IEnumerable<SyntaxTrivia> trivia)
    {
        foreach (var trivium in trivia)
        {
            switch (trivium.Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                    CheckSingleLineTrivia(context, trivium);
                    break;
                case SyntaxKind.MultiLineCommentTrivia:                 // usecase: /* [many lines...] */
                    CheckMultiLineTrivia(context, trivium);
                    break;
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                    CheckSingleLineDocumentationTrivia(context, trivium);
                    break;
                case SyntaxKind.MultiLineDocumentationCommentTrivia:    // usecase: /**
                    CheckMultiLineDocumentationTrivia(context, trivium);
                    break;
                default:
                    break;
            }
        }
    }

    private void CheckSingleLineTrivia(SonarSyntaxTreeReportingContext context, SyntaxTrivia trivium)
    {
        if (string.CompareOrdinal(trivium.ToFullString().Trim(), "//") == 0)
        {
            context.ReportIssue(Diagnostic.Create(Rule, trivium.GetLocation()));
        }
    }

    private void CheckMultiLineTrivia(SonarSyntaxTreeReportingContext context, SyntaxTrivia trivium)
    {

    }

    private void CheckSingleLineDocumentationTrivia(SonarSyntaxTreeReportingContext context, SyntaxTrivia trivium)
    {
        var lines = trivium.ToFullString().Split(MetricsBase.LineTerminators, StringSplitOptions.None);
        foreach (var line in lines.Take(lines.Length - 1)) // last entry is empty string after newline
        {
            if (string.CompareOrdinal(line.Trim(), "///") != 0)
            {
                return;
            }
        }
        context.ReportIssue(Diagnostic.Create(Rule, trivium.GetLocation()));
    }

    private void CheckMultiLineDocumentationTrivia(SonarSyntaxTreeReportingContext context, SyntaxTrivia trivium)
    {

    }
}
