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
using System.Reflection;

namespace SonarAnalyzer.TestFramework.Common;

public static class SourceGeneratorProvider
{
    private static readonly string RazorSourceGeneratorPath =
        Path.Combine(LatestSdkFolder(), "Sdks", "Microsoft.NET.Sdk.Razor", "source-generators", "Microsoft.CodeAnalysis.Razor.Compiler.SourceGenerators.dll");

    public static AnalyzerFileReference[] SourceGenerators { get; } =
    [
        new(CheckAndReturnRazorSourceGeneratorPath(), new AssemblyLoader())
    ];

    public static string CheckAndReturnRazorSourceGeneratorPath() =>
        File.Exists(RazorSourceGeneratorPath) ? RazorSourceGeneratorPath : throw new FileNotFoundException($"Razor sourcegenerator not found: {RazorSourceGeneratorPath}");

    public static string LatestSdkFolder()
    {
        var objectAssembly = typeof(object).Assembly;
        var objectAssemblyDirectory = Directory.GetParent(objectAssembly.Location);    // C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.4
        var dotnetDirectory = objectAssemblyDirectory.Parent.Parent.Parent;            // C:\Program Files\dotnet
        var sdkDirectory = Path.Combine(dotnetDirectory.FullName, "sdk");              // C:\Program Files\dotnet\sdk

        if (!Directory.Exists(sdkDirectory))
        {
            throw new NotSupportedException($"The directory '{sdkDirectory}' does not exist. " +
                $"This may be because you are not using .NET Core. " +
                $"Please note that Razor analysis is only supported when using .NET Core.");
        }
        return Directory.GetDirectories(sdkDirectory, $"{objectAssembly.GetName().Version.Major}.*", SearchOption.TopDirectoryOnly) is { Length: > 0 } specificMajorVersionSdkDirectories
            ? specificMajorVersionSdkDirectories.OrderByDescending(x => Version.Parse(new DirectoryInfo(x).Name)).First()
            : throw new DirectoryNotFoundException($"SDK directory not found for version {objectAssembly.GetName().Version.Major}");
    }

    private sealed class AssemblyLoader : IAnalyzerAssemblyLoader
    {
        public void AddDependencyLocation(string fullPath) { }
        public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
    }
}
