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

        /// <param name="allowedDirectories">List of allowed directories sorted by preference to search for DLL files.</param>
        private static IEnumerable<MetadataReference> Create(Package package, string[] allowedDirectories)
        {
            var packageDir = package.EnsureInstalled();
            // some packages (see Mono.Posix.NETStandard.1.0.0) may contain target framework only in ref folder
            var dllsPerDirectory = Directory.GetFiles(packageDir, "*.dll", SearchOption.AllDirectories)
                                            .GroupBy(x => Path.GetDirectoryName(x).Split('+').First())
                                            .Select(x => (directory: Path.GetFileName(x.Key), dllPaths: x.AsEnumerable()))
                                            .ToArray();
            foreach (var allowedDirectory in allowedDirectories)
            {
                // dllsPerDirectory can contain the same <directory> from \lib\<directory> and \ref\<directory>. We don't care who wins.
                if (dllsPerDirectory.Where(x => x.directory == allowedDirectory).Select(x => x.dllPaths).FirstOrDefault() is { } dllPaths)
                {
                    foreach (var dllPath in dllPaths)
                    {
                        LogMessage("File: " + dllPath);
                    }
                    return dllPaths.Select(x => MetadataReference.CreateFromFile(x)).ToArray();
                }
            }
            throw new InvalidOperationException($"No allowed DLL directory was found in {packageDir}. Add new target framework to SortedAllowedDirectories or set dllDirectory argument explicitly.");
        }

        private static void LogMessage(string message) =>
            Console.WriteLine($"Test setup: {message}");
    }
}
