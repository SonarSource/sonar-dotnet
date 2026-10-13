/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Syntax.Extensions;

internal static class SyntaxTreeExtensions
{
    private static readonly ConditionalWeakTable<SyntaxTree, IReadOnlyList<ConditionalCompilationRegion>> ConditionalCompilationRegionsCache = new();

    extension(SyntaxTree tree)
    {
        public IReadOnlyList<ConditionalCompilationRegion> ConditionalCompilationRegions => ConditionalCompilationRegionsCache.GetValue(tree, x => CollectConditionalCompilationRegions(x.GetRoot()));
    }

    private static IReadOnlyList<ConditionalCompilationRegion> CollectConditionalCompilationRegions(SyntaxNode root)
    {
        if (!root.ContainsDirectives)
        {
            return [];
        }
        var trivia = root.DescendantTrivia(descendIntoChildren: x => x.ContainsDirectives).ToList();
        // Collect across the tree: nested regions in active branches need not share a trivia run with their directives.
        var disabledSpans = trivia.Where(x => x.IsKind(SyntaxKind.DisabledTextTrivia)).Select(x => x.Span).ToList();
        return trivia.Where(x => x.IsKind(SyntaxKind.IfDirectiveTrivia))
            .Select(x => ConditionalCompilationRegion((DirectiveTriviaSyntax)x.GetStructure(), disabledSpans))
            .ToList();
    }

    private static ConditionalCompilationRegion ConditionalCompilationRegion(DirectiveTriviaSyntax ifDirective, List<TextSpan> disabledSpans)
    {
        var directives = ifDirective.GetRelatedDirectives();
        var range = new DirectiveRange(ifDirective, directives[directives.Count - 1]);
        // A missing #endif produces no final branch; disabled text beyond the last directive is deliberately excluded.
        var branches = directives.Zip(directives.Skip(1), (start, end) => new ConditionalCompilationBranch(new(start, end))).ToList();
        return new(range, disabledSpans.Where(x => range.Span.Contains(x)).ToList(), branches);
    }
}
