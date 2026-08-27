/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
            else
            {
                Strategy newStrategy = key switch
                {
                    { Namespace: "System.Reflection.Metadata" } => new SkipStrategy(key),  // Old Roslyn throws: Could not load 'System.Reflection.Metadata, Version=1.3.0.0, ...'}
                    { Name: "ImmutableArray`1" } when this[key.GenericTypeArguments.Single()] is OperationWrapStrategy typeArgument => new ImmutableArrayStrategy(key, typeArgument),
                    { Name: "SeparatedSyntaxList`1" } when this[key.GenericTypeArguments.Single()] is SyntaxNodeWrapStrategy typeArgument => new SeparatedSyntaxListStrategy(key, typeArgument),
                    { IsArray: true } => new ArrayStrategy(key, this[key.GetElementType()]),
                    { IsGenericType: true } => new GenericTypeStrategy(key, key.GenericTypeArguments.Select(x => this[x]).ToArray()),
                    // Primitive types can't be added in ModelBuilder, because typeof(int) (from RuntimeTypes module) is not equivalent to the Int32 we see here (from EcmaModule).
                    { Name: "Boolean" } => new PrimitiveStrategy(key, "bool"),
                    { Name: "Int32" } => new PrimitiveStrategy(key, "int"),
                    { Name: "String" } => new PrimitiveStrategy(key, "string"),
                    { Name: "Void" } => new PrimitiveStrategy(key, "void"),
                    _ => new NoChangeStrategy(key)
                };
                Add(key, newStrategy);
                return newStrategy;
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
