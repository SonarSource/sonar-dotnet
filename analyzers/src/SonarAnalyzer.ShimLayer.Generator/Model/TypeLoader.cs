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

using System.IO;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.ShimLayer.Generator.Model;

public sealed class TypeLoader : IDisposable
{
    private readonly MetadataLoadContext metadataContext = new(new CustomAssemblyResolver(), Path.GetFileNameWithoutExtension(typeof(object).Assembly.Location));

    public void Dispose() =>
        metadataContext.Dispose();

    public TypeDescriptor[] LoadBaseline()
    {
        var assembly = typeof(TypeLoader).Assembly;
        using var common = assembly.GetManifestResourceStream("Microsoft.CodeAnalysis.1.3.2.dll");
        using var csharp = assembly.GetManifestResourceStream("Microsoft.CodeAnalysis.CSharp.1.3.2.dll");
        return [
            .. Load(metadataContext.LoadFromStream(common)),
            .. Load(metadataContext.LoadFromStream(csharp))
            ];
    }

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

file sealed class CustomAssemblyResolver : PathAssemblyResolver
{
    public CustomAssemblyResolver() : base(Directory.GetFiles(Path.GetDirectoryName(typeof(object).Assembly.Location), "*.dll")) { }

    public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName) =>
        base.Resolve(context, assemblyName)
        ?? context.GetAssemblies().Single(x => x.GetName().Name == assemblyName.Name);   // Microsoft.CodeAnalysis 1.3.2 is actually 1.3.1 and it does not resolve automatically
}
