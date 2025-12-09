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
    public static Dictionary<Type, Strategy> Build(TypeDescriptor[] baseline, TypeDescriptor[] latest)
    {
        var ret = new Dictionary<Type, Strategy>();
        var skip = new SkipStrategy();
        foreach (var type in latest)
        {
            if (IsSkipped(type.Type))
            {
                ret.Add(type.Type, skip);
            }
            // ToDo: SyntaxNodeStrategy
            // ToDo: IOperationStrategy
            // ToDo: StaticClassStrategy when (type.IsSealed && type.IsAbstract)
            // ToDo: TypeStrategy, or ClassStrategy / StructStrategy / InterfaceStrategy?
            // ToDo: EnumStrategy
        }
        return ret;
    }

    private static bool IsSkipped(Type type) =>
        type.IsNested
        || type.IsGenericType
        || typeof(Delegate).IsAssignableFrom(type);
}
