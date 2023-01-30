/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules;

public abstract class CommentsShouldNotBeEmptyBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4663";

    protected abstract bool IsValidTriviaType(SyntaxTrivia trivia);
    protected abstract string GetCommentText(SyntaxTrivia trivia);

    protected abstract bool IsSimpleComment(SyntaxTrivia trivia);
    protected abstract bool IsEndOfLine(SyntaxTrivia trivia);
    protected abstract bool IsWhitespace(SyntaxTrivia trivia);

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
        foreach (var partition in Partition(trivia)?.Where(ShouldReport))
        {
            var start = partition.First().GetLocation().SourceSpan.Start;
            var end = partition.Last().GetLocation().SourceSpan.End;

            var location = Location.Create(context.Tree, TextSpan.FromBounds(start, end));
            context.ReportIssue(Diagnostic.Create(Rule, location));
        }
    }

    protected List<List<SyntaxTrivia>>? Partition(IEnumerable<SyntaxTrivia> trivia)
    {
        var res = new Lazy<List<List<SyntaxTrivia>>>();
        var current = new Lazy<List<SyntaxTrivia>>();
        var firstEndOfLineFound = false;

        foreach (var trivium in trivia)
        {
            if (IsWhitespace(trivium))
            {
                continue;
            }

            if (IsSimpleComment(trivium)) // put it on the current block of "//"
            {
                current.Value.Add(trivium);
                firstEndOfLineFound = false;
                continue;
            }

            // This is for the case, of two different comment types, for example:
            // //
            // ///
            if (IsValidTriviaType(trivium)) // valid but not "//", because of the upper if
            {
                CloseCurrentPartition();
                // all comments except single-line comments are parsed as a block already.
                current.Value.Add(trivium);
                CloseCurrentPartition();
            }
            // This handles an empty line, for example:
            // // some comment \n <- EOL found, set to true
            // //  \n <- EOL is set to false at CommentTrivia, set to true after it
            // // some other comment <- EOL is set to false at CommentTrivia, set to true after it
            // \n <- EOL found, is already true, closes current partition
            else if (IsEndOfLine(trivium))
            {
                if (firstEndOfLineFound)
                {
                    CloseCurrentPartition();
                }
                else
                {
                    firstEndOfLineFound = true;
                }
            }
            else
            {
                CloseCurrentPartition();
            }
        }

        res.Value.Add(current.Value);
        return res.Value;

        void CloseCurrentPartition()
        {
            if (current.Value.Count > 0)
            {
                res.Value.Add(current.Value);
                current = new();
            }
            firstEndOfLineFound = false;
        }
    }

    private bool ShouldReport(IEnumerable<SyntaxTrivia> trivia) =>
        trivia.Any() && trivia.All(x => string.IsNullOrWhiteSpace(GetCommentText(x)));
}
