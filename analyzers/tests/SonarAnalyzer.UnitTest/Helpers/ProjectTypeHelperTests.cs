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
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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
            CreateSemanticModel(NuGetMetadataReference.JetBrainsDotMemoryUnit(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.MSTestTestFrameworkV1).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.MSTestTestFramework(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.MachineSpecifications(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.NUnitLite(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.SpecFlow(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.XunitFrameworkV1).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.XunitFramework(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();

            // Assertion
            CreateSemanticModel(NuGetMetadataReference.FluentAssertions(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.Shouldly(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();

            // Mock
            CreateSemanticModel(NuGetMetadataReference.FakeItEasy(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.JustMock(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.Moq(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.NSubstitute(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
            CreateSemanticModel(NuGetMetadataReference.RhinoMocks(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeTrue();
        }

        [TestMethod]
        public void IsTest_ReturnsFalse()
        {
            CreateSemanticModel(null).Compilation.IsTest().Should().BeFalse();
            CreateSemanticModel(NuGetMetadataReference.SystemValueTuple(Constants.NuGetLatestVersion)).Compilation.IsTest().Should().BeFalse();   // Any non-test reference
        }

        [TestMethod]
        public void IsTest_Compilation()
        {
            CreateSemanticModel(NuGetMetadataReference.MSTestTestFrameworkV1).Compilation.IsTest().Should().BeTrue();

            CreateSemanticModel(null).Compilation.IsTest().Should().BeFalse();
        }

        private static SemanticModel CreateSemanticModel(IEnumerable<MetadataReference> additionalReferences) =>
            new SnippetCompiler("// Nothing to see here", additionalReferences).SemanticModel;
    }
}
