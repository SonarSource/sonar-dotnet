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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    public static partial class NuGetMetadataFactory
    {
        private class Package
        {
            public string Id { get; }
            public string Version { get; }
            public string Runtime { get; }

            public Package(string id, string version, string runtime)
            {
                Id = id;
                Version = version;
                Runtime = runtime;
            }

            public string PackageDirectory()
            {
                var runtimePath = Runtime == null ? string.Empty : $"runtimes\\{Runtime}\\";
                var versionFolder = Version == Constants.NuGetLatestVersion
                        ? SortedPackageFolders().Select(x => Path.GetFileName(x).Substring(Id.Length + 1)).Last(x => char.IsNumber(x[0]))
                        : Version;
                var x = $@"{PackagesFolderRelativePath}{Id}.{versionFolder}\{runtimePath}lib";
                return Path.GetFullPath(x);
            }

            public void EnsurePackageIsInstalled()
            {
                if (Version == Constants.NuGetLatestVersion)
                {
                    if (IsCheckForLatestPackageRequired())
                    {
                        Install();
                        WriteNextCheckTime();
                    }
                    else
                    {
                        LogMessage($"Skipping check for latest NuGet since checked recently: {Id}");
                    }
                }
                else
                {
                    var packageDir = PackageDirectory();
                    if (Directory.Exists(packageDir))
                    {
                        LogMessage($"Package found at {packageDir}");
                    }
                    else
                    {
                        LogMessage($"Package not found at {packageDir}");
                        Install();
                    }
                }
            }

            private void Install()
            {
                var versionArgument = Version == Constants.NuGetLatestVersion ? string.Empty : $"-Version {Version}";
                var configFile = ValidatedNuGetConfigPath();
                // Explicitly specify the NuGet config to use to avoid being impacted by the NuGet config on the machine running the tests
                var args = $"install {Id} {versionArgument} -OutputDirectory {Path.GetFullPath(PackagesFolderRelativePath)} -NonInteractive -ForceEnglishOutput -ConfigFile {configFile}";
                LogMessage($"Installing package using nuget.exe {args}");
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

                return Directory.Exists(PackagesFolderRelativePath)
                    ? Directory.GetDirectories(PackagesFolderRelativePath, $"{Id}.*", SearchOption.TopDirectoryOnly).Where(x => matcher.IsMatch(x)).OrderBy(x => x)
                    : Enumerable.Empty<string>();
            }

            private static string ValidatedNuGetConfigPath()
            {
                var path = Path.GetFullPath(NuGetConfigFileRelativePath);
                if (!File.Exists(path))
                {
                    throw new ApplicationException($"Test setup error: failed to find nuget.config file at \"{path}\"");
                }
                LogMessage($"Path to nuget.config: {path}");
                return path;
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
                const string NextUpdateFileName = "NextCheckedForUpdate.txt";
                var directory = SortedPackageFolders().LastOrDefault();
                return directory == null ? null : Path.Combine(directory, NextUpdateFileName);
            }
        }
    }
}
