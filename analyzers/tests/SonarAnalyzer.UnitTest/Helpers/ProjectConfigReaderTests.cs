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
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
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
            var sut = CreateProjectConfigReader($"ResourceTests\\SonarProjectConfig\\Path_Windows\\SonarProjectConfig.xml");

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
            var sut = CreateProjectConfigReader($"ResourceTests\\SonarProjectConfig\\Path_Windows\\SonarProjectConfig.xml");

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
        public void WithVariousPathFormats_ReturnsValueAsIs(string project, string expectedFilesToAnalyzePath)
        {
            var sut = CreateProjectConfigReader($"ResourceTests\\SonarProjectConfig\\{project}\\SonarProjectConfig.xml");

            sut.FilesToAnalyzePath.Should().Be(expectedFilesToAnalyzePath);
        }

        [TestMethod]
        public void WhenHasMissingFilesToAnalyzePath_ReturnsCorrectValue()
        {
            var sut = CreateProjectConfigReader($"ResourceTests\\SonarProjectConfig\\MissingFilesToAnalyzePath\\SonarProjectConfig.xml");

            sut.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
            sut.ProjectPath.Should().Be(@"C:\foo\bar\AwesomeBankWeb.CSharp\AwesomeBankWeb.CSharp.csproj");
            sut.OutPath.Should().Be(@"C:\foo\bar\.sonarqube\out\0");
            sut.ProjectType.Should().Be(ProjectType.Product);
            sut.TargetFramework.Should().Be("netcoreapp3.1");
        }

        [TestMethod]
        public void WhenHasUnexpectedProjectType_FallsBackToProduct()
        {
            var sut = CreateProjectConfigReader($"ResourceTests\\SonarProjectConfig\\UnexpectedProjectTypeValue\\SonarProjectConfig.xml");

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
            var sut = CreateProjectConfigReader($"ResourceTests\\SonarProjectConfig\\{folder}\\SonarProjectConfig.xml");

            sut.FilesToAnalyzePath.Should().Be(@"c:\foo\bar\.sonarqube\conf\0\FilesToAnalyze.txt");
        }

        [DataTestMethod]
        [DataRow("Invalid_DifferentClassName")]
        [DataRow("Invalid_DifferentNamespace")]
        [DataRow("Invalid_Xml")]
        public void WhenInvalid_FilesToReturnPath_ThrowsException(string folder)
        {
            Action a = () => CreateProjectConfigReader($"ResourceTests\\SonarProjectConfig\\{folder}\\SonarProjectConfig.xml");

            a.Should().Throw<InvalidOperationException>().WithMessage("File LogNameFor-SonarProjectConfig.xml could not be parsed.");
        }

        private ProjectConfigReader CreateProjectConfigReader(string relativePath) =>
            new ProjectConfigReader(SourceText.From(File.ReadAllText(relativePath)), "LogNameFor-SonarProjectConfig.xml");
    }
}
