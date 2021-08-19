/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    public static partial class NuGetMetadataFactory
    {
        private const string NugetConfigFolderRelativePath = @"..\..\..";
        private const string PackagesFolderRelativePath = @"..\..\..\..\..\packages\";

        // We use the global nuget cache for storing our packages if the NUGET_PACKAGES environment variable is defined.
        // This is especially helpful on the build agents where the packages are precached
        // (since we don't need to spawn a new process for calling the nuget.exe to install or copy them from global cache)
        private static readonly string PackagesFolder = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? PackagesFolderRelativePath;

        private static readonly string[] SortedAllowedDirectories =
        {
            "net",
            "netstandard2.1",
            "netstandard2.0",
            "net47",
            "net461",
            "netstandard1.6",
            "netstandard1.3",
            "netstandard1.1",
            "netstandard1.0",
            "net451",
            "net45",
            "net40",
            "net20",
            "portable-net45",
            "lib", // This has to be last, some packages have DLLs directly in "lib" directory
        };

        /// <param name="dllDirectory">Name of the directory containing DLL files inside *.nupgk/lib/{dllDirectory}/ or *.nupgk/runtimes/{runtime}/lib/{dllDirectory}/ folder.
        /// This directory name represents target framework in most cases.</param>
        public static IEnumerable<MetadataReference> Create(string packageId, string packageVersion, string runtime, string dllDirectory) =>
            Create(new Package(packageId, packageVersion, runtime), new[] { dllDirectory });

        public static IEnumerable<MetadataReference> Create(string packageId, string packageVersion, string runtime = null) =>
            Create(new Package(packageId, packageVersion, runtime), SortedAllowedDirectories);

        public static IEnumerable<MetadataReference> CreateNetStandard21()
        {
            var package = new Package("NETStandard.Library.Ref", "2.1.0", "netstandard2.1");
            package.EnsurePackageIsInstalled();

            return Directory.GetFiles(package.PackageDirectory(), "*.dll", SearchOption.AllDirectories)
               .Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
               .ToImmutableArray();
        }

        /// <param name="allowedDirectories">List of allowed directories sorted by preference to search for DLL files.</param>
        private static IEnumerable<MetadataReference> Create(Package package, string[] allowedDirectories)
        {
            package.EnsurePackageIsInstalled();

            var allowedNugetLibDirectoriesByPreference = allowedDirectories.Select((folder, priority) => new { folder, priority });
            var packageDirectory = package.PackageDirectory();
            var matchingDllsGroups = Directory.GetFiles(packageDirectory, "*.dll", SearchOption.AllDirectories)
                                              .Select(path => new FileInfo(path))
                                              .GroupBy(file => file.Directory.Name).ToArray();
            var selectedGroup = matchingDllsGroups.Length == 1 && matchingDllsGroups[0].Key.EndsWith(".dll")
                ? matchingDllsGroups[0]
                : matchingDllsGroups.Join(
                                        allowedNugetLibDirectoriesByPreference,
                                        group => group.Key.Split('+').First(),
                                        allowed => allowed.folder,
                                        (group, allowed) => new { group, allowed.priority })
                                    .OrderBy(merged => merged.priority)
                                    .First()
                                    .group;

            return selectedGroup.Select(file => (MetadataReference)MetadataReference.CreateFromFile(file.FullName))
                                .ToImmutableArray();
        }
    }
}
