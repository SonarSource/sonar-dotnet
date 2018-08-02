/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using NuGet;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest
{
    internal static class AssemblyReferenceService
    {
        private const string PackagesFolderRelativePath = @"..\..\..\..\packages\";

        private static readonly string systemAssembliesFolder =
            new FileInfo(typeof(object).Assembly.Location).Directory.FullName;

        private static PackageManager packageManager =
            new PackageManager(PackageRepositoryFactory.Default.CreateRepository("https://www.nuget.org/api/v2/"),
                PackagesFolderRelativePath);

        private static readonly Dictionary<AssemblyReference, MetadataReference> cache =
            new Dictionary<AssemblyReference, MetadataReference>();

        public static MetadataReference GetMetadataReference(AssemblyReference assemblyReference) =>
            cache.GetOrAdd(assemblyReference, ProcessAssemblyReference);

        private static MetadataReference ProcessAssemblyReference(AssemblyReference assemblyReference)
        {
            if (assemblyReference.NuGet == null)
            {
                var assemblyPath = Path.Combine(systemAssembliesFolder, assemblyReference.Name);
                return MetadataReference.CreateFromFile(assemblyPath);
            }

            packageManager.InstallPackage(assemblyReference.NuGet.Name, assemblyReference.NuGet.Version,
                ignoreDependencies: true, allowPrereleaseVersions: false);

            var matchingDlls = Directory.GetFiles(GetNuGetPackageDirectory(assemblyReference.NuGet),
                    assemblyReference.Name, SearchOption.AllDirectories)
                .Select(x => new FileInfo(x))
                .Where(x => x.Directory.Name == "lib" || (x.Directory.Name.StartsWith("net") && x.Directory.Name != "netstandard1.3"))
                .OrderByDescending(x => x.FullName);

            return MetadataReference.CreateFromFile(matchingDlls.First().FullName);
        }

        private static string GetNuGetPackageDirectory(AssemblyReference.NuGetInfo nuGetInfo) =>
            $@"{PackagesFolderRelativePath}{nuGetInfo.Name}.{GetRealVersionFolder(nuGetInfo)}\lib";

        private static string GetRealVersionFolder(AssemblyReference.NuGetInfo nuGetInfo) =>
            nuGetInfo.Version?.ToString()
            ?? Directory.GetDirectories(PackagesFolderRelativePath, nuGetInfo.Name, SearchOption.TopDirectoryOnly)
                .First()
                .Substring(nuGetInfo.Name.Length + 1);
    }
}
