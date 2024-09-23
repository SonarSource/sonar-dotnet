/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.IO;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class ProjectConfigReaderTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void AllPropertiesAreSet()
    {
        var sut = CreateProjectConfigReader(@"TestResources\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");

        // this will fail if a new property is added to the class and no test case for it is added
        foreach (var property in sut.GetType().GetProperties().Where(x => x.Name != "AnalysisConfig"))
        {
            var value = property.GetValue(sut)?.ToString();
            value.Should().NotBeNullOrEmpty($"property '{property.Name}' should have value");
        }
    }

    [TestMethod]
    public void WhenAllValuesAreSet_LoadsExpectedValues()
    {
        var sut = CreateProjectConfigReader(@"TestResources\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");

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
    public void WithVariousPathFormats_ReturnsValueAsIs(string project, string expectedFilesToAnalyzePath) =>
        CreateProjectConfigReader($@"TestResources\SonarProjectConfig\{project}\SonarProjectConfig.xml").FilesToAnalyzePath.Should().Be(expectedFilesToAnalyzePath);

    [TestMethod]
    public void WhenHasMissingFilesToAnalyzePath_ReturnsCorrectValue()
    {
        var sut = CreateProjectConfigReader(@"TestResources\SonarProjectConfig\MissingFilesToAnalyzePath\SonarProjectConfig.xml");

        sut.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
        sut.ProjectPath.Should().Be(@"C:\foo\bar\AwesomeBankWeb.CSharp\AwesomeBankWeb.CSharp.csproj");
        sut.OutPath.Should().Be(@"C:\foo\bar\.sonarqube\out\0");
        sut.ProjectType.Should().Be(ProjectType.Product);
        sut.TargetFramework.Should().Be("netcoreapp3.1");
    }

    [TestMethod]
    public void WhenHasUnexpectedProjectType_FallsBackToProduct() =>
        CreateProjectConfigReader(@"TestResources\SonarProjectConfig\UnexpectedProjectTypeValue\SonarProjectConfig.xml").ProjectType.Should().Be(ProjectType.Product);

    [DataTestMethod]
    [DataRow("MissingAnalysisConfigPath")]
    [DataRow("MissingOutPath")]
    [DataRow("MissingProjectPath")]
    [DataRow("MissingProjectType")]
    [DataRow("MissingTargetFramework")]
    public void WhenHasMissingValues_FilesToAnalyzePath_ReturnsCorrectValue(string folder) =>
        CreateProjectConfigReader($@"TestResources\SonarProjectConfig\{folder}\SonarProjectConfig.xml").FilesToAnalyzePath.Should().Be(@"c:\foo\bar\.sonarqube\conf\0\FilesToAnalyze.txt");

    [DataTestMethod]
    [DataRow("Invalid_DifferentClassName")]
    [DataRow("Invalid_DifferentNamespace")]
    [DataRow("Invalid_Xml")]
    public void WhenInvalid_FilesToReturnPath_ThrowsException(string folder) =>
        Assert.ThrowsException<InvalidOperationException>(() => CreateProjectConfigReader($@"TestResources\SonarProjectConfig\{folder}\SonarProjectConfig.xml"))
            .Message.Should().Be("sonarProjectConfig could not be parsed.");

    [TestMethod]
    public void FilesToAnalyze_LoadsFileFromConfig()
    {
        var config = SourceText.From(@"
<SonarProjectConfig xmlns=""http://www.sonarsource.com/msbuild/analyzer/2021/1"">
    <FilesToAnalyzePath>TestResources\FilesToAnalyze\FilesToAnalyze.txt</FilesToAnalyzePath>
</SonarProjectConfig>");
        var files = new ProjectConfigReader(config).FilesToAnalyze.FindFiles("web.config", false);

        files.Should().BeEquivalentTo(new[] { @"C:\Projects/DummyProj/wEB.config", @"C:\Projects/DummyProj/Views\Web.confiG" });
    }

    [TestMethod]
    public void AnalysisConfig_LoadsConfigFromDisk()
    {
        var path = AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext);     // With AnalysisConfigPath that exists
        var sut = CreateProjectConfigReader(path);
        sut.AnalysisConfig.Should().NotBeNull();
    }

    private static ProjectConfigReader CreateProjectConfigReader(string relativePath) =>
        new(SourceText.From(File.ReadAllText(relativePath)));
}
