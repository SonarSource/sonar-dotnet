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

using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules;

public abstract class CommentsShouldNotBeEmptyBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4663";

    protected abstract string GetCommentText(SyntaxTrivia trivia);

    protected override string MessageFormat => "Remove this empty comment";

    protected CommentsShouldNotBeEmptyBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterTreeAction(Language.GeneratedCodeRecognizer, c =>
        {
            foreach (var token in c.Tree.GetRoot().DescendantTokens())
            {
                // Hotpath: Don't allocate the trivia enumerable if not needed
                if (token.HasLeadingTrivia)
                {
                    CheckTrivia(c, token.LeadingTrivia);
                }

                if (token.HasTrailingTrivia)
                {
                    CheckTrivia(c, token.TrailingTrivia);
                }
            }
        });

    private void CheckTrivia(SonarSyntaxTreeReportingContext context, IEnumerable<SyntaxTrivia> trivia)
    {
        var partitions = PartitionComments(trivia);
        if (partitions is null)
        {
            return;
        }

        foreach (var partition in partitions.Where(trivia => trivia.Any() && trivia.All(x => string.IsNullOrWhiteSpace(GetCommentText(x)))))
        {
            var location = partition.First().GetLocation();
            var secondary = partition.Skip(1).Select(x => x.GetLocation());
            context.ReportIssue(CreateDiagnostic(Rule, location, secondary));
        }
    }

    private List<List<SyntaxTrivia>> PartitionComments(IEnumerable<SyntaxTrivia> trivia)
    {
        // Hotpath: avoid unnecessary allocations
        List<List<SyntaxTrivia>> partitions = null;
        List<SyntaxTrivia> current = null;
        var firstEndOfLineFound = false;

        foreach (var trivium in trivia)
        {
            if (IsSimpleComment(trivium))
            {
                AddTriviaToPartition(ref current, trivium, ref firstEndOfLineFound);
            }
            // This is for the case, of two different comment types, for example:
            // //
            // ///
            else if (IsValidTriviaType(trivium)) // valid but not "//", because of the upper if
            {
                CloseCurrentPartition(ref current, ref partitions, ref firstEndOfLineFound);
                // all comments except single-line comments are parsed as a block already.
                AddTriviaToPartition(ref current, trivium, ref firstEndOfLineFound);
                CloseCurrentPartition(ref current, ref partitions, ref firstEndOfLineFound);
            }
            // This handles an empty line, for example:
            // // some comment \n <- EOL found, firstEndOfLineFound set to true
            // //  \n <- EOL is set to false at CommentTrivia, set to true after it
            // // some other comment <- EOL is set to false at CommentTrivia, set to true after it
            // \n <- EOL found, is already true, closes current partition
            else if (IsEndOfLine(trivium))
            {
                if (firstEndOfLineFound)
                {
                    CloseCurrentPartition(ref current, ref partitions, ref firstEndOfLineFound);
                }
                else
                {
                    firstEndOfLineFound = true;
                }
            }
            else if (!IsWhitespace(trivium))
            {
                CloseCurrentPartition(ref current, ref partitions, ref firstEndOfLineFound);
            }
        }

        CloseCurrentPartition(ref current, ref partitions, ref firstEndOfLineFound);
        return partitions;

        // Hotpath: Don't capture variables
        static void AddTriviaToPartition(ref List<SyntaxTrivia> current, SyntaxTrivia trivia, ref bool firstEndOfLineFound)
        {
            current ??= new();
            current.Add(trivia);
            firstEndOfLineFound = false;
        }

        static void CloseCurrentPartition(ref List<SyntaxTrivia> current, ref List<List<SyntaxTrivia>> partitions, ref bool firstEndOfLineFound)
        {
            if (current is { Count: > 0 })
            {
                partitions ??= new();
                partitions.Add(current);
                current = null;
            }

            firstEndOfLineFound = false;
        }
    }

    private bool IsValidTriviaType(SyntaxTrivia trivia) =>
        Language.Syntax.IsAnyKind(trivia, Language.SyntaxKind.CommentTrivia);

    private bool IsSimpleComment(SyntaxTrivia trivia) =>
        Language.Syntax.IsKind(trivia, Language.SyntaxKind.SimpleCommentTrivia);

    private bool IsEndOfLine(SyntaxTrivia trivia) =>
        Language.Syntax.IsKind(trivia, Language.SyntaxKind.EndOfLineTrivia);

    private bool IsWhitespace(SyntaxTrivia trivia) =>
        Language.Syntax.IsKind(trivia, Language.SyntaxKind.WhitespaceTrivia);
}
