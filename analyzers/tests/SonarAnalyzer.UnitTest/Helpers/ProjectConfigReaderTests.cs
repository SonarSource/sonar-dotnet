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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]

    public class ProjectConfigReaderTests
    {
        [TestMethod]
        public void WhenProjectConfigHasWindowsPathsAndProductType_ReturnsCorrectValue()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\Valid_Product_WindowsPaths\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
            sut.ProjectPath.Should().Be(@"C:\foo\bar\AwesomeBankWeb.CSharp\AwesomeBankWeb.CSharp.csproj");
            sut.FilesToAnalyzePath.Should().Be(@"c:\foo\bar\.sonarqube\conf\0\FilesToAnalyze.txt");
            sut.OutPath.Should().Be(@"C:\foo\bar\.sonarqube\out\0");
            sut.ProjectType.Should().Be(ProjectType.Product);
            sut.TargetFramework.Should().Be("netcoreapp3.1");
        }

        [TestMethod]
        public void WhenProjectConfigHasUnixPathAndTestType_ReturnsCorrectValue()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\Valid_Test_UnixPaths\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.AnalysisConfigPath.Should().Be(@"/home/user/.sonarqube/conf/SonarQubeAnalysisConfig.xml");
            sut.ProjectPath.Should().Be(@"/home/user/AwesomeBankWeb.CSharp.csproj");
            sut.FilesToAnalyzePath.Should().Be(@"/home/user/.sonarqube/conf/0/FilesToAnalyze.txt");
            sut.OutPath.Should().Be(@"/home/user/.sonarqube/out/0");
            sut.ProjectType.Should().Be(ProjectType.Test);
            sut.TargetFramework.Should().Be("net5");
        }

        [TestMethod]
        public void WhenProjectConfigHasMixedSeparators_FilesToAnalyzePath_ReturnsValues()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\Invalid_MixedSeparators\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube/conf/SonarQubeAnalysisConfig.xml");
            sut.ProjectPath.Should().Be(@"C:\foo\bar\AwesomeBankWeb.CSharp/AwesomeBankWeb.CSharp.csproj");
            sut.FilesToAnalyzePath.Should().Be(@"c:\foo\bar\.sonarqube\conf/0/FilesToAnalyze.txt");
            sut.OutPath.Should().Be(@"C:\foo\bar\.sonarqube/out/0");
            sut.ProjectType.Should().Be(ProjectType.Product);
            sut.TargetFramework.Should().Be("netcoreapp3.1");
        }

        [TestMethod]
        public void WhenProjectConfigHasMissingFilesToAnalyzePath_ReturnsCorrectValue()
        {
            var options = TestHelper.CreateOptions($"ResourceTests\\SonarProjectConfig\\Invalid_MissingFilesToAnalyzePath\\SonarProjectConfig.xml");

            var sut = new ProjectConfigReader(options);

            sut.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
            sut.ProjectPath.Should().Be(@"C:\foo\bar\AwesomeBankWeb.CSharp\AwesomeBankWeb.CSharp.csproj");
            sut.OutPath.Should().Be(@"C:\foo\bar\.sonarqube\out\0");
            sut.ProjectType.Should().Be(ProjectType.Product);
            sut.TargetFramework.Should().Be("netcoreapp3.1");
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

            sut.AnalysisConfigPath.Should().BeNull();
            sut.ProjectPath.Should().BeNull();
            sut.FilesToAnalyzePath.Should().BeNull();
            sut.OutPath.Should().BeNull();
            sut.ProjectType.Should().BeNull();
            sut.TargetFramework.Should().BeNull();
        }
    }
}
