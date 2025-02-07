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

    public static MetadataReference CreateReference(string assemblyName, Sdk sdk)
    {
        var  path = sdk switch
        {
            Sdk.AspNetCore => SdkPathProvider.LatestAspNetCoreSdkFolder(),
            Sdk.WindowsDesktop => SdkPathProvider.LatestWindowsDesktopSdkFolder(),
            _ => SystemAssembliesFolder
        };

        return MetadataReference.CreateFromFile(Path.Combine(path, assemblyName));
    }

#endif

    public static MetadataReference CreateReference(string assemblyName) =>
        MetadataReference.CreateFromFile(Path.Combine(SystemAssembliesFolder, assemblyName));

    public static MetadataReference CreateReference(string assemblyName, string subFolder) =>
        MetadataReference.CreateFromFile(Path.Combine(SystemAssembliesFolder, subFolder, assemblyName));
}
