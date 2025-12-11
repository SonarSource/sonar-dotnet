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

namespace SonarAnalyzer.ShimLayer.Generator.Strategies;

// TODO: Move this logic to Factory
internal class WrapperGenerator
{
    public GeneratedFile GenerateWrapper(Type latest, StrategyModel model)
    {
        if (model.TryGetValue(latest, out var strategy))
        {
            return strategy.Generate(model) is { } content
                ? new($"{latest.Name}Wrapper.g.cs", content)
                : null;
        }
        else
        {
            throw new KeyNotFoundException($"No strategy found for type {latest.FullName}.");
        }
    }
}
