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
            private string Version { get; }
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
                var combinedPath = Path.Combine(PackagesFolder, $@"{Id}.{Version}", runtimePath, "lib");
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
                    if (!Directory.Exists(packageDir))
                    {
                        throw new ApplicationException($"Test setup error: folder for downloaded package does not exist. Folder: {packageDir}");
                    }
                }
            }

            private async Task InstallPackageAsync()
            {
                var nugetOrgUrl = Settings.LoadSpecificSettings(NugetConfigFolderRelativePath, "nuget.config")
                                          .GetSection("packageSources").Items.OfType<AddItem>()
                                          .Where(ai => ai.Key == "nuget.org")
                                          .Select(ai => ai.Value)
                                          .Single();
                var repository = Repository.Factory.GetCoreV3(nugetOrgUrl);
                var context = new SourceCacheContext();
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

                NuGetVersion version;
                if (Version == Constants.NuGetLatestVersion)
                {
                    var versions = await resource.GetAllVersionsAsync(
                        Id,
                        context,
                        NullLogger.Instance,
                        CancellationToken.None);
                    version = versions.OrderByDescending(x => x.Version).First(x => !x.IsPrerelease);
                }
                else
                {
                    version = new NuGetVersion(Version);
                }

                using var packageStream = new MemoryStream();
                await resource.CopyNupkgToStreamAsync(
                    Id,
                    version,
                    packageStream,
                    context,
                    NullLogger.Instance,
                    CancellationToken.None);

                var packageDirectory = PackageDirectory();
                using var packageReader = new PackageArchiveReader(packageStream);

                foreach (var dllFile in packageReader.GetFiles().Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    packageReader.ExtractFile(dllFile, $"{packageDirectory}\\{dllFile}", NullLogger.Instance);
                }
            }

            private static void LogMessage(string message) =>
                Console.WriteLine($"[{DateTime.Now}] Test setup: {message}");
        }
    }
}
