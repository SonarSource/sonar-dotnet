/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using static SonarAnalyzer.Helpers.KnownReference;

namespace SonarAnalyzer.UnitTest;

[TestClass]
public class KnownReferenceTests
{
    [DataTestMethod]
    [DataRow("Test", true)]
    [DataRow("test", false)]
    [DataRow("TEST", false)]
    [DataRow("MyTest", false)]
    [DataRow("TestMy", false)]
    [DataRow("MyTestMy", false)]
    [DataRow("MyTESTMy", false)]
    [DataRow("Without", false)]
    public void NameIs_Test(string name, bool expected)
    {
        var sut = new KnownReference(NameIs("Test"));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity(name);
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("Test", true)]
    [DataRow("test", false)]
    [DataRow("TEST", false)]
    [DataRow("MyTest", true)]
    [DataRow("TestMy", true)]
    [DataRow("MyTestMy", true)]
    [DataRow("MyTESTMy", false)]
    [DataRow("Without", false)]
    public void NameContains_Test(string name, bool expected)
    {
        var sut = new KnownReference(Contains("Test"));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity(name);
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("Test", true)]
    [DataRow("test", false)]
    [DataRow("TEST", false)]
    [DataRow("MyTest", false)]
    [DataRow("TestMy", true)]
    [DataRow("MyTestMy", false)]
    [DataRow("MyTESTMy", false)]
    [DataRow("Without", false)]
    public void NameStartsWith_Test(string name, bool expected)
    {
        var sut = new KnownReference(StartsWith("Test"));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity(name);
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("Test", true)]
    [DataRow("test", false)]
    [DataRow("TEST", false)]
    [DataRow("MyTest", true)]
    [DataRow("TestMy", false)]
    [DataRow("MyTestMy", false)]
    [DataRow("MyTESTMy", false)]
    [DataRow("Without", false)]
    public void NameEndsWith_Test(string name, bool expected)
    {
        var sut = new KnownReference(EndsWith("Test"));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity(name);
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("1.0.0.0", false)]
    [DataRow("1.9.9.99", false)]
    [DataRow("2.0.0.0", true)]
    [DataRow("2.0.0.1", true)]
    [DataRow("2.1.0.0", true)]
    [DataRow("3.1.0.0", true)]
    public void Version_GreaterOrEqual_2_0(string version, bool expected)
    {
        var sut = new KnownReference(VersionGreaterOrEqual(new Version(2, 0)));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity("assemblyName", new Version(version));
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("1.0.0.0", true)]
    [DataRow("1.9.9.99", true)]
    [DataRow("2.0.0.0", false)]
    [DataRow("2.0.0.1", false)]
    [DataRow("2.1.0.0", false)]
    [DataRow("3.1.0.0", false)]
    public void Version_LowerThen_2_0(string version, bool expected)
    {
        var sut = new KnownReference(VersionLowerThen(new Version(2, 0)));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity("assemblyName", new Version(version));
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("1.0.0.0", false)]
    [DataRow("1.9.9.99", false)]
    [DataRow("2.0.0.0", true)]
    [DataRow("2.0.0.1", true)]
    [DataRow("2.1.0.0", true)]
    [DataRow("3.1.0.0", true)]
    [DataRow("3.4.9.99", true)]
    [DataRow("3.5.0.0", true)]
    [DataRow("3.5.0.1", false)]
    [DataRow("10.0.0.0", false)]
    public void Version_Between_2_0_and_3_5(string version, bool expected)
    {
        var sut = new KnownReference(VersionBetween(new Version(2, 0, 0, 0), new Version(3, 5, 0, 0)));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity("assemblyName", new Version(version));
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("Test", "1.0.0.0", false)]
    [DataRow("Test", "1.9.9.99", false)]
    [DataRow("TestMy", "2.0.0.0", true)]
    [DataRow("MyTest", "2.0.0.0", false)]
    [DataRow("TestMy", "3.5.0.0", true)]
    [DataRow("TestMy", "3.5.0.1", false)]
    [DataRow("Test", "10.0.0.0", false)]
    public void Combinator_NameStartWith_Test_And_Version_Between_2_0_And_3_5(string name, string version, bool expected)
    {
        var sut = new KnownReference(StartsWith("Test").And(VersionBetween(new Version(2, 0, 0, 0), new Version(3, 5, 0, 0))));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity(name, new Version(version));
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("Start", true)]
    [DataRow("End", true)]
    [DataRow("StartOrEnd", true)]
    [DataRow("StartTest", true)]
    [DataRow("TestEnd", true)]
    [DataRow("EndStart", false)]
    [DataRow("EndSomething", false)]
    [DataRow("SomethingStart", false)]
    public void Combinator_StartsWith_Start_Or_EndsWith_End(string name, bool expected)
    {
        var sut = new KnownReference(StartsWith("Start").Or(EndsWith("End")));
        var compilation = new Mock<Compilation>("compilationName", ImmutableArray<MetadataReference>.Empty, new Dictionary<string, string>(), false, null, null);
        var identity = new AssemblyIdentity(name);
        compilation.SetupGet(x => x.ReferencedAssemblyNames).Returns(new[] { identity });
        sut.IsReferenced(compilation.Object).Should().Be(expected);
    }

    [TestMethod]
    public void XUnitAssert_2_4()
    {
        var compilation = SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.CSharp)
            .AddReferences(NuGetMetadataReference.XunitFramework("2.4.2"))
            .AddSnippet("// Empty file")
            .GetCompilation();
        compilation.References(XUnit_Assert).Should().BeTrue();
    }

    [TestMethod]
    public void XUnitAssert_1_9()
    {
        var compilation = SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.CSharp)
            .AddReferences(NuGetMetadataReference.XunitFrameworkV1)
            .AddSnippet("// Empty file")
            .GetCompilation();
        compilation.References(XUnit_Assert).Should().BeTrue();
    }

    [TestMethod]
    public void XUnitAssert_NoReference()
    {
        var compilation = SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.CSharp)
            .AddSnippet("// Empty file")
            .GetCompilation();
        compilation.References(XUnit_Assert).Should().BeFalse();
    }
}
