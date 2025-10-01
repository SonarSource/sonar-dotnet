/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Globalization;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace SonarAnalyzer.TestFramework.MetadataReferences;

internal static partial class NuGetMetadataFactory
{
    private const string PackageVersionPrefix = "Sonar.";

    private sealed class Package
    {
        private readonly string runtime;
        private readonly string version;

        public string Id { get; }

        public Package(string id, string version, string runtime)
        {
            Id = id;
            this.runtime = runtime;
            this.version = version == TestConstants.NuGetLatestVersion
                ? LatestVersion().Result
                : version;
        }

        public string EnsureInstalled()
        {
            // Check to see if the specific package is already installed
            var packageDir = Path.GetFullPath(Path.Combine(PackagesFolder, Id, PackageVersionPrefix + version, runtime is null ? string.Empty : $@"runtimes\{runtime}\"));
            if (!Directory.Exists(packageDir))
            {
                LogMessage($"Package not found at {packageDir}, will attempt to download and install.");
                InstallPackageAsync(packageDir).Wait();
            }

            return packageDir;
        }

        private async Task InstallPackageAsync(string packageDir)
        {
            var resource = await NuGetRepository().ConfigureAwait(false);
            using var packageStream = new MemoryStream();
            await resource.CopyNupkgToStreamAsync(Id, new NuGetVersion(version), packageStream, new SourceCacheContext(), NullLogger.Instance, default).ConfigureAwait(false);
            using var packageReader = new PackageArchiveReader(packageStream);
            var dllFiles = packageReader.GetFiles().Where(x => x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (dllFiles.Any())
            {
                foreach (var dllFile in dllFiles)
                {
                    packageReader.ExtractFile(dllFile, $"{packageDir}\\{dllFile}", NullLogger.Instance);
                }
            }
            else
            {
                throw new InvalidOperationException($"Test setup error: required dlls files are missing in the downloaded package. Package: {Id} Runtime: {runtime}");
            }
        }

        private static async Task<FindPackageByIdResource> NuGetRepository()
        {
            // We don't read and verify trustedSigners. These packages are stored in dedicated directories that are not used by regular NuGet restore.
            var source = SettingsFileName() is { } fileName
                ? PackageSourceProvider.LoadPackageSources(Settings.LoadSpecificSettings(Paths.AnalyzersRoot, fileName)).Single()   // CI, or Sonar local machine
                : new PackageSource("https://api.nuget.org/v3/index.json");                                                         // External contributor local machine
            return await Repository.Factory.GetCoreV3(source).GetResourceAsync<FindPackageByIdResource>().ConfigureAwait(false);

            static string SettingsFileName()
            {
                if (File.Exists(Path.Combine(Paths.ProjectRoot, "CI.NuGet.Config")))
                {
                    return TestEnvironment.IsAzureDevOpsContext ? "CI.NuGet.Config" : null;
                }
                else
                {
                    return "NuGet.Config";
                }
            }
        }

        private async Task<string> LatestVersion()
        {
            const int VersionCheckDays = 5;
            var path = Path.Combine(PackagesFolder, Id, "Sonar.Latest.txt");
            var (nextCheck, latest) =
                File.Exists(path)
                && File.ReadAllText(path).Split(';') is var values
                && DateTime.TryParseExact(values[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var nextCheckValue)
                    ? Pair.From(nextCheckValue, values[1])
                    : new(DateTime.MinValue, null);
            LogMessage($"Next check for latest NuGets: {nextCheck}");
            if (nextCheck < DateTime.Now)
            {
                var resource = await NuGetRepository().ConfigureAwait(false);
                var versions = await resource.GetAllVersionsAsync(Id, new SourceCacheContext(), NullLogger.Instance, default).ConfigureAwait(false);
                latest = versions.OrderByDescending(x => x.Version).First(x => !x.IsPrerelease).OriginalVersion;
                new FileInfo(path).Directory.Create(); // Ensure that folder exists, if not create one
                File.WriteAllText(path, $"{DateTime.Today.AddDays(VersionCheckDays):yyyy-MM-dd};{latest}");
            }
            return latest;
        }
    }
}
