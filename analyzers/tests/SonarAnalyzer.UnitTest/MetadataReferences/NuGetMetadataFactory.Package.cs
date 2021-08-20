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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    public static partial class NuGetMetadataFactory
    {
        private class Package
        {
            private readonly string id;
            private readonly string runtime;
            private string version;

            public Package(string id, string version, string runtime)
            {
                this.id = id;
                this.version = version;
                this.runtime = runtime;
            }

            public string PackageDirectory()
            {
                var runtimePath = runtime == null ? string.Empty : $"runtimes\\{runtime}\\";
                var combinedPath = Path.Combine(PackagesFolder, id, version, runtimePath);
                return Path.GetFullPath(combinedPath);
            }

            public void EnsurePackageIsInstalled()
            {
                if (version == Constants.NuGetLatestVersion)
                {
                    version = GetLatestVersion().Result;
                }

                // Check to see if the specific package is already installed
                var packageDir = PackageDirectory();
                if (!Directory.Exists(packageDir))
                {
                    LogMessage($"Package not found at {packageDir}, will attempt to download and install.");
                    InstallPackageAsync().Wait();
                    if (!Directory.Exists(packageDir))
                    {
                        throw new ApplicationException($"Test setup error: folder for downloaded package does not exist. Folder: {packageDir}");
                    }
                }
            }

            private async Task InstallPackageAsync()
            {
                var resource = await GetNuGetRepository();

                using var packageStream = new MemoryStream();
                await resource.CopyNupkgToStreamAsync(id, new NuGetVersion(version), packageStream, new SourceCacheContext(), NullLogger.Instance, default);

                var packageDirectory = PackageDirectory();
                using var packageReader = new PackageArchiveReader(packageStream);

                foreach (var dllFile in packageReader.GetFiles().Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    packageReader.ExtractFile(dllFile, $"{packageDirectory}\\{dllFile}", NullLogger.Instance);
                }
                WriteNextCheckTime();
            }

            private static async Task<FindPackageByIdResource> GetNuGetRepository()
            {
                var nugetOrgUrl = Settings.LoadSpecificSettings(NugetConfigFolderRelativePath, "nuget.config")
                                          .GetSection("packageSources").Items.OfType<AddItem>()
                                          .Where(ai => ai.Key == "nuget.org")
                                          .Select(ai => ai.Value)
                                          .Single();
                var repository = Repository.Factory.GetCoreV3(nugetOrgUrl);
                return await repository.GetResourceAsync<FindPackageByIdResource>();
            }

            /// <summary>
            /// Returns the list of folders containing installed versions of the specified package,
            /// or an empty list if the package is not installed.
            /// </summary>
            /// <remarks>
            /// Package directory names are in the form "{package id}\{package version}".
            /// The list is sorted in ascending order, so the most recent version will be last.
            /// </remarks>
            private IEnumerable<string> SortedPackageFolders()
            {
                // The package will be in a folder called "\packages\{packageId}\{version}", but:
                // : the package might not be installed
                // : there might be multiple versions installed
                // : there might be a package that starts with the same package id
                //      e.g. Microsoft.AspNetCore.Core and Microsoft.AspNetCore.Core.Diagnostics
                // Most packages have a three-part version, but some have four. We don't check
                // the actual number of parts, as long as there is at least one.
                var matcher = new Regex(@"(\.\d+)+$", RegexOptions.IgnoreCase);
                var packagePath = Path.Combine(PackagesFolder, id);
                return Directory.Exists(packagePath)
                    ? Directory.GetDirectories(packagePath, "*.*", SearchOption.TopDirectoryOnly).Where(x => matcher.IsMatch(x)).OrderBy(x => x)
                    : Enumerable.Empty<string>();
            }

            private async Task<string> GetLatestVersion()
            {
                var latest = SortedPackageFolders().LastOrDefault(x => char.IsNumber(x[0]));
                if (latest == null || IsCheckForLatestPackageRequired())
                {
                    var resource = await GetNuGetRepository();
                    var versions = await resource.GetAllVersionsAsync(id, new SourceCacheContext(), NullLogger.Instance, default);
                    return versions.OrderByDescending(x => x.Version).First(x => !x.IsPrerelease).OriginalVersion;
                }

                return latest;
            }

            private bool IsCheckForLatestPackageRequired()
            {
                // Install new nugets only once per day to improve the performance when running tests locally.
                var nextCheck = NextCheckFilePath() is { } filePath && File.Exists(filePath) && DateTime.TryParse(File.ReadAllText(filePath), out var timestamp)
                    ? timestamp
                    : DateTime.MinValue;
                LogMessage($"Next check for latest NuGets: {nextCheck}");
                return nextCheck < DateTime.Now;
            }

            private void WriteNextCheckTime()
            {
                const int VersionCheckDays = 5;
                var filePath = NextCheckFilePath();
                if (filePath == null)
                {
                    return;
                }
                File.WriteAllText(filePath, DateTime.Now.AddDays(VersionCheckDays).ToString("yyyy-MM-dd"));
            }

            private string NextCheckFilePath()
            {
                // The file containing the next-check timestamp is stored in folder of the latest version of the package.
                const string NextUpdateFileName = "NextCheckForUpdate.txt";
                var directory = SortedPackageFolders().LastOrDefault();
                return directory == null ? null : Path.Combine(directory, "..", NextUpdateFileName);
            }

            private static void LogMessage(string message) =>
                Console.WriteLine($"[{DateTime.Now}] Test setup: {message}");
        }
    }
}
