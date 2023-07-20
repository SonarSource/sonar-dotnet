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

namespace SonarAnalyzer.Rules
{
    public abstract class CommentKeywordBase : SonarDiagnosticAnalyzer
    {
        private const string ToDoKeyword = "TODO";
        protected const string ToDoDiagnosticId = "S1135";
        protected const string ToDoMessageFormat = "Complete the task associated to this '" + ToDoKeyword + "' comment.";

        private const string FixMeKeyword = "FIXME";
        protected const string FixMeDiagnosticId = "S1134";
        protected const string FixMeMessageFormat = "Take the required action to fix the issue indicated by this '" + FixMeKeyword + "' comment.";

        private readonly DiagnosticDescriptor toDoRule;
        private readonly DiagnosticDescriptor fixMeRule;

        protected abstract ILanguageFacade Language { get; }
        protected abstract bool IsComment(SyntaxTrivia trivia);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        protected CommentKeywordBase()
        {
            toDoRule = Language.CreateDescriptor(ToDoDiagnosticId, ToDoMessageFormat);
            fixMeRule = Language.CreateDescriptor(FixMeDiagnosticId, FixMeMessageFormat);
            SupportedDiagnostics = ImmutableArray.Create(toDoRule, fixMeRule);
        }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterTreeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    var comments = c.Tree.GetRoot().DescendantTrivia()
                        .Where(trivia => IsComment(trivia));

                    foreach (var comment in comments)
                    {
                        foreach (var location in GetKeywordLocations(c.Tree, comment, ToDoKeyword))
                        {
                            c.ReportIssue(CreateDiagnostic(toDoRule, location));
                        }

                        foreach (var location in GetKeywordLocations(c.Tree, comment, FixMeKeyword))
                        {
                            c.ReportIssue(CreateDiagnostic(fixMeRule, location));
                        }
                    }
                });

        private static IEnumerable<Location> GetKeywordLocations(SyntaxTree tree, SyntaxTrivia comment, string word)
        {
            var text = comment.ToString();

            return AllIndexesOf(text, word)
                .Where(i => IsWordAt(text, i, word.Length))
                .Select(
                    i =>
                    {
                        var startLocation = comment.SpanStart + i;
                        var location = Location.Create(tree, TextSpan.FromBounds(startLocation, startLocation + word.Length));

                        return location;
                    });
        }

        private static IEnumerable<int> AllIndexesOf(string text, string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            var index = 0;
            while ((index = text.IndexOf(value, index, text.Length - index, comparisonType)) != -1)
            {
                yield return index;
                index += value.Length;
            }
        }

        private static bool IsWordAt(string text, int index, int count)
        {
            var leftBoundary = true;
            if (index > 0)
            {
                leftBoundary = !char.IsLetterOrDigit(text[index - 1]);
            }

            var rightBoundary = true;
            var rightOffset = index + count;
            if (rightOffset < text.Length)
            {
                rightBoundary = !char.IsLetterOrDigit(text[rightOffset]);
            }

            return leftBoundary && rightBoundary;
        }
    }
}
