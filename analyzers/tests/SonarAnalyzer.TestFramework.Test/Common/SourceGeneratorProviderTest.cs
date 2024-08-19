/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.IO;

namespace SonarAnalyzer.TestFramework.Test.Common;

[TestClass]
public class SourceGeneratorProviderTest
{
    private static AnalyzerFileReference RazorSourceGenerator =>
        SourceGeneratorProvider.SourceGenerators.Single(x => x.FullPath.EndsWith("Microsoft.CodeAnalysis.Razor.Compiler.dll"));

    [TestMethod]
    public void SourceGenerators_ContainsRazorSourceGenerator() =>
        SourceGeneratorProvider.SourceGenerators.Should()
            .Contain(x => x.FullPath.EndsWith(Path.Combine("Sdks", "Microsoft.NET.Sdk.Razor", "source-generators", "Microsoft.CodeAnalysis.Razor.Compiler.dll")));

    [TestMethod]
    public void RazorSourceGenerator_ExistsLocally() =>
        File.Exists(RazorSourceGenerator.FullPath).Should().BeTrue();

    [TestMethod]
    public void RazorSourceGenerator_LoadsCorrectAssembly() =>
        RazorSourceGenerator.GetAssembly().GetName().Name.Should().Be("Microsoft.CodeAnalysis.Razor.Compiler");

    [TestMethod]
    public void LatestSdkFolder_ReturnsAssemblyMajor()
    {
        var latestSdkFolder = SourceGeneratorProvider.LatestSdkFolder();
        Version.TryParse(Path.GetFileName(latestSdkFolder), out var latestSdkVersion).Should().BeTrue($"'{latestSdkFolder}' cannot be parsed to a version number");
        latestSdkVersion.Major.Should().Be(typeof(object).Assembly.GetName().Version.Major);
    }

    [TestMethod]
    public void LatestSdkFolder_ReturnLatest()
    {
        var latestSdkFolder = SourceGeneratorProvider.LatestSdkFolder();
        Version.TryParse(Path.GetFileName(latestSdkFolder), out var latestSdkVersion).Should().BeTrue($"'{latestSdkFolder}' cannot be parsed to a version number");
        var parentDirectory = Directory.GetParent(latestSdkFolder);
        parentDirectory.Name.Should().Be("sdk", "Parent directory of the latest SDK should be 'sdk'");
        Directory.GetDirectories(parentDirectory.FullName, $"{typeof(object).Assembly.GetName().Version.Major}.*")
            .Should().NotContain(x => IsHigherVersion(x, latestSdkVersion), "There should be no SDK folders with a higher version number than the latest SDK folder");
    }

    private static bool IsHigherVersion(string directory, Version referenceVersion) =>
        Version.TryParse(new DirectoryInfo(directory).Name, out var version) && version > referenceVersion;
}
