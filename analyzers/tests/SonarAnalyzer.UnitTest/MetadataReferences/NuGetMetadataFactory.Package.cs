﻿/*
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    public static partial class NuGetMetadataFactory
    {
        private static string GetNuGetPackageDirectory(string packageId, string packageVersion, string runtime)
        {
            var runtimePath = runtime == null ? string.Empty : $"runtimes\\{runtime}\\";
            var x = $@"{PackagesFolderRelativePath}{packageId}.{GetRealVersionFolder(packageId, packageVersion)}\{runtimePath}lib";
            return Path.GetFullPath(x);
        }

        private static void EnsurePackageIsInstalled(string packageId, string packageVersion, string runtime)
        {
            if (packageVersion == Constants.NuGetLatestVersion)
            {
                if (IsCheckForLatestPackageRequired(packageId))
                {
                    LogMessage($"Checking for newer version of package: {packageId}");
                    InstallWithCommandLine(packageId, packageVersion);
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
                var packageDir = GetNuGetPackageDirectory(packageId, packageVersion, runtime);
                if (Directory.Exists(packageDir))
                {
                    LogMessage($"Package found at {packageDir}");
                }
                else
                {
                    LogMessage($"Package not found at {packageDir}");
                    InstallWithCommandLine(packageId, packageVersion);
                }
            }
        }

        private static void InstallWithCommandLine(string packageId, string packageVersion)
        {
            var versionArgument = packageVersion == Constants.NuGetLatestVersion
                ? string.Empty
                : $"-Version {packageVersion}";

            var nugetConfigPath = GetValidatedNuGetConfigPath();

            var args = $"install {packageId} {versionArgument} -OutputDirectory {Path.GetFullPath(PackagesFolderRelativePath)} -NonInteractive -ForceEnglishOutput" +
                // Explicitly specify the NuGet config to use to avoid being impacted by
                // the NuGet config on the machine running the tests
                $" -ConfigFile {nugetConfigPath}";
            LogMessage("Installing package using nuget.exe:");
            LogMessage($"\tArgs: {args}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "nuget.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (s, e) => LogMessage($"  nuget.exe: {e.Data}");
                process.ErrorDataReceived += OnErrorDataReceived;

                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new ApplicationException($"Test setup error: failed to download package using nuget.exe. Exit code: {process.ExitCode}");
                }
            }

            static void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    LogMessage($"  nuget.exe: ERROR: {e.Data}");
                }
            }
        }

        private static string GetRealVersionFolder(string packageId, string packageVersion) =>
            packageVersion != Constants.NuGetLatestVersion
                ? packageVersion
                : GetSortedPackageFolders(packageId)
                    .Select(path => Path.GetFileName(path).Substring(packageId.Length + 1))
                    .Last(path => char.IsNumber(path[0]));

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
            var matcher = new Regex($@"{Regex.Escape(packageId)}(\.\d+)+$", RegexOptions.IgnoreCase);

            return Directory.Exists(PackagesFolderRelativePath)
                ? Directory.GetDirectories(PackagesFolderRelativePath, $"{packageId}.*", SearchOption.TopDirectoryOnly)
                    .Where(path => matcher.IsMatch(path))
                    .OrderBy(name => name)
                    .ToArray()
                : Enumerable.Empty<string>();
        }

        private static string GetValidatedNuGetConfigPath()
        {
            var path = Path.GetFullPath(NuGetConfigFileRelativePath);
            if (!File.Exists(path))
            {
                throw new ApplicationException($"Test setup error: failed to find nuget.config file at \"{path}\"");
            }
            LogMessage($"Path to nuget.config: {path}");
            return path;
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

        private static DateTime GetLastCheckTime(string packageId) =>
            GetLastCheckFilePath(packageId) is { } filePath
            && File.Exists(filePath)
            && DateTime.TryParse(File.ReadAllText(filePath), out var timestamp)
            ? timestamp
            : DateTime.MinValue;

        private static void WriteLastUpdateFile(string packageId)
        {
            var filePath = GetLastCheckFilePath(packageId);
            if (filePath == null)
            {
                return;
            }
            File.WriteAllText(filePath, DateTime.Now.ToString("d")); // short date pattern
        }

        private static string GetLastCheckFilePath(string packageId)
        {
            // The file containing the last-check timestamp is stored in folder of the latest version of the package.
            const string LastUpdateFileName = "LastCheckedForUpdate.txt";

            var directory = GetSortedPackageFolders(packageId).LastOrDefault();
            return directory == null ? null : Path.Combine(directory, LastUpdateFileName);
        }
    }
}
