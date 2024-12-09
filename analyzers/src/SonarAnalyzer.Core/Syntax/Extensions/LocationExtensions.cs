/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Syntax.Extensions;

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
