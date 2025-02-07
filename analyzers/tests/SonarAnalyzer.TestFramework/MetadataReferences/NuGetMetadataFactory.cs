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

namespace SonarAnalyzer.TestFramework.MetadataReferences;

internal static partial class NuGetMetadataFactory
{
    // We use the global nuget cache for storing our packages if the NUGET_PACKAGES environment variable is defined.
    // This is especially helpful on the build agents where the packages are precached
    // (since we don't need to spawn a new process for calling the nuget.exe to install or copy them from global cache)
    private static readonly string PackagesFolder;

    private static readonly string[] SortedAllowedDirectories =
    {
        "net",
        "netstandard2.1",
        "netstandard2.0",
        "net7.0",
        "net6.0",
        "net8.0",
        "net9.0",
        "net47",
        "net461",
        "netstandard1.6",
        "netstandard1.5",
        "netstandard1.3",
        "netstandard1.1",
        "netstandard1.0",
        "net451",
        "net45",
        "net40",
        "net20",
        "portable-net45",
        "lib", // This has to be last, some packages have DLLs directly in "lib" directory
    };

    static NuGetMetadataFactory()
    {
        PackagesFolder = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Paths.AnalyzersRoot, "packages");
        LogMessage($"Using packages from {PackagesFolder}");
    }

    /// <param name="dllDirectory">Name of the directory containing DLL files inside *.nupgk/lib/{dllDirectory}/ or *.nupgk/runtimes/{runtime}/lib/{dllDirectory}/ folder.
    /// This directory name represents target framework in most cases.</param>
    public static IEnumerable<MetadataReference> Create(string packageId, string packageVersion, string runtime, string dllDirectory) =>
        Create(new Package(packageId, packageVersion, runtime), new[] { dllDirectory });

    public static IEnumerable<MetadataReference> Create(string packageId, string packageVersion, string runtime = null) =>
        Create(new Package(packageId, packageVersion, runtime), SortedAllowedDirectories);

    /// <param name="allowedDirectories">List of allowed directories sorted by preference to search for DLL files.</param>
    private static IEnumerable<MetadataReference> Create(Package package, string[] allowedDirectories)
    {
        if (package.Id == "Microsoft.Build.NoTargets")
        {
            return Enumerable.Empty<MetadataReference>();
        }

        var packageDir = package.EnsureInstalled();
        // some packages (see Mono.Posix.NETStandard.1.0.0) may contain target framework only in ref folder
        var dllsPerDirectory = Directory.GetFiles(packageDir, "*.dll", SearchOption.AllDirectories)
                                        .GroupBy(x => Path.GetDirectoryName(x).Split('+').First())
                                        .Select(x => (directory: Path.GetFileName(x.Key), dllPaths: x.AsEnumerable()))
                                        .ToArray();

        foreach (var allowedDirectory in allowedDirectories)
        {
            // dllsPerDirectory can contain the same <directory> from \lib\<directory> and \ref\<directory>. We don't care who wins.
            if (dllsPerDirectory.Where(x => x.directory == allowedDirectory).Select(x => x.dllPaths).FirstOrDefault() is { } dllPaths)
            {
                foreach (var dllPath in dllPaths)
                {
                    LogMessage("File: " + dllPath);
                }
                return dllPaths.Select(x => MetadataReference.CreateFromFile(x)).ToArray();
            }
        }

        throw new InvalidOperationException($"No allowed DLL directory was found in {packageDir}. Add new target framework to SortedAllowedDirectories or set dllDirectory argument explicitly.");
    }

    private static void LogMessage(string message) =>
        Console.WriteLine($"Test setup: {message}");
}
