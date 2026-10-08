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

namespace SonarAnalyzer.ShimLayer.Generator.Strategies;

public class ImmutableArrayStrategy : Strategy
{
    private readonly string type;
    private readonly Strategy typeArgument;

    // We can use only those, that assign directly, or we convert them in AccessorFactory. We can remove the strategy list once we implement missing WrapStrategy conversions there
    public override bool IsSupported => typeArgument.IsSupported
                                        && typeArgument is ExtendStrategy
                                                            or IOperationStrategy
                                                            or NewEnumStrategy
                                                            or NoChangeStrategy
                                                            or OperationWrapStrategy
                                                            or PartialEnumStrategy
                                                            or PrimitiveStrategy
                                                            or TypeWrapStrategy;
    public override string TypeSnippet => $"{type}<{typeArgument.TypeSnippet}>";
    public override string CompiletimeTypeSnippet => TypeSnippet;

    public ImmutableArrayStrategy(Type latest, Strategy typeArgument) : base(latest)
    {
        type = latest.Name.Replace("`1", null);
        this.typeArgument = typeArgument;
    }

    protected override string GenerateCore(StrategyModel model) => null;
}
