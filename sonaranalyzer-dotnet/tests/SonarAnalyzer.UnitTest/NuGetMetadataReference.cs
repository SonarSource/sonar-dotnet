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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
                "net20",
                "net40",
                "net45",
                "netstandard1.0",
                "netstandard1.1",
                "netstandard2.0"
            };

        private static readonly PackageManager packageManager =
            new PackageManager(CreatePackageRepository(), PackagesFolderRelativePath);

        private static Lazy<Dictionary<string, MetadataReference[]>> CreateLazy(string packageId, params string[] versions)
        {
            foreach (var packageVersion in versions)
            {
                allNuGets.Add((packageId, packageVersion));
            }

            return new Lazy<Dictionary<string, MetadataReference[]>>(Create, LazyThreadSafetyMode.PublicationOnly);

            Dictionary<string, MetadataReference[]> Create() =>
                versions.Select(v => new { version = v, references = CreateReferences(packageId, v) })
                    .ToDictionary(x => x.version, x => x.references);
        }

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
            var packageDir = GetNuGetPackageDirectory();
            if (packageVersion != Constants.NuGetLatestVersion && !Directory.Exists(packageDir))
            {
                LogMessage($"Package not found at {packageDir}");
                InstallPackage(packageId, packageVersion);
            }

            var matchingDlls = Directory.GetFiles(packageDir, "*.dll", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .GroupBy(file => file.Directory.Name)
                .Where(group => allowedNugetLibDirectories.Any(y => group.Key.StartsWith(y)))
                .OrderByDescending(group => group.Key)
                .First();

            return matchingDlls.Select(file => MetadataReference.CreateFromFile(file.FullName)).ToArray();

            string GetNuGetPackageDirectory() =>
                $@"{PackagesFolderRelativePath}{packageId}.{GetRealVersionFolder(packageId, packageVersion)}\lib";
        }

        private static string GetRealVersionFolder(string packageId, string packageVersion) =>
            packageVersion != Constants.NuGetLatestVersion
                ? packageVersion.ToString()
                : Directory.GetDirectories(PackagesFolderRelativePath, $"{packageId}*", SearchOption.TopDirectoryOnly)
                    .Last()
                    .Substring(PackagesFolderRelativePath.Length + packageId.Length + 1);

        #endregion Helpers

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyFluentAssertions =
            CreateLazy("FluentAssertions", "4.19.4", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyMicrosoftAspNetCoreMvcCore =
            CreateLazy("Microsoft.AspNetCore.Mvc.Core", "2.0.4", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyMicrosoftAspNetCoreMvcViewFeatures =
            CreateLazy("Microsoft.AspNetCore.Mvc.ViewFeatures", "2.0.4", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyMicrosoftAspNetCoreRoutingAbstractions =
            CreateLazy("Microsoft.AspNetCore.Routing.Abstractions", "2.0.3", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyMicrosoftAspNetMvc =
            CreateLazy("Microsoft.AspNet.Mvc", "3.0.20105.1", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyMSTestTestFramework =
            CreateLazy("MSTest.TestFramework", "1.1.11", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyNUnit =
            CreateLazy("NUnit", "2.5.7.10213", "2.6.7", "3.0.0", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazySystemCollectionsImmutable =
            CreateLazy("System.Collections.Immutable", "1.3.0");

        private static Lazy<Dictionary<string, MetadataReference[]>> lazySystemThreadingTasksExtensions =
            CreateLazy("System.Threading.Tasks.Extensions", "4.0.0", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyXunitAssert =
            CreateLazy("xunit.assert", "2.0.0", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyXunitExtensibilityCore =
            CreateLazy("xunit.extensibility.core", "2.0.0", Constants.NuGetLatestVersion);

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyXunitV1 =
            CreateLazy("xunit", "1.9.1");

        private static Lazy<Dictionary<string, MetadataReference[]>> lazyXunitExtensionsV1 =
            CreateLazy("xunit.extensions", "1.9.1");

        internal static Dictionary<string, MetadataReference[]> FluentAssertions =>
            lazyFluentAssertions.Value;

        internal static Dictionary<string, MetadataReference[]> MicrosoftAspNetCoreMvcCore =>
            lazyMicrosoftAspNetCoreMvcCore.Value;

        internal static Dictionary<string, MetadataReference[]> MicrosoftAspNetCoreMvcViewFeatures =>
            lazyMicrosoftAspNetCoreMvcViewFeatures.Value;

        internal static Dictionary<string, MetadataReference[]> MicrosoftAspNetCoreRoutingAbstractions =>
            lazyMicrosoftAspNetCoreRoutingAbstractions.Value;

        internal static Dictionary<string, MetadataReference[]> MicrosoftAspNetMvc =>
            lazyMicrosoftAspNetMvc.Value;

        internal static Dictionary<string, MetadataReference[]> MSTestTestFramework =>
            lazyMSTestTestFramework.Value;

        internal static Dictionary<string, MetadataReference[]> NUnit =>
            lazyNUnit.Value;

        internal static Dictionary<string, MetadataReference[]> SystemCollectionsImmutable =>
            lazySystemCollectionsImmutable.Value;

        internal static Dictionary<string, MetadataReference[]> SystemThreadingTasksExtensions =>
            lazySystemThreadingTasksExtensions.Value;

        internal static Dictionary<string, MetadataReference[]> XunitAssert =>
            lazyXunitAssert.Value;

        internal static Dictionary<string, MetadataReference[]> XunitExtensibilityCore =>
            lazyXunitExtensibilityCore.Value;

        internal static MetadataReference[] XunitV1Packages =>
            lazyXunitV1.Value.Values.First()
            .Concat(lazyXunitExtensionsV1.Value.Values.First())
            .ToArray();

        [AssemblyInitialize]
        public static void SetupAssembly(TestContext context)
        {
            // Choosing one day to reduce the waiting time when a new version of the used nugets is
            // released. If the waiting time when running tests locally is big we can increase.
            const int VersionCheckDelayInDays = 1;

            // Install new nugets only once per day to improve the performance when running tests locally
            // When adding a new nuget it is recommended to delete the content of
            // sonar -csharp\sonaranalyzer-dotnet\TestResults
            var lastCheck = TestResultHelper.GetPreviousRunDate(context);
            LogMessage($"Last check for latest NuGets: {lastCheck}");
            if (DateTime.Now.Subtract(lastCheck).TotalDays < VersionCheckDelayInDays)
            {
                LogMessage("Skipping check for latest NuGets.");
                return;
            }

            LogMessage("Checking for latest versions of NuGet packages...");
            foreach (var (packageId, packageVersion) in allNuGets)
            {
                if (packageVersion == Constants.NuGetLatestVersion)
                {
                    InstallPackage(packageId, packageVersion);
                }
            }
            LogMessage("Check for latest versions completed.");
        }

        private static void InstallPackage(string packageId, string packageVersion)
        {
            var realVersion = packageVersion != Constants.NuGetLatestVersion
                ? packageVersion
                : null;

            LogMessage($"Installing NuGet {packageId}.{packageVersion ?? "{latest}"}");
            packageManager.InstallPackage(packageId, SemanticVersion.ParseOptionalVersion(realVersion),
                ignoreDependencies: true, allowPrereleaseVersions: false);
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now} Test setup: {message}");
        }
    }
}
