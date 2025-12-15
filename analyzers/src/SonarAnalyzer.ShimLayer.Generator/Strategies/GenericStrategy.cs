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

public class GenericStrategy : Strategy
{
    // ToDo: Solve this from StrategyModel instead of hardcoding
    private static readonly HashSet<string> SubstitutedTypeArguments = [    // These generic arguments of (Separated)SyntaxList and do not exist in Roslyn 1.3.2
        "AllowsConstraintSyntax",
        "CollectionElementSyntax",
        "FunctionPointerParameterSyntax",
        "FunctionPointerUnmanagedCallingConventionSyntax",
        "PatternSyntax",
        "VariableDesignationSyntax",
        "SubpatternSyntax",
        "SwitchExpressionArmSyntax",
        "TupleElementSyntax"
    ];

    private readonly string type;
    private readonly bool isWrapped;
    private readonly string genericArgumentType;

    public GenericStrategy(Type latest) : base(latest)
    {
        isWrapped = latest.GenericTypeArguments.Any(x => SubstitutedTypeArguments.Contains(x.Name));
        type = latest.Name.Replace("`1", null) + (isWrapped ? "Wrapper" : null);
        genericArgumentType = latest.GenericTypeArguments.Single().Name + (isWrapped ? "Wrapper" : null);
    }

    public override string ReturnTypeSnippet() =>
        $"{type}<{genericArgumentType}>";

    public override string ToConversionSnippet(string from) =>
        from;

    public override string CompiletimeTypeSnippet() =>
        ReturnTypeSnippet();

    public override string Generate(StrategyModel model) => null;

    public override string PropertyAccessorInitializerSnippet(string compiletimeType, string propertyName) =>
        isWrapped
            ? $"LightupHelpers.CreateSeparatedSyntaxListPropertyAccessor<{compiletimeType}, {genericArgumentType}>(WrappedType, nameof({propertyName}))"
            : base.PropertyAccessorInitializerSnippet(compiletimeType, propertyName);
}
