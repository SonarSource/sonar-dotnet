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

using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.Extensions;

public static class LocationExtensions
{
    public static FileLinePositionSpan GetMappedLineSpanIfAvailable(this Location location) =>
        GeneratedCodeRecognizer.IsRazorGeneratedFile(location.SourceTree) ? location.GetMappedLineSpan() : location.GetLineSpan();

    public static FileLinePositionSpan GetMappedLineSpanIfAvailable(this Location location, SyntaxToken token)
    {
        var node = token.Parent;
        if (GeneratedCodeRecognizer.IsRazorGeneratedFile(location.SourceTree))
        {
            var mappedLocation = location.GetMappedLineSpan();
            if (mappedLocation.HasMappedPath)
            {
                var lineDirective = FindLineDirecitive(node);
                if (LineSpanDirectiveTriviaSyntaxWrapper.IsInstance(lineDirective)
                    && (LineSpanDirectiveTriviaSyntaxWrapper)lineDirective is var lineSpanDirective
                    && lineSpanDirective.CharacterOffset.ValueText is var stringValue
                    && int.TryParse(stringValue, out var numericValue)
                    && numericValue >= location.GetLineSpan().Span.End.Character)
                {
                    return location.GetLineSpan();
                }
            }
            return mappedLocation;
        }
        return location.GetLineSpan();
    }

    private static SyntaxNode FindLineDirecitive(SyntaxNode node)
    {
        while (node != null)
        {
            if (LineDirective(node) is { } lineDirective)
            {
                return lineDirective;
            }
            var directive = FindLineDirectiveOnSameLevel(node);

            if (directive != null)
            {
                return directive;
            }

            node = node.Parent;
        }
        return null;
    }

    private static SyntaxNode FindLineDirectiveOnSameLevel(SyntaxNode node)
    {
        var childNodes = node.Parent.ChildNodes().ToArray();
        var index = -1;
        for (var i = 0; i < childNodes.Count(); i++)
        {
            if (childNodes[i] == node)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            return null;
        }

        for (var j = index; j >= 0; j--)
        {
            if (LineDirective(childNodes[j]) is { } lineDirective)
            {
                return lineDirective;
            }
        }

        return null;
    }

    private static SyntaxNode LineDirective(SyntaxNode node) =>
        node.DescendantNodes(_ => true, true).FirstOrDefault(x => x.IsKind(SyntaxKind.LineDirectiveTrivia)
                                                                  || x.IsKind(SyntaxKindEx.LineSpanDirectiveTrivia));
}
