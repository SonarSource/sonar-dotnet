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
        new(CheckRazorSourceGeneratorPath(), new AssemblyLoader())
    ];

    public static string CheckRazorSourceGeneratorPath() =>
        File.Exists(RazorSourceGeneratorPath) ? RazorSourceGeneratorPath : throw new FileNotFoundException($"Razor sourcegenerator not found: {RazorSourceGeneratorPath}");

    public static string LatestSdkFolder()
    {
        var objectAssembly = typeof(object).Assembly;
        var objectAssemblyDirectory = new FileInfo(objectAssembly.Location).Directory; // C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.4
        var dotnetDirectory = objectAssemblyDirectory.Parent.Parent.Parent;            // C:\Program Files\dotnet
        var sdkDirectory = Path.Combine(dotnetDirectory.FullName, "sdk");              // C:\Program Files\dotnet\sdk
        if (!Directory.Exists(sdkDirectory))
        {
            throw new NotSupportedException($"Razor analysis is only supported for .Net Core.");
        }
        // List of all sdk directories for the major version
        var latestSdkMajorDirectories = Directory.GetDirectories(sdkDirectory, $"{objectAssembly.GetName().Version.Major}.*", SearchOption.TopDirectoryOnly);
        return latestSdkMajorDirectories.OrderByDescending(x => new DirectoryInfo(x).Name).FirstOrDefault(); // Get the latest sdk directory
    }

    private sealed class AssemblyLoader : IAnalyzerAssemblyLoader
    {
        public void AddDependencyLocation(string fullPath) { }
        public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
    }
}
