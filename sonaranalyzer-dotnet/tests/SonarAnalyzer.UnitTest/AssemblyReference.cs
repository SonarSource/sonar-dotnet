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
    internal static class MetadataReferenceHelper
    {
        internal const string NuGetLatestVersion = null;

        /// <summary>
        /// Relative path to the solution packages folder
        /// </summary>
        private const string PackagesFolderRelativePath = @"..\..\..\..\packages\";

        private static readonly string systemAssembliesFolder =
            new FileInfo(typeof(object).Assembly.Location).Directory.FullName;

        private static readonly PackageManager packageManager =
            new PackageManager(CreatePackageRepository(), PackagesFolderRelativePath);

        private static readonly Dictionary<string, MetadataReference[]> metadataReferenceCache =
            new Dictionary<string, MetadataReference[]>();

        private static readonly List<string> allowedNugetLibDirectories =
            new List<string>
            {
                "lib",
                "portable-net45",
                "net20",
                "net40",
                "net45",
                "netstandard1.0",
                "netstandard1.1",
                "netstandard2.0"
            };

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

        public static MetadataReference FromFrameworkAssembly(string assemblyName)
        {
            return metadataReferenceCache.GetOrAdd(assemblyName, ProcessAssembly).Single();

            MetadataReference[] ProcessAssembly(string name) =>
                new[] { MetadataReference.CreateFromFile(Path.Combine(systemAssembliesFolder, name)) };
        }

        public static MetadataReference[] FromNuGet(string packageId, string packageVersion = NuGetLatestVersion)
        {
            return metadataReferenceCache.GetOrAdd($"{packageId}.{packageVersion}", x => ProcessNuGet());

            MetadataReference[] ProcessNuGet()
            {
                packageManager.InstallPackage(packageId, SemanticVersion.ParseOptionalVersion(packageVersion),
                    ignoreDependencies: true, allowPrereleaseVersions: false);

                return GetFilesFromLatestFrameworkVersion(packageId, packageVersion);
            }
        }

        private static MetadataReference[] GetFilesFromLatestFrameworkVersion(string packageId, string packageVersion)
        {
            // The NuGet package will probably target multiple framework versions, with the
            // assemblies for each in a separate folder named after the framework. We want to
            // pick the latest, and we're relying on the fact that the folder names when sorted
            // order the frameworks from oldest to newest.
            var matchingDlls = Directory.GetFiles(GetNuGetPackageDirectory(), "*.dll", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .GroupBy(file => file.Directory.Name)
                .Where(group => allowedNugetLibDirectories.Any(y => group.Key.StartsWith(y)))
                .OrderByDescending(group => group.Key)
                .First();

            return matchingDlls.Select(file => MetadataReference.CreateFromFile(file.FullName)).ToArray();

            string GetNuGetPackageDirectory() =>
                $@"{PackagesFolderRelativePath}{packageId}.{GetRealVersionFolder()}\lib";

            string GetRealVersionFolder() =>
                packageVersion ?? GetLatestInstalledVersionFolder();

            string GetLatestInstalledVersionFolder() =>
                Directory.GetDirectories(PackagesFolderRelativePath, $"{packageId}*", SearchOption.TopDirectoryOnly)
                .Last()
                .Substring(PackagesFolderRelativePath.Length + packageId.Length + 1);
        }
    }
}
