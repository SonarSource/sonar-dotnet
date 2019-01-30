/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class CommentKeywordBase : SonarDiagnosticAnalyzer
    {
        private const string TodoKeyword = "TODO";
        protected const string TodoDiagnosticId = "S1135";
        protected const string TodoMessageFormat = "Complete the task associated to this '" + TodoKeyword + "' comment.";

        private const string FixMeKeyword = "FIXME";
        protected const string FixMeDiagnosticId = "S1134";
        protected const string FixMeMessageFormat = "Take the required action to fix the issue indicated by this '" + FixMeKeyword + "' comment.";

        protected abstract DiagnosticDescriptor TodoDiagnostic { get; }
        protected abstract DiagnosticDescriptor FixMeDiagnostic { get; }
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        protected CommentKeywordBase()
        {
            SupportedDiagnostics = ImmutableArray.Create(TodoDiagnostic, FixMeDiagnostic);
        }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var comments = c.Tree.GetRoot().DescendantTrivia()
                        .Where(trivia => IsComment(trivia));

                    foreach (var comment in comments)
                    {
                        foreach (var location in GetKeywordLocations(c.Tree, comment, TodoKeyword))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(TodoDiagnostic, location));
                        }

                        foreach (var location in GetKeywordLocations(c.Tree, comment, FixMeKeyword))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(FixMeDiagnostic, location));
                        }
                    }
                });
        }

        protected abstract bool IsComment(SyntaxTrivia trivia);

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
