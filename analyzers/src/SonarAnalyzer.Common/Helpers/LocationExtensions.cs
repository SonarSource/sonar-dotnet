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

namespace SonarAnalyzer.Helpers
{
    public static class LocationExtensions
    {
        public static Location EnsureMappedLocation(this Location location)
        {
            if (!GeneratedCodeRecognizer.IsRazorGeneratedFile(location.SourceTree))
            {
                return location;
            }

            var lineSpan = location.GetMappedLineSpan();

            // var lines = File.ReadAllLines(lineSpan.Path);
            // var from = lines.Take(lineSpan.Span.Start.Line).Sum(x => x.Length) + lineSpan.Span.Start.Character;
            // var to = from + lineSpan.Span.End.Character;

            location = Location.Create(lineSpan.Path, location.SourceSpan, lineSpan.Span);
            return location;
        }
    }
}
