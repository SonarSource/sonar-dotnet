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
using System.Reflection;
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

        private static readonly PackageManager packageManager =
            new PackageManager(CreatePackageRepository(), PackagesFolderRelativePath);

        private static readonly Dictionary<AssemblyReference, MetadataReference> cache =
            new Dictionary<AssemblyReference, MetadataReference>();

        private static readonly List<string> allowedNugetLibDirectories =
            new List<string>
            {
                "lib",
                "portable-net45",
                "net40",
                "net45",
                "netstandard1.0",
                "netstandard1.1"
            };

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
                .Where(x => allowedNugetLibDirectories.Any(y => x.Directory.Name.StartsWith(y)))
                .OrderByDescending(x => x.FullName);

            return MetadataReference.CreateFromFile(matchingDlls.First().FullName);
        }

        private static string GetNuGetPackageDirectory(AssemblyReference.NuGetInfo nuGetInfo) =>
            $@"{PackagesFolderRelativePath}{nuGetInfo.Name}.{GetRealVersionFolder(nuGetInfo)}\lib";

        private static string GetRealVersionFolder(AssemblyReference.NuGetInfo nuGetInfo) =>
            nuGetInfo.Version?.ToString()
            ?? Directory.GetDirectories(PackagesFolderRelativePath, $"{nuGetInfo.Name}*", SearchOption.TopDirectoryOnly)
                .Last()
                .Substring(PackagesFolderRelativePath.Length + nuGetInfo.Name.Length + 1);

        private static IPackageRepository CreatePackageRepository()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var localSettings = Settings.LoadDefaultSettings(new PhysicalFileSystem(currentFolder), null, null);

            // Get a package source provider that can use the settings
            var packageSourceProvider = new PackageSourceProvider(localSettings);

            // Create an aggregate repository that uses all of the configured sources
            var aggregateRepository = packageSourceProvider.CreateAggregateRepository(PackageRepositoryFactory.Default,
                true /* ignore failing repos. Errors will be logged as warnings. */ );

            return aggregateRepository;
        }
    }
}
