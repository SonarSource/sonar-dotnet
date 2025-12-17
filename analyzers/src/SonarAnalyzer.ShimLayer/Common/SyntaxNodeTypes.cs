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

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.ShimLayer;

internal static class SyntaxNodeTypes
{
    public static Type LatestType(Type wrapper)
    {
        return Load(wrapper, nameof(BaseNamespaceDeclarationSyntaxWrapper.WrappedTypeName)) ?? Load(wrapper, nameof(BaseNamespaceDeclarationSyntaxWrapper.FallbackWrappedTypeName));

        static Type Load(Type wrapper, string fieldName) =>
            wrapper.GetField(fieldName, BindingFlags.Static | BindingFlags.Public) is { } field && field.GetValue(null) is string name
                ? typeof(CSharpSyntaxNode).Assembly.GetType(name) // This may need to be extended to other assemblies if needed. See TypeLoader.LoadBaseline and .LoadLatest
                : null;
    }
}
