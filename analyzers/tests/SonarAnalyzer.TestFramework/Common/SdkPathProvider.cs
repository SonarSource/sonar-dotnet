/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Reflection;

namespace SonarAnalyzer.TestFramework.Common;

public static class SdkPathProvider
{
    // Due to the preview versions naming convention, we cannot use the folder names as version numbers so we need to look at the file versions of the assemblies from the folder.
    // When reading the dll versions, the File version (e.g. 9.1.24.40712) is considered instead of the Product version (e.g. 9.1.100-preview.7.24407.12+hash).
    // FixMe: SDK pinned to 10.0.204 until https://sonarsource.atlassian.net/browse/NET-3786 is fixed.
    // Runtime folders (AspNetCore, WindowsDesktop) use a different version (e.g. 10.0.4) so they keep the wildcard pattern.
    private const string SdkVersion = "10.0.204";
    private const string RuntimeVersionPattern = "10.*";

    private static readonly string RazorSourceGeneratorPath = Path.Combine(LatestSdkFolder(), "Sdks", "Microsoft.NET.Sdk.Razor", "source-generators", "Microsoft.CodeAnalysis.Razor.Compiler.dll");

    public static AnalyzerFileReference[] SourceGenerators { get; } =
    [
        new(RazorSourceGeneratorPath, new AssemblyLoader())
    ];

    public static string LatestSdkFolder() =>
        LatestFolder(TestConstants.SdkPath, "dotnet.dll", SdkVersion);

    public static string LatestAspNetCoreSdkFolder() =>
        LatestFolder(TestConstants.AspNetCorePath, "Microsoft.AspNetCore.dll", RuntimeVersionPattern);

    public static string LatestWindowsDesktopSdkFolder() =>
        LatestFolder(TestConstants.WindowsDesktopPath, "PresentationCore.dll", RuntimeVersionPattern);

    public static string LatestFolder(string path, string assemblyName, string versionPattern)
    {
        if (!Directory.Exists(path))
        {
            throw new NotSupportedException(
                $"The directory '{path}' does not exist. This may be because you are not using .NET Core. Please note that Razor analysis is only supported when using .NET Core.");
        }
        return Directory.GetDirectories(path, versionPattern).OrderBy(x => FromFolderName(x, assemblyName)).Last();

        static Version FromFolderName(string folderName, string assemblyName)
        {
            var fv = FileVersionInfo.GetVersionInfo(Path.Combine(folderName, assemblyName));
            return new Version(fv.FileMajorPart, fv.FileMinorPart, fv.FileBuildPart, fv.FilePrivatePart);
        }
    }

    private sealed class AssemblyLoader : IAnalyzerAssemblyLoader
    {
        public void AddDependencyLocation(string fullPath) { }

        public Assembly LoadFromPath(string fullPath) =>
            Assembly.LoadFrom(fullPath);
    }
}
