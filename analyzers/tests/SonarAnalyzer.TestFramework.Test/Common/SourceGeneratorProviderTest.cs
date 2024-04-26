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

#if NET

    [TestMethod]
    public void SourceGenerators_ContainsRazorSourceGenerator() =>
        SourceGeneratorProvider.SourceGenerators.Should().ContainSingle().Which.FullPath.Should().EndWith("Microsoft.CodeAnalysis.Razor.Compiler.SourceGenerators.dll");

    [TestMethod]
    public void RazorSourceGenerator_HasCorrectPath() =>
        RazorSourceGenerator.FullPath
            .Should().Be(Path.Combine(SourceGeneratorProvider.LatestSdkFolder(), "Sdks", "Microsoft.NET.Sdk.Razor", "source-generators", "Microsoft.CodeAnalysis.Razor.Compiler.SourceGenerators.dll"));

    [TestMethod]
    public void RazorSourceGenerator_LoadsCorrectAssembly() =>
        RazorSourceGenerator.GetAssembly().GetName().Name.Should().Be("Microsoft.CodeAnalysis.Razor.Compiler.SourceGenerators");

    [TestMethod]
    public void LatestSdkFolder_ReturnsCorrectPath()
    {
        var expectedPath = Directory.GetDirectories(Path.Combine(Directory.GetParent(typeof(object).Assembly.Location).Parent.Parent.Parent.FullName, "sdk"), $"{typeof(object).Assembly.GetName().Version.Major}.*", SearchOption.TopDirectoryOnly)
                                    .OrderByDescending(dir => new DirectoryInfo(dir).Name)
                                    .FirstOrDefault();
        SourceGeneratorProvider.LatestSdkFolder().Should().Be(expectedPath);
    }

    private static AnalyzerFileReference RazorSourceGenerator =>
        SourceGeneratorProvider.SourceGenerators.Single(x => x.FullPath.EndsWith("Microsoft.CodeAnalysis.Razor.Compiler.SourceGenerators.dll"));

#else

    [TestMethod]
    public void SourceGenerators_NetFramework_Throws()
    {
        Action action = () => { SourceGeneratorProvider.LatestSdkFolder(); };
        action.Should().Throw<TypeInitializationException>();
    }

#endif

}
