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

namespace SonarAnalyzer.Core.Configuration.Test;

[TestClass]
public class ProjectTypeCacheTest
{
    [TestMethod]
    public void TestReference_ShouldBeSynchronized()
    {
        // Purpose of this test is to remind us, that we need to synchronize this list with sonar-scanner-msbuild and sonar-security.
        var synchronizedSortedReferences = new[]
        {
            "dotMemory.Unit",
            "Microsoft.VisualStudio.TestPlatform.TestFramework",
            "Microsoft.VisualStudio.QualityTools.UnitTestFramework",
            "MSTest.TestFramework",
            "Machine.Specifications",
            "nunit.framework",
            "nunitlite",
            "TechTalk.SpecFlow",
            "xunit",
            "xunit.core",
            "xunit.v3.core",
            "FluentAssertions",
            "Shouldly",
            "FakeItEasy",
            "Moq",
            "NSubstitute",
            "Rhino.Mocks",
            "Telerik.JustMock"
        };
        ProjectTypeCache.TestAssemblyNames.OrderBy(x => x).Should().BeEquivalentTo(synchronizedSortedReferences);
    }

    [TestMethod]
    public void IsTest_ReturnsTrueForTestFrameworks()
    {
        IsTest(NuGetMetadataReference.JetBrainsDotMemoryUnit(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.MSTestTestFrameworkV1).Should().BeTrue();
        IsTest(NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework).Should().BeTrue();
        IsTest(NuGetMetadataReference.MachineSpecifications(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.NUnit(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.NUnitLite(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.SpecFlow(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.XunitFrameworkV1).Should().BeTrue();
        IsTest(NuGetMetadataReference.XunitFramework(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.XunitFrameworkV3(TestConstants.NuGetLatestVersion)).Should().BeTrue();

        // Assertion
        IsTest(NuGetMetadataReference.FluentAssertions(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.Shouldly(TestConstants.NuGetLatestVersion)).Should().BeTrue();

        // Mock
        IsTest(NuGetMetadataReference.FakeItEasy(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.JustMock(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.Moq(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.NSubstitute(TestConstants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.RhinoMocks(TestConstants.NuGetLatestVersion)).Should().BeTrue();
    }

    [TestMethod]
    public void IsTest_ReturnsFalse()
    {
        IsTest(null).Should().BeFalse();
        // Any non-test reference
        IsTest(NuGetMetadataReference.SystemValueTuple(TestConstants.NuGetLatestVersion)).Should().BeFalse();
    }

    [TestMethod]
    public void IsTest_Compilation()
    {
        IsTest(NuGetMetadataReference.MSTestTestFrameworkV1).Should().BeTrue();

        IsTest(null).Should().BeFalse();
    }

    private static bool IsTest(IEnumerable<MetadataReference> additionalReferences) =>
        CreateSemanticModel(additionalReferences).Compilation.IsTest();

    private static SemanticModel CreateSemanticModel(IEnumerable<MetadataReference> additionalReferences) =>
        new SnippetCompiler("// Nothing to see here", additionalReferences).Model;
}
