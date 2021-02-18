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
        public void IsTest_ReturnsTrueForTestFrameworks()
        {
            CreateContext(NuGetMetadataReference.JetBrainsDotMemoryUnit(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.MSTestTestFrameworkV1).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.MSTestTestFramework(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.MachineSpecifications(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.NUnitLite(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.SpecFlow(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.XunitFrameworkV1).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.XunitFramework(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();

            // Assertion
            CreateContext(NuGetMetadataReference.FluentAssertions(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.Shouldly(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();

            // Mock
            CreateContext(NuGetMetadataReference.FakeItEasy(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.JustMock(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.Moq(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.NSubstitute(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
            CreateContext(NuGetMetadataReference.RhinoMocks(Constants.NuGetLatestVersion)).IsTest().Should().BeTrue();
        }

        [TestMethod]
        public void IsTest_ReturnsFalse()
        {
            CreateContext(null).IsTest().Should().BeFalse();
            CreateContext(NuGetMetadataReference.SystemValueTuple(Constants.NuGetLatestVersion)).IsTest().Should().BeFalse();   // Any non-test reference
        }

        [TestMethod]
        public void IsTest_Compilation()
        {
            CreateSemanticModel(NuGetMetadataReference.MSTestTestFrameworkV1).Compilation.IsTest().Should().BeTrue();

            CreateSemanticModel(null).Compilation.IsTest().Should().BeFalse();
        }

        private static SyntaxNodeAnalysisContext CreateContext(IEnumerable<MetadataReference> additionalReferences) =>
            new SyntaxNodeAnalysisContext(null, CreateSemanticModel(additionalReferences), null, null, null, CancellationToken.None);

        private static SemanticModel CreateSemanticModel(IEnumerable<MetadataReference> additionalReferences) =>
            new SnippetCompiler("// Nothing to see here", additionalReferences).SemanticModel;
    }
}
