/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Extensions;

public static class LocationExtensions
{
    public static FileLinePositionSpan GetMappedLineSpanIfAvailable(this Location location) =>
        GeneratedCodeRecognizer.IsRazorGeneratedFile(location.SourceTree)
            ? location.GetMappedLineSpan()
            : location.GetLineSpan();

    public static Location EnsureMappedLocation(this Location location)
    {
        if (location is null || !GeneratedCodeRecognizer.IsRazorGeneratedFile(location.SourceTree))
        {
            return location;
        }

        var lineSpan = location.GetMappedLineSpan();

        return Location.Create(lineSpan.Path, location.SourceSpan, lineSpan.Span);
    }

    public static int StartLine(this Location location) =>
        location.GetLineSpan().StartLinePosition.Line;

    public static int EndLine(this Location location) =>
        location.GetLineSpan().EndLinePosition.Line;

    public static bool IsValid(this Location location, Compilation compilation) =>
        location.Kind != LocationKind.SourceFile || compilation.ContainsSyntaxTree(location.SourceTree);

    public static SecondaryLocation ToSecondary(this Location location, string message = null) =>
        new(location, message);
}
