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
using System.IO;
using System.Linq;
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
        private const string PackageVersionPrefix = "Sonar.";

        private class Package
        {
            private readonly string id;
            private readonly string runtime;
            private readonly string version;

            public Package(string id, string version, string runtime)
            {
                this.id = id;
                this.runtime = runtime;
                this.version = version == Constants.NuGetLatestVersion
                    ? GetLatestVersion().Result
                    : version;
            }

            public string PackageDirectory()
            {
                var runtimePath = runtime == null ? string.Empty : $"runtimes\\{runtime}\\";
                var combinedPath = Path.Combine(PackagesFolder, id, $"{PackageVersionPrefix}{version}", runtimePath);
                return Path.GetFullPath(combinedPath);
            }

            public void EnsurePackageIsInstalled()
            {
                // Check to see if the specific package is already installed
                var packageDir = PackageDirectory();
                if (!Directory.Exists(packageDir))
                {
                    LogMessage($"Package not found at {packageDir}, will attempt to download and install.");
                    InstallPackageAsync().Wait();
                }
            }

            private async Task InstallPackageAsync()
            {
                var packageDirectory = PackageDirectory();
                var resource = await GetNuGetRepository();
                using var packageStream = new MemoryStream();
                await resource.CopyNupkgToStreamAsync(id, new NuGetVersion(version), packageStream, new SourceCacheContext(), NullLogger.Instance, default);
                using var packageReader = new PackageArchiveReader(packageStream);
                var dllFiles = packageReader.GetFiles().Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToArray();
                if (dllFiles.Any())
                {
                    foreach (var dllFile in dllFiles)
                    {
                        packageReader.ExtractFile(dllFile, $"{packageDirectory}\\{dllFile}", NullLogger.Instance);
                    }
                }
                else
                {
                    throw new ApplicationException($"Test setup error: required dlls files are missing in the downloaded package. Package: {id} Runtime: {runtime}");
                }
            }

            private static async Task<FindPackageByIdResource> GetNuGetRepository()
            {
                var nugetOrgUrl = Settings.LoadSpecificSettings(NugetConfigFolderRelativePath, "nuget.config")
                                          .GetSection("packageSources").Items.OfType<AddItem>()
                                          .Single(x => x.Key == "nuget.org").Value;
                var repository = Repository.Factory.GetCoreV3(nugetOrgUrl);
                return await repository.GetResourceAsync<FindPackageByIdResource>();
            }

            private async Task<string> GetLatestVersion()
            {
                const int VersionCheckDays = 5;
                var path = Path.Combine(PackagesFolder, id, "Sonar.Latest.txt");
                var (nextCheck, latest) = File.Exists(path) && File.ReadAllText(path).Split(';') is var values && DateTime.TryParse(values[0], out var nextCheckValue)
                    ? (nextCheckValue, values[1])
                    : (DateTime.MinValue, null);
                LogMessage($"Next check for latest NuGets: {nextCheck}");
                if (nextCheck < DateTime.Now)
                {
                    var resource = await GetNuGetRepository();
                    var versions = await resource.GetAllVersionsAsync(id, new SourceCacheContext(), NullLogger.Instance, default);
                    latest = versions.OrderByDescending(x => x.Version).First(x => !x.IsPrerelease).OriginalVersion;
                    File.WriteAllText(path, $"{DateTime.Today.AddDays(VersionCheckDays):yyyy-MM-dd};{latest}");
                }
                return latest;
            }

            private static void LogMessage(string message) =>
                Console.WriteLine($"[{DateTime.Now}] Test setup: {message}");
        }
    }
}
