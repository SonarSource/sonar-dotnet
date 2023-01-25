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

namespace SonarAnalyzer.Rules;

public abstract class CommentsShouldNotBeEmptyBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4663";

    protected abstract bool IsValidTriviaType(SyntaxTrivia trivia);
    protected abstract string GetCommentText(SyntaxTrivia trivia);

    protected override string MessageFormat => "Remove this empty comment";

    protected CommentsShouldNotBeEmptyBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterTreeAction(Language.GeneratedCodeRecognizer, c =>
        {
            foreach (var token in c.Tree.GetRoot().DescendantTokens())
            {
                CheckTrivia(c, token.LeadingTrivia);
                CheckTrivia(c, token.TrailingTrivia);
            }
        });

    protected void CheckTrivia(SonarSyntaxTreeReportingContext context, IEnumerable<SyntaxTrivia> trivia)
    {
        foreach (var trivium in trivia.Where(ShouldReport))
        {
            context.ReportIssue(Diagnostic.Create(Rule, trivium.GetLocation()));
        }

        bool ShouldReport(SyntaxTrivia trivia)
        => IsValidTriviaType(trivia) && string.IsNullOrEmpty(GetCommentText(trivia));
    }
}
