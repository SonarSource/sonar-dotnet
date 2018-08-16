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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet;

namespace SonarAnalyzer.UnitTest
{
    [TestClass]
    internal static class NuGetMetadataReference
    {
        #region Helpers

        private const string PackagesFolderRelativePath = @"..\..\..\..\packages\";

        private static readonly ISet<(string packageId, string packageVersion)> allNuGets =
            new HashSet<(string, string)>();

        private static readonly List<string> allowedNugetLibDirectories =
            new List<string>
            {
                "lib",
                "portable-net45",
                "net40",
                "net45",
                "netstandard1.0",
                "netstandard1.1",
                "netstandard2.0"
            };

        private static readonly PackageManager packageManager =
            new PackageManager(CreatePackageRepository(), PackagesFolderRelativePath);

        private static Dictionary<string, MetadataReference[]> Create(string packageId, params string[] versions) =>
            versions.Select(v => new { version = v, references = CreateReferences(packageId, v) })
                .ToDictionary(x => x.version, x => x.references);

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

        private static MetadataReference[] CreateReferences(string packageId, string packageVersion)
        {
            var fixedVersion = packageVersion != Constants.NuGetLatestVersion
                ? packageVersion
                : null;

            allNuGets.Add((packageId, fixedVersion));

            var matchingDlls = Directory.GetFiles(GetNuGetPackageDirectory(), "*.dll", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .GroupBy(file => file.Directory.Name)
                .Where(group => allowedNugetLibDirectories.Any(y => group.Key.StartsWith(y)))
                .OrderByDescending(group => group.Key)
                .First();

            return matchingDlls.Select(file => MetadataReference.CreateFromFile(file.FullName)).ToArray();

            string GetNuGetPackageDirectory() =>
                $@"{PackagesFolderRelativePath}{packageId}.{GetRealVersionFolder(packageId, fixedVersion)}\lib";
        }

        private static string GetRealVersionFolder(string packageId, string packageVersion) =>
            packageVersion?.ToString()
            ?? Directory.GetDirectories(PackagesFolderRelativePath, $"{packageId}*", SearchOption.TopDirectoryOnly)
                .Last()
                .Substring(PackagesFolderRelativePath.Length + packageId.Length + 1);
        #endregion Helpers

        internal static Dictionary<string, MetadataReference[]> FluentAssertions { get; }
            = Create("FluentAssertions", "4.19.4", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> MicrosoftAspNetCoreMvcCore { get; }
            = Create("Microsoft.AspNetCore.Mvc.Core", "2.0.4", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> MicrosoftAspNetCoreMvcViewFeatures { get; }
            = Create("Microsoft.AspNetCore.Mvc.ViewFeatures", "2.0.4", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> MicrosoftAspNetCoreRoutingAbstractions { get; }
            = Create("Microsoft.AspNetCore.Routing.Abstractions", "2.0.3", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> MicrosoftAspNetMvc { get; }
            = Create("Microsoft.AspNet.Mvc", "3.0.20105.1", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> MSTestTestFramework { get; }
            = Create("MSTest.TestFramework", "1.1.11", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> NUnit { get; }
            = Create("NUnit", "2.5.7.10213", "2.6.7", "3.0.0", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> SystemCollectionsImmutable { get; }
            = Create("System.Collections.Immutable", "1.3.0");

        internal static Dictionary<string, MetadataReference[]> SystemThreadingTasksExtensions { get; }
            = Create("System.Threading.Tasks.Extensions", "4.0.0", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> XunitAssert { get; }
            = Create("xunit.assert", "2.0.0", Constants.NuGetLatestVersion);

        internal static Dictionary<string, MetadataReference[]> XunitExtensibilityCore { get; }
            = Create("xunit.extensibility.core", "2.0.0", Constants.NuGetLatestVersion);

        [AssemblyInitialize]
        public static void SetupAssembly(TestContext context)
        {
            foreach (var (packageId, packageVersion) in allNuGets)
            {
                if (packageVersion == null ||
                    !Directory.Exists(GetRealVersionFolder(packageId, packageVersion)))
                {
                    packageManager.InstallPackage(packageId, SemanticVersion.ParseOptionalVersion(packageVersion),
                        ignoreDependencies: true, allowPrereleaseVersions: false);
                }
            }
        }
    }
}
