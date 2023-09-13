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

using SonarAnalyzer.Rules;

namespace SonarAnalyzer.Extensions;

public static class LocationExtensions
{
    public static FileLinePositionSpan GetMappedLineSpanIfAvailable(this Location location, ImmutableSortedSet<LineDirectiveEntry> lineDirectiveMap)
    {
        if (GeneratedCodeRecognizer.IsRazorGeneratedFile(location.SourceTree)
            && !lineDirectiveMap.IsEmpty)
        {
            var unmappedLocation = location.GetLineSpan().StartLinePosition.Line;
            var lineSpanIndex = -1;
            for (var i = 0; i < lineDirectiveMap.Count; i++)
            {
                if (lineDirectiveMap[i].LineNumber > unmappedLocation)
                {
                    lineSpanIndex = i - 1;
                    break;
                }
            }

            return lineSpanIndex != -1
                && LineSpanDirectiveTriviaSyntaxWrapper.IsInstance(lineDirectiveMap[lineSpanIndex].LineDirective)
                    && (LineSpanDirectiveTriviaSyntaxWrapper)lineDirectiveMap[lineSpanIndex].LineDirective is var lineSpanDirective
                    && lineSpanDirective.CharacterOffset.ValueText is var stringValue
                    && int.TryParse(stringValue, out var numericValue)
                    && numericValue >= location.GetLineSpan().Span.End.Character
                ? location.GetLineSpan()
                : location.GetMappedLineSpan();
        }
        return location.GetLineSpan();
    }
}
