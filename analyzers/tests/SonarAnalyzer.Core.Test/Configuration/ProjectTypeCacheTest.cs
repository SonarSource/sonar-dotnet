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

using SonarAnalyzer.Core.Configuration;

namespace SonarAnalyzer.Core.Test.Configuration;

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
            "Machine.Specifications",
            "nunit.framework",
            "nunitlite",
            "TechTalk.SpecFlow",
            "xunit",
            "xunit.core",
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
        IsTest(NuGetMetadataReference.JetBrainsDotMemoryUnit(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.MSTestTestFrameworkV1).Should().BeTrue();
        IsTest(NuGetMetadataReference.MSTestTestFramework(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework).Should().BeTrue();
        IsTest(NuGetMetadataReference.MachineSpecifications(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.NUnitLite(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.SpecFlow(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.XunitFrameworkV1).Should().BeTrue();
        IsTest(NuGetMetadataReference.XunitFramework(Constants.NuGetLatestVersion)).Should().BeTrue();

        // Assertion
        IsTest(NuGetMetadataReference.FluentAssertions(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.Shouldly(Constants.NuGetLatestVersion)).Should().BeTrue();

        // Mock
        IsTest(NuGetMetadataReference.FakeItEasy(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.JustMock(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.Moq(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.NSubstitute(Constants.NuGetLatestVersion)).Should().BeTrue();
        IsTest(NuGetMetadataReference.RhinoMocks(Constants.NuGetLatestVersion)).Should().BeTrue();
    }

    [TestMethod]
    public void IsTest_ReturnsFalse()
    {
        IsTest(null).Should().BeFalse();
        // Any non-test reference
        IsTest(NuGetMetadataReference.SystemValueTuple(Constants.NuGetLatestVersion)).Should().BeFalse();
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
        new SnippetCompiler("// Nothing to see here", additionalReferences).SemanticModel;
}
