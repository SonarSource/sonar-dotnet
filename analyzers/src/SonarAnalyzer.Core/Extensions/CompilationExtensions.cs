/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Extensions;

internal static class CompilationExtensions
{
    public static INamedTypeSymbol GetTypeByMetadataName(this Compilation compilation, KnownType knownType) =>
        compilation.GetTypeByMetadataName(knownType.MetadataName);

    public static IMethodSymbol SpecialTypeMethod(this Compilation compilation, SpecialType type, string methodName) =>
        (IMethodSymbol)compilation.GetSpecialType(type).GetMembers(methodName).SingleOrDefault();

    public static bool IsNetFrameworkTarget(this Compilation compilation) =>
        // There's no direct way of checking compilation target framework yet (09/2020).
        // See https://github.com/dotnet/roslyn/issues/3798
        compilation.ObjectType.ContainingAssembly.Name == "mscorlib";

    public static bool ReferencesAny(this Compilation compilation, params KnownAssembly[] assemblies) =>
        assemblies.Any()
            ? Array.Exists(assemblies, x => compilation.References(x))
            : throw new ArgumentException("Assemblies argument needs to be non-empty");

    public static bool ReferencesAll(this Compilation compilation, params KnownAssembly[] assemblies) =>
        Array.TrueForAll(assemblies, x => compilation.References(x));

    public static bool References(this Compilation compilation, KnownAssembly assembly) =>
        assembly.IsReferencedBy(compilation);

    public static bool IsMemberAvailable<TMemberType>(this Compilation compilation, KnownType type, string memberName, Func<TMemberType, bool> memberCheck = null)
        where TMemberType : ISymbol
    {
        var containingType = compilation.GetTypeByMetadataName(type);
        if (containingType is null)
        {
            return false;
        }
        var memberSymbols = containingType.GetMembers(memberName).OfType<TMemberType>();
        return memberCheck is null
            ? memberSymbols.Any()
            : memberSymbols.Any(memberCheck);
    }
}
