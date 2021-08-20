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
using System.Threading;
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
            private string Id { get; }
            private string Version { get; set; }
            private string Runtime { get; }

            public Package(string id, string version, string runtime)
            {
                Id = id;
                Version = version;
                Runtime = runtime;
            }

            public string PackageDirectory()
            {
                var runtimePath = Runtime == null ? string.Empty : $"runtimes\\{Runtime}\\";
                var combinedPath = Path.Combine(PackagesFolder, $@"{Id}.{Version}", runtimePath);
                return Path.GetFullPath(combinedPath);
            }

            public void EnsurePackageIsInstalled()
            {
                var writeTimeStamp = false;
                if (Version == Constants.NuGetLatestVersion)
                {
                    Version = GetLatestVersion().Result;
                    writeTimeStamp = true;
                }

                // Check to see if the specific package is already installed
                var packageDir = PackageDirectory();
                if (!Directory.Exists(packageDir))
                {
                    LogMessage($"Package not found at {packageDir}, will attempt to download and install.");
                    InstallPackageAsync(writeTimeStamp).Wait();
                    if (!Directory.Exists(packageDir))
                    {
                        throw new ApplicationException($"Test setup error: folder for downloaded package does not exist. Folder: {packageDir}");
                    }
                }
            }

            private async Task InstallPackageAsync(bool writeTimeStamp)
            {
                var resource = await GetNuGetRepository();

                using var packageStream = new MemoryStream();
                await resource.CopyNupkgToStreamAsync(
                    Id,
                    new NuGetVersion(Version),
                    packageStream,
                    new SourceCacheContext(),
                    NullLogger.Instance,
                    CancellationToken.None);

                var packageDirectory = PackageDirectory();
                using var packageReader = new PackageArchiveReader(packageStream);

                foreach (var dllFile in packageReader.GetFiles().Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    packageReader.ExtractFile(dllFile, $"{packageDirectory}\\{dllFile}", NullLogger.Instance);
                }

                if (writeTimeStamp)
                {
                    WriteNextCheckTime();
                }
            }

            private static async Task<FindPackageByIdResource> GetNuGetRepository()
            {
                var nugetOrgUrl = Settings.LoadSpecificSettings(NugetConfigFolderRelativePath,
                                              "nuget.config")
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
            /// Package directory names are in the form "{package id}.{package version}".
            /// The list is sorted in ascending order, so the most recent version will be last.
            /// </remarks>
            private IEnumerable<string> SortedPackageFolders()
            {
                // The package will be in a folder called "\packages\{packageId}.{version}", but:
                // : the package might not be installed
                // : there might be multiple versions installed
                // : there might be a package that starts with the same package id
                //      e.g. Microsoft.AspNetCore.Core and Microsoft.AspNetCore.Core.Diagnostics
                // Most packages have a three-part version, but some have four. We don't check
                // the actual number of parts, as long as there is at least one.
                var matcher = new Regex($@"{Regex.Escape(Id)}(\.\d+)+$", RegexOptions.IgnoreCase);

                return Directory.Exists(PackagesFolder)
                    ? Directory.GetDirectories(PackagesFolder, $"{Id}.*", SearchOption.TopDirectoryOnly).Where(x => matcher.IsMatch(x)).OrderBy(x => x)
                    : Enumerable.Empty<string>();
            }

            private async Task<string> GetLatestVersion()
            {
                var latest = SortedPackageFolders().Select(x => Path.GetFileName(x).Substring(Id.Length + 1)).LastOrDefault(x => char.IsNumber(x[0]));
                if (latest == null || IsCheckForLatestPackageRequired())
                {
                    var resource =  await GetNuGetRepository();
                    var versions = await resource.GetAllVersionsAsync(
                        Id,
                        new SourceCacheContext(),
                        NullLogger.Instance,
                        CancellationToken.None);
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
                const int versionCheckDays = 5;
                var filePath = NextCheckFilePath();
                if (filePath == null)
                {
                    return;
                }
                File.WriteAllText(filePath, DateTime.Now.AddDays(versionCheckDays).ToString("yyyy-MM-dd"));
            }

            private string NextCheckFilePath()
            {
                // The file containing the next-check timestamp is stored in folder of the latest version of the package.
                const string nextUpdateFileName = "NextCheckForUpdate.txt";
                var directory = SortedPackageFolders().LastOrDefault();
                return directory == null ? null : Path.Combine(directory, nextUpdateFileName);
            }

            private static void LogMessage(string message) =>
                Console.WriteLine($"[{DateTime.Now}] Test setup: {message}");
        }
    }
}
