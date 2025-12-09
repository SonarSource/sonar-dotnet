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

namespace SonarAnalyzer.ShimLayer.Generator.Model;

public static class ModelBuilder
{
    private static readonly SkipStrategy Skip = new();

    public static Dictionary<Type, Strategy> Build(TypeDescriptor[] latest, TypeDescriptor[] baseline)
    {
        var baselineMap = baseline.ToDictionary(x => x.Type.FullName, x => x);
        return latest.ToDictionary(x => x.Type, x => CreateStrategy(x, baselineMap.TryGetValue(x.Type.FullName, out var baselineType) ? baselineType : null));
    }

    private static Strategy CreateStrategy(TypeDescriptor latest, TypeDescriptor baseline)
    {
        if (IsSkipped(latest.Type))
        {
            return Skip;
        }
        else if (typeof(SyntaxNode).IsAssignableFrom(latest.Type))
        {
            return new SyntaxNodeStrategy(latest.Type, []); // ToDo: Resolve members compared to baseline
        }
        // ToDo: IOperationStrategy
        // ToDo: StaticClassStrategy when (type.IsSealed && type.IsAbstract)
        // ToDo: EnumStrategy
        // ToDo: TypeStrategy, or ClassStrategy / StructStrategy / InterfaceStrategy?
        else
        {
            return null;    // ToDo: Throw NotSupportedException, there should be nothing left after explicitly handling all known cases
        }
    }

    private static bool IsSkipped(Type type) =>
        type.IsNested
        || type.IsGenericType
        || typeof(Delegate).IsAssignableFrom(type);
}
