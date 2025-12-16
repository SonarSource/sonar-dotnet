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

using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.ShimLayer;

internal static class SyntaxNodeTypes
{
    private static readonly ImmutableDictionary<Type, Type> Map;    // Wrapper type => Latest Roslyn type

    static SyntaxNodeTypes()
    {
        var builder = ImmutableDictionary.CreateBuilder<Type, Type>();
        var assembly = typeof(CSharpSyntaxNode).Assembly;
        foreach (var wrapper in typeof(SyntaxNodeTypes).Assembly.ExportedTypes)
        {
            if ((Load(wrapper, nameof(BaseNamespaceDeclarationSyntaxWrapper.WrappedTypeName)) ?? Load(wrapper, nameof(BaseNamespaceDeclarationSyntaxWrapper.FallbackWrappedTypeName))) is { } type)
            {
                builder.Add(wrapper, type);
            }
        }
        Map = builder.ToImmutable();

        Type Load(Type wrapper, string fieldName) =>
            wrapper.GetField(fieldName) is { } field && field.GetValue(null) is string name ? assembly.GetType(name) : null;
    }

    public static Type LatestType(Type wrapper) =>
        Map.TryGetValue(wrapper, out var latest) ? latest : null;
}
