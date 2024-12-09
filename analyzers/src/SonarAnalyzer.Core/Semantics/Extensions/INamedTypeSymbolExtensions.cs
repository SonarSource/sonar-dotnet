/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Core.Semantics.Extensions;

public static class INamedTypeSymbolExtensions
{
    public static bool IsTopLevelProgram(this INamedTypeSymbol symbol) =>
        TopLevelStatements.ProgramClassImplicitName.Contains(symbol.Name)
        && symbol.ContainingNamespace.IsGlobalNamespace
        && symbol.GetMembers(TopLevelStatements.MainMethodImplicitName).Any();

    public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamedTypeSymbol type)
    {
        if (type is null)
        {
            yield break;
        }

        yield return type;

        foreach (var nestedType in type.GetTypeMembers().SelectMany(GetAllNamedTypes))
        {
            yield return nestedType;
        }
    }
}
