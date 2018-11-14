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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using NuGet;

namespace SonarAnalyzer.UnitTest
{
    internal static class NuGetMetadataReference
    {
        #region Helpers

        private const string PackagesFolderRelativePath = @"..\..\..\..\packages\";

        private static readonly string[] allowedNugetLibDirectoriesInOrderOfPreference =
            new string[] {
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
                "lib",
            };

        private static readonly PackageManager packageManager =
            new PackageManager(CreatePackageRepository(), PackagesFolderRelativePath);

        public static IEnumerable<MetadataReference> Create(string packageId, string packageVersion)
        {
            return Create(packageId, packageVersion, allowedNugetLibDirectoriesInOrderOfPreference);
        }

        public static IEnumerable<MetadataReference> Create(string packageId, string packageVersion, string targetFramework)
        {
            return Create(packageId, packageVersion, new string[] { targetFramework });
        }

        private static IEnumerable<MetadataReference> Create(string packageId, string packageVersion, string[] allowedTargetFrameworks)
        {
            EnsurePackageIsInstalled(packageId, packageVersion);

            var allowedNugetLibDirectoriesByPreference = allowedTargetFrameworks.
                Zip(Enumerable.Range(0, allowedTargetFrameworks.Length), (folder, priority) => new { folder, priority });
            var packageDirectory = GetNuGetPackageDirectory(packageId, packageVersion);
            var matchingDllsGroups = Directory.GetFiles(packageDirectory, "*.dll", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .GroupBy(file => file.Directory.Name).ToArray();
            IGrouping<string, FileInfo> selectedGroup;

            selectedGroup = matchingDllsGroups.Length == 1 && matchingDllsGroups[0].Key.EndsWith(".dll")
                ? matchingDllsGroups[0]
                : matchingDllsGroups.Join(allowedNugetLibDirectoriesByPreference,
                    group => group.Key.Split('+').First(),
                    allowed => allowed.folder,
                    (group, allowed) => new { group, allowed.priority })
                .OrderBy(merged => merged.priority)
                .First()
                .group;

            DumpSelectedGroup(packageId, packageVersion, selectedGroup);

            return selectedGroup.Select(file => (MetadataReference)MetadataReference.CreateFromFile(file.FullName))
                .ToImmutableArray();
        }

        private static void DumpSelectedGroup(string packageId, string packageVersion, IGrouping<string, FileInfo> fileGroup)
        {
            Console.WriteLine();
            Console.WriteLine($"Package: {packageId}");
            Console.WriteLine($"Version: {packageVersion}, chosen targetFramework: {fileGroup.Key}");
            foreach (var file in fileGroup)
            {
                Console.WriteLine($"File: {file.FullName}");
            }
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
        private static string GetNuGetPackageDirectory(string packageId, string packageVersion)
        {
            var x = $@"{PackagesFolderRelativePath}{packageId}.{GetRealVersionFolder(packageId, packageVersion)}\lib";
            return x;
        }

        private static string GetRealVersionFolder(string packageId, string packageVersion) =>
            packageVersion != Constants.NuGetLatestVersion
                ? packageVersion.ToString()
                : GetSortedPackageFolders(packageId)
                    .Select(path => Path.GetFileName(path).Substring(packageId.Length + 1))
                    .Last(path => char.IsNumber(path[0]));

        #endregion Helpers

        #region Package installation and update helpers

        private static void EnsurePackageIsInstalled(string packageId, string packageVersion)
        {
            if (packageVersion == Constants.NuGetLatestVersion)
            {
                if (IsCheckForLatestPackageRequired(packageId))
                {
                    LogMessage($"Checking for newer version of package: {packageId}");
                    InstallPackage(packageId, packageVersion);
                    WriteLastUpdateFile(packageId);
                }
                else
                {
                    LogMessage($"Skipping check for latest NuGet since checked recently: {packageId}");
                }
            }
            else
            {
                // Check to see if the specific package is already installed
                var packageDir = GetNuGetPackageDirectory(packageId, packageVersion);
                if (Directory.Exists(packageDir))
                {
                    LogMessage($"Package found at {packageDir}");
                }
                else
                {
                    LogMessage($"Package not found at {packageDir}");
                    InstallPackage(packageId, packageVersion);
                }
            }
        }
        
        /// <summary>
        /// Returns the list of folders containing installed versions of the specified package,
        /// or an empty list if the package is not installed.
        /// </summary>
        /// <remarks>
        /// Package directory names are in the form "{package id}.{package version}".
        /// The list is sorted in ascending order, so the most recent version will be last.
        /// </remarks>
        private static IEnumerable<string> GetSortedPackageFolders(string packageId)
        {
            // The package will be in a folder called "\packages\{packageId}.{version}", but:
            // : the package might not be installed
            // : there might be multiple versions installed
            // : there might be a package that starts with the same package id
            //      e.g. Microsoft.AspNetCore.Core and Microsoft.AspNetCore.Core.Diagnostics
            // Most packages have a three-part version, but some have four. We don't check
            // the actual number of parts, as long as there is at least one.
            var matcher = new Regex($"{packageId}(.\\d+)+$");

            var directories = Directory.GetDirectories(PackagesFolderRelativePath, $"{packageId}.*", SearchOption.TopDirectoryOnly)
                .Where(path => matcher.IsMatch(path))
                .OrderBy(name => name)
                .ToArray();
            return directories;
        }
        
        private static string GetLastCheckFilePath(string packageId)
        {
            // The file containing the last-check timestamp is stored in folder of the
            // latest version of the package.
            const string LastUpdateFileName = "LastCheckedForUpdate.txt";

            var directory = GetSortedPackageFolders(packageId)
                .LastOrDefault();

            if (directory == null)
            {
                return null;
            }

            return Path.Combine(directory, LastUpdateFileName);
        }

        private static DateTime GetLastCheckTime(string packageId)
        {
            var filePath = GetLastCheckFilePath(packageId);
            if (filePath == null ||
                !File.Exists(filePath) ||
                !DateTime.TryParse(File.ReadAllText(filePath), out var timestamp))
            {
                return DateTime.MinValue;
            }
            return timestamp;
        }

        private static void InstallPackage(string packageId, string packageVersion)
        {
            var realVersion = packageVersion != Constants.NuGetLatestVersion
                ? packageVersion
                : null;

            LogMessage($"Installing NuGet {packageId}.{packageVersion}");
            packageManager.InstallPackage(packageId, SemanticVersion.ParseOptionalVersion(realVersion),
                ignoreDependencies: true, allowPrereleaseVersions: false);
        }

        private static bool IsCheckForLatestPackageRequired(string packageId)
        {
            // Install new nugets only once per day to improve the performance when running tests locally.

            // We write a file with the timestamp of the last check in the package directory
            // of the newest version of the package.
            // If we can't find the package directory, we assume a check is required.
            // If we can find an installation of the package but not the timestamp file, we assume a
            // check is required (the package we found might be a specific older version that was installed
            // by another test).

            // Choosing one day to reduce the waiting time when a new version of the used nugets is
            // released. If the waiting time when running tests locally is big we can increase.Annecy, France
            const int VersionCheckDelayInDays = 1;

            var lastCheck = GetLastCheckTime(packageId);
            LogMessage($"Last check for latest NuGets: {lastCheck}");
            return (DateTime.Now.Subtract(lastCheck).TotalDays > VersionCheckDelayInDays);
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] Test setup: {message}");
        }

        private static void WriteLastUpdateFile(string packageId)
        {
            var filePath = GetLastCheckFilePath(packageId);
            File.WriteAllText(filePath, DateTime.Now.ToString("d")); // short date pattern
        }
        #endregion

        public static IEnumerable<MetadataReference> MicrosoftVisualStudioQualityToolsUnitTestFramework =>
            Create("VS.QualityTools.UnitTestFramework", "15.0.27323.2");

        public static IEnumerable<MetadataReference> MSTestTestFrameworkV1 =>
            Create("MSTest.TestFramework", "1.1.11");

        public static IEnumerable<MetadataReference> XunitFrameworkV1 =>
            Create("xunit", "1.9.1")
            .Concat(Create("xunit.extensions", "1.9.1"));

        public static IEnumerable<MetadataReference> FluentAssertions(string packageVersion) =>
            Create("FluentAssertions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCore(string packageVersion) =>
            Create("Microsoft.AspNetCore", packageVersion);

        public static IEnumerable<MetadataReference> Log4Net(string packageVersion, string targetFramework) =>
            Create("log4net", packageVersion, targetFramework);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreDiagnostics(string packageVersion) =>
            Create("Microsoft.AspNetCore.Diagnostics", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(string packageVersion) =>
            Create("Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreHosting(string packageVersion) =>
            Create("Microsoft.AspNetCore.Hosting", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreHostingAbstractions(string packageVersion) =>
            Create("Microsoft.AspNetCore.Hosting.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreHttpAbstractions(string packageVersion) =>
            Create("Microsoft.AspNetCore.Http.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreMvcAbstractions(string packageVersion) =>
            Create("Microsoft.AspNetCore.Mvc.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreMvcCore(string packageVersion) =>
            Create("Microsoft.AspNetCore.Mvc.Core", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreMvcViewFeatures(string packageVersion) =>
            Create("Microsoft.AspNetCore.Mvc.ViewFeatures", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreMvcWebApiCompatShim(string packageVersion) =>
            Create("Microsoft.AspNetCore.Mvc.WebApiCompatShim", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetCoreRoutingAbstractions(string packageVersion) =>
            Create("Microsoft.AspNetCore.Routing.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftAspNetMvc(string packageVersion) =>
            Create("Microsoft.AspNet.Mvc", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftExtensionsConfigurationAbstractions(string packageVersion) =>
            Create("Microsoft.Extensions.Configuration.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftExtensionsDependencyInjectionAbstractions(string packageVersion) =>
            Create("Microsoft.Extensions.DependencyInjection.Abstractions", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftExtensionsLoggingPackages(string packageVersion) =>
            Create("Microsoft.Extensions.Logging", packageVersion)
            .Concat(Create("Microsoft.Extensions.Logging.AzureAppServices", packageVersion))
            .Concat(Create("Microsoft.Extensions.Logging.Abstractions", packageVersion))
            .Concat(Create("Microsoft.Extensions.Logging.Console", packageVersion))
            .Concat(Create("Microsoft.Extensions.Logging.Debug", packageVersion))
            .Concat(Create("Microsoft.Extensions.Logging.EventLog", packageVersion));

        public static IEnumerable<MetadataReference> MicrosoftExtensionsOptions(string packageVersion) =>
            Create("Microsoft.Extensions.Options", packageVersion);

        public static IEnumerable<MetadataReference> NLog(string packageVersion) =>
            Create("NLog", packageVersion);

        public static IEnumerable<MetadataReference> SerilogPackages(string packageVersion) =>
            Create("Serilog", packageVersion)
            .Concat(Create("Serilog.Sinks.Console", packageVersion));

        public static IEnumerable<MetadataReference> SystemDataSqlServerCe(string packageVersion) =>
            Create("Microsoft.SqlServer.Compact", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftEntityFrameworkCore(string packageVersion) =>
            Create("Microsoft.EntityFrameworkCore", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftEntityFrameworkCoreRelational(string packageVersion) =>
            Create("Microsoft.EntityFrameworkCore.Relational", packageVersion);

        public static IEnumerable<MetadataReference> MicrosoftNetHttpHeaders(string packageVersion) =>
            Create("Microsoft.Net.Http.Headers", packageVersion);

        public static IEnumerable<MetadataReference> MSTestTestFramework(string packageVersion) =>
            Create("MSTest.TestFramework", packageVersion);

        public static IEnumerable<MetadataReference> NUnit(string packageVersion) =>
            Create("NUnit", packageVersion);

        public static IEnumerable<MetadataReference> SystemCollectionsImmutable(string packageVersion) =>
            Create("System.Collections.Immutable", packageVersion);

        public static IEnumerable<MetadataReference> SystemThreadingTasksExtensions(string packageVersion) =>
            Create("System.Threading.Tasks.Extensions", packageVersion);

        public static IEnumerable<MetadataReference> SystemValueTuple(string packageVersion) =>
            Create("System.ValueTuple", packageVersion);

        public static IEnumerable<MetadataReference> XunitFramework(string packageVersion) =>
            Create("xunit.assert", packageVersion)
            .Concat(Create("xunit.extensibility.core", packageVersion));
    }
}
