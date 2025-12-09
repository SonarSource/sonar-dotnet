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

namespace SonarAnalyzer.ShimLayer.Generator;

internal static class Factory
{
    public static IEnumerable<GeneratedFile> CreateAllFiles()
    {
        using var typeLoader = new TypeLoader();
        var model = ModelBuilder.Build(typeLoader.LoadLatest(), typeLoader.LoadBaseline());

        yield return new(
            "Temporary.g.cs",
            """
            namespace SonarAnalyzer.ShimLayer;

            public static partial class Temporary
            {
                private const int ValueOfOneFromSourceGeneratedPartialClass = 1;
            }
            """);
    }
}

internal record GeneratedFile(string Name, string Content);
