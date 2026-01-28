/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.ShimLayer.Generator;

public static class Factory
{
    // Match 3 or more consecutive newlines (with optional whitespace-only lines between them) and replace with exactly 2 newlines.
    private static readonly Regex ExcessiveNewLines = new(@"\n(\s*\n){2,}", RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(100));

    public static IEnumerable<GeneratedFile> CreateAllFiles()
    {
        using var typeLoader = new TypeLoader();
        var model = ModelBuilder.Build(typeLoader.LoadLatest(), typeLoader.LoadBaseline());
        foreach (var strategy in model.ToArray())
        {
            if (strategy.Generate(model) is { } content)
            {
                var shortened = ExcessiveNewLines.Replace(content, "\n\n");
                yield return new($"{strategy.Latest.Name}.g.cs", shortened);
            }
        }
    }
}

public record GeneratedFile(string Name, string Content);
