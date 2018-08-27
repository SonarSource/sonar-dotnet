/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    public abstract class CommentWordBase : SonarDiagnosticAnalyzer
    {
        protected abstract string Word { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(
                c =>
                {
                    var comments = c.Tree.GetCompilationUnitRoot().DescendantTrivia()
                        .Where(trivia => IsComment(trivia));

                    foreach (var comment in comments)
                    {
                        var text = comment.ToString();

                        foreach (var i in AllCaseInsensitiveIndexesOf(text, Word).Where(i => IsWordAt(text, i, Word.Length)))
                        {
                            var startLocation = comment.SpanStart + i;
                            var location = Location.Create(
                                c.Tree,
                                TextSpan.FromBounds(startLocation, startLocation + Word.Length));

                            c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], location));
                        }
                    }
                });
        }

        private static bool IsComment(SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
        }

        private static IEnumerable<int> AllCaseInsensitiveIndexesOf(string str, string value)
        {
            var index = 0;
            while ((index = str.IndexOf(value, index, str.Length - index, System.StringComparison.OrdinalIgnoreCase)) != -1)
            {
                yield return index;
                index += value.Length;
            }
        }

        private static bool IsWordAt(string str, int index, int count)
        {
            var leftBoundary = true;
            if (index > 0)
            {
                leftBoundary = !char.IsLetterOrDigit(str[index - 1]);
            }

            var rightBoundary = true;
            var rightOffset = index + count;
            if (rightOffset < str.Length)
            {
                rightBoundary = !char.IsLetterOrDigit(str[rightOffset]);
            }

            return leftBoundary && rightBoundary;
        }
    }
}
