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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ProjectTypeHelperTests
    {
        [TestMethod]
        public void TestReference_ShouldBeSynchronized()
        {
            // Purpose of this test is to remind us, that we need to synchronize this list with sonar-scanner-msbuild and sonar-security.
            var synchronizedSortedReferences = new[]
            {
                "DOTMEMORY.UNIT",
                "FAKEITEASY",
                "FLUENTASSERTIONS",
                "MICROSOFT.VISUALSTUDIO.TESTPLATFORM.TESTFRAMEWORK",
                "MICROSOFT.VISUALSTUDIO.QUALITYTOOLS.UNITTESTFRAMEWORK",
                "MACHINE.SPECIFICATIONS",
                "MOQ",
                "NSUBSTITUTE",
                "NUNIT.FRAMEWORK",
                "NUNITLITE",
                "RHINO.MOCKS",
                "SHOULDLY",
                "TECHTALK.SPECFLOW",
                "TELERIK.JUSTMOCK",
                "XUNIT",
                "XUNIT.CORE"
            };
            ProjectTypeHelper.TestAssemblyNames.OrderBy(x => x).Should().BeEquivalentTo(synchronizedSortedReferences);
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
}
