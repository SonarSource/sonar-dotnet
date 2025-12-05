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

using System.Diagnostics.CodeAnalysis;

namespace SonarAnalyzer.ShimLayer.Generator;

[Generator]
[ExcludeFromCodeCoverage]
public class ShimLayerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterSourceOutput(
            context.ParseOptionsProvider.Select((_, _) => context.GetType().Assembly.FullName), // Any simple provider, return Roslyn version
            (context, _) =>
            {
                foreach (var file in Factory.CreateAllFiles())
                {
                    context.AddSource(file.Name, file.Content);
                }
            });
}
