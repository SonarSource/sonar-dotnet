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

namespace SonarAnalyzer.ShimLayer.Generator.Model;

public static class TypeLoader
{
    public static TypeDescriptor[] LoadLatest() =>
        [
            ..Load(typeof(SyntaxNode).Assembly),        // Microsoft.CodeAnalysis
            ..Load(typeof(CSharpSyntaxNode).Assembly)   // Microsoft.CodeAnalysis.CSharp
        ];

    private static TypeDescriptor[] Load(Assembly assembly) =>
        assembly.GetExportedTypes().Select(x => new TypeDescriptor(x, FindMembers(x).ToArray())).ToArray();

    private static IEnumerable<MemberInfo> FindMembers(Type type)
    {
        foreach (var member in type.GetMembers())
        {
            yield return member;
        }
        if (type.IsInterface)   // Members from inherited interfaces are not present in type.GetMembers()
        {
            foreach (var member in type.GetInterfaces().SelectMany(x => x.GetMembers()))
            {
                yield return member;
            }
        }
    }
}
