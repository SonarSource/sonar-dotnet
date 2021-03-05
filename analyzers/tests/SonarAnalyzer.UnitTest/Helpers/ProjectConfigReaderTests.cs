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

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ProjectConfigReaderTests
    {
        [TestMethod]
        public void AllPropertiesAreSet()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\Path_Windows\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            // this will fail if a new property is added to the class and no test case for it is added
            foreach (var property in sut.GetType().GetProperties())
            {
                var value = property.GetValue(sut)?.ToString();
                value.Should().NotBeNullOrEmpty($"property '{property.Name}' should have value");
            }
        }

        [TestMethod]
        public void WhenAllValuesAreSet_LoadsExpectedValues()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\Path_Windows\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
            sut.ProjectPath.Should().Be(@"C:\foo\bar\AwesomeBankWeb.CSharp\AwesomeBankWeb.CSharp.csproj");
            sut.FilesToAnalyzePath.Should().Be(@"c:\foo\bar\.sonarqube\conf\0\FilesToAnalyze.txt");
            sut.OutPath.Should().Be(@"C:\foo\bar\.sonarqube\out\0");
            sut.ProjectType.Should().Be(ProjectType.Product);
            sut.TargetFramework.Should().Be("netcoreapp3.1");
        }

        [DataTestMethod]
        [DataRow("Path_MixedSeparators", @"c:\foo\bar\.sonarqube\conf/0/FilesToAnalyze.txt")]
        [DataRow("Path_Unix", @"/home/user/.sonarqube/conf/0/FilesToAnalyze.txt")]
        [DataRow("Path_Windows", @"c:\foo\bar\.sonarqube\conf\0\FilesToAnalyze.txt")]
        public void WithVariousPathFormats_ReturnsAsIsValue(string project, string expectedFilesToAnalyzePath)
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\{project}\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.FilesToAnalyzePath.Should().Be(expectedFilesToAnalyzePath);
        }

        [TestMethod]
        public void WhenHasMissingFilesToAnalyzePath_ReturnsCorrectValue()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\MissingFilesToAnalyzePath\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
            sut.ProjectPath.Should().Be(@"C:\foo\bar\AwesomeBankWeb.CSharp\AwesomeBankWeb.CSharp.csproj");
            sut.OutPath.Should().Be(@"C:\foo\bar\.sonarqube\out\0");
            sut.ProjectType.Should().Be(ProjectType.Product);
            sut.TargetFramework.Should().Be("netcoreapp3.1");
        }

        [TestMethod]
        public void WhenHasUnexpectedProjectType_FallsBackToProduct()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\UnexpectedProjectTypeValue\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.ProjectType.Should().Be(ProjectType.Product);
        }

        [DataTestMethod]
        [DataRow("MissingAnalysisConfigPath")]
        [DataRow("MissingOutPath")]
        [DataRow("MissingProjectPath")]
        [DataRow("MissingProjectType")]
        [DataRow("MissingTargetFramework")]
        public void WhenHasMissingValues_FilesToAnalyzePath_ReturnsCorrectValue(string folder)
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\{folder}\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.FilesToAnalyzePath.Should().Be(@"c:\foo\bar\.sonarqube\conf\0\FilesToAnalyze.txt");
        }

        [DataTestMethod]
        [DataRow("Invalid_DifferentClassName")]
        [DataRow("Invalid_DifferentNamespace")]
        [DataRow("Invalid_Xml")]
        [ExpectedException(typeof(InvalidOperationException), "File SonarProjectConfig.xml could not be parsed.")]
        public void WhenInvalid_FilesToReturnPath_ReturnNull(string folder)
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\{folder}\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.AnalysisConfigPath.Should().BeNull();
            sut.ProjectPath.Should().BeNull();
            sut.FilesToAnalyzePath.Should().BeNull();
            sut.OutPath.Should().BeNull();
            sut.ProjectType.Should().Be(ProjectType.Product);
            sut.TargetFramework.Should().BeNull();
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("/foo/bar/do-not-exit")]
        [DataRow("/foo/bar/x.xml")]
        public void WhenNoFileFound_ReturnsNull(string folder)
        {
            var options = TestHelper.CreateOptions(folder);

            var sut = new ProjectConfigReader(options);

            sut.AnalysisConfigPath.Should().BeNull();
            sut.ProjectPath.Should().BeNull();
            sut.FilesToAnalyzePath.Should().BeNull();
            sut.OutPath.Should().BeNull();
            sut.ProjectType.Should().Be(ProjectType.Product);
            sut.TargetFramework.Should().BeNull();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "File SonarProjectConfig.xml has been added as an AdditionalFile but does not exist.")]
        public void WhenFileIsMissing_ThrowException()
        {
            var options = TestHelper.CreateOptions("ResourceTests\\SonarProjectConfig\\FOO\\SonarProjectConfig.xml");
            var sut = new ProjectConfigReader(options);
            // trigger the lazy reading of the file, should not execute
            sut.AnalysisConfigPath.Should().BeNull();
        }
    }
}
