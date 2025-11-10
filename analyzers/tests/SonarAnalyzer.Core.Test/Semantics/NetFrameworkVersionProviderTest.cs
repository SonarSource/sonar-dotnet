/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using System.IO;

namespace SonarAnalyzer.Core.Test.Semantics;

[TestClass]
public class NetFrameworkVersionProviderTest
{
    [TestMethod]
    public void NetFrameworkVersionProvider_WithNullCompilation_ReturnsUnknown()
    {
        var versionProvider = new NetFrameworkVersionProvider();
        versionProvider.Version(null).Should().Be(NetFrameworkVersion.Unknown);
    }

    [TestMethod]
    [DataRow("3.5", NetFrameworkVersion.Probably35)]
    [DataRow("4.0_no_IO", NetFrameworkVersion.Between4And451)]
    [DataRow("4.0_with_IO", NetFrameworkVersion.Between4And451)]
    [DataRow("4.8", NetFrameworkVersion.After452)]
    public void Version_MockedFramework(string name, NetFrameworkVersion expected)
    {
        var compilation = CreateRawCompilation(MetadataReference.CreateFromFile(Path.Combine(Paths.TestsRoot, "FrameworkMocks", "lib", name, "mscorlib.dll")));
        var versionProvider = new NetFrameworkVersionProvider();
        versionProvider.Version(compilation).Should().Be(expected);
    }

    [TestMethod]
    public void NetFrameworkVersionProvider_NoReference()
    {
        var compilation = CreateRawCompilation();
        var versionProvider = new NetFrameworkVersionProvider();
        versionProvider.Version(compilation).Should().Be(NetFrameworkVersion.Unknown);
    }

    private static Compilation CreateRawCompilation(params MetadataReference[] references)
    {
        var solution = new AdhocWorkspace().CurrentSolution;
        var project = solution.AddProject("Test", "Test", LanguageNames.CSharp);
        var projectBuilder = ProjectBuilder.FromProject(project);
        return projectBuilder.AddReferences(references).GetCompilation();   // Do not add default references from SnippetCompiler
    }
}
