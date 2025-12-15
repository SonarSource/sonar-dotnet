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

using System.Collections;

namespace SonarAnalyzer.ShimLayer.Generator.Model;

public class StrategyModel : IEnumerable<Strategy>
{
    private readonly Dictionary<Type, Strategy> strategies;

    public Strategy this[Type key]
    {
        get
        {
            if (strategies.TryGetValue(key, out var strategy))
            {
                return strategy;
            }
            else if (key.Name is "SyntaxList`1" or "SeparatedSyntaxList`1")
            {
                var genericStrategy = new GenericStrategy(key);
                Add(key, genericStrategy);
                return genericStrategy;
            }
            else
            {
                var defaultStrategy = new NoChangeStrategy(key);
                Add(key, defaultStrategy);
                return defaultStrategy;
            }
        }
    }

    public StrategyModel() =>
        strategies = [];

    public StrategyModel(Dictionary<Type, Strategy> strategies) =>
        this.strategies = strategies;

    public void Add(Type type, Strategy strategy) =>
        strategies.Add(type, strategy);

    public IEnumerator<Strategy> GetEnumerator() =>
        strategies.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
