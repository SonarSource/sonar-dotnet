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

public class SeparatedSyntaxListStrategy : Strategy
{
    private readonly string type;
    private readonly Strategy typeArgument;

    public SeparatedSyntaxListStrategy(Type latest, Strategy typeArgument) : base(latest)
    {
        type = latest.Name.Replace("`1", "Wrapper");
        this.typeArgument = typeArgument;
    }

    public override string ReturnTypeSnippet() =>
        $"{type}<{typeArgument.ReturnTypeSnippet()}>";

    public override string ToConversionSnippet(string from) =>
        from;

    public override string CompiletimeTypeSnippet() =>
        ReturnTypeSnippet();

    public override string PropertyAccessorInitializerSnippet(string compiletimeType, string propertyName) =>
        $"LightupHelpers.CreateSeparatedSyntaxListPropertyAccessor<{compiletimeType}, {typeArgument.ReturnTypeSnippet()}>(WrappedType, nameof({propertyName}))";

    public override string Generate(StrategyModel model) => null;
}
