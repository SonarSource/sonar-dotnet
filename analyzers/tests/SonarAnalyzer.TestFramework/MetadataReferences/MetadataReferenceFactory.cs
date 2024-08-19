/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.IO;

namespace SonarAnalyzer.TestFramework.MetadataReferences;

#if NET

public enum Sdk
{
    NETCore,    // This is the default folder with system assemblies
    AspNetCore,
    WindowsDesktop
}

#endif

internal static class MetadataReferenceFactory
{
    private static readonly string SystemAssembliesFolder = Path.GetDirectoryName(typeof(object).Assembly.Location);

    public static IEnumerable<MetadataReference> Create(string assemblyName) =>
        ImmutableArray.Create(CreateReference(assemblyName));

    public static MetadataReference Create(Type type) =>
        MetadataReference.CreateFromFile(type.Assembly.Location);

#if NET

    public static MetadataReference CreateReference(string assemblyName, Sdk sdk) =>
        MetadataReference.CreateFromFile(Path.Combine(SystemAssembliesFolder.Replace(Sdk.NETCore.ToString(), sdk.ToString()), assemblyName));

#endif

    public static MetadataReference CreateReference(string assemblyName) =>
        MetadataReference.CreateFromFile(Path.Combine(SystemAssembliesFolder, assemblyName));

    public static MetadataReference CreateReference(string assemblyName, string subFolder) =>
        MetadataReference.CreateFromFile(Path.Combine(SystemAssembliesFolder, subFolder, assemblyName));
}
