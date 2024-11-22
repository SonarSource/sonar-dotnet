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

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class IMethodSymbolExtensions
{
    public static bool IsModuleInitializer(this IMethodSymbol methodSymbol) =>
        methodSymbol.AnyAttributeDerivesFrom(KnownType.System_Runtime_CompilerServices_ModuleInitializerAttribute);

    public static bool IsGetTypeCall(this IMethodSymbol invokedMethod) =>
        invokedMethod.Name == nameof(Type.GetType)
        && !invokedMethod.IsStatic
        && invokedMethod.ContainingType is not null
        && IsObjectOrType(invokedMethod.ContainingType);

    public static SyntaxNode ImplementationSyntax(this IMethodSymbol method) =>
        (method.PartialImplementationPart ?? method).DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

    private static bool IsObjectOrType(ITypeSymbol namedType) =>
        namedType.SpecialType == SpecialType.System_Object
        || namedType.Is(KnownType.System_Type);
}
