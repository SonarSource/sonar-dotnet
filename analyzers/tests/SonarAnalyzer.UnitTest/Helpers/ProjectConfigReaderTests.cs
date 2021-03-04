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

using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]

    public class ProjectConfigReaderTests
    {
        [DataTestMethod]
        [DataRow("Valid_Product_WindowsPaths", @"c:\foo\bar\.sonarqube\conf\0\FilesToAnalyze.txt")]
        [DataRow("Valid_Test_UnixPaths", @"/home/user/.sonarqube/conf/0/FilesToAnalyze.txt")]
        public void WhenProjectConfigIsValid_FilesToAnalyzePath_ReturnsCorrectValue(string folder, string expectedFileToAnalyzePath)
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\{folder}\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.FilesToAnalyzePath.Should().Be(expectedFileToAnalyzePath);
        }

        [DataTestMethod]
        [DataRow("Invalid_MissingAnalysisConfigPath")]
        [DataRow("Invalid_MissingOutPath")]
        [DataRow("Invalid_MissingProjectPath")]
        [DataRow("Invalid_MissingProjectType")]
        [DataRow("Invalid_MissingTargetFramework")]

        public void WhenProjectConfigHasMissingValues_FilesToAnalyzePath_ReturnsCorrectValue(string folder)
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\{folder}\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.FilesToAnalyzePath.Should().Be(@"c:\foo\bar\.sonarqube\conf\0\FilesToAnalyze.txt");
        }

        [TestMethod]
        public void WhenProjectConfigHasMixedSeparators_FilesToAnalyzePath_ReturnsValues()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\Invalid_MixedSeparators\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.FilesToAnalyzePath.Should().Be(@"c:\foo\bar\.sonarqube\conf/0/FilesToAnalyze.txt");
        }

        [DataTestMethod]
        [DataRow("Invalid_DifferentClassName")]
        [DataRow("Invalid_DifferentNamespace")]
        [DataRow("Invalid_WrongProjectTypeValue")]
        [DataRow("Invalid_Xml")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void WhenProjectConfigIsValid_FilesToReturnPath_ReturnNull(string folder)
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\{folder}\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.FilesToAnalyzePath.Should().BeNull();
        }
    }
}
