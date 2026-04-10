/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public class AnalysisConfigReaderTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void MissingFile_ReturnsEmpty()
    {
        var sut = new AnalysisConfigReader("ThisFileDoesNotExist.xml");
        sut.UnchangedFiles().Should().BeEmpty();
        sut.IsCloud.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(@"<?xml that is invalid")]
    [DataRow(@"<ValidXml><UnexpectedContent /></ValidXml>")]
    [DataRow(@"<WrongRootCorrectNamespace xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1"" />")]
    [DataRow(@"<AnalysisConfig xmlns=""http://wrong.namespace"" />")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig><ConfigSetting /></AdditionalConfig></AnalysisConfig>")] // No Id attribute
    public void UnexpectedXml_Throws(string xml)
    {
        var path = TestFiles.WriteFile(TestContext, "SonarQubeAnalysisConfig.xml", xml);
        ((Func<AnalysisConfigReader>)(() => new AnalysisConfigReader(path))).Should().Throw<InvalidOperationException>().WithMessage($"File '{path}' could not be parsed.");
    }

    [TestMethod]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig><ConfigSetting Id=""wrong"" Value=""42"" /></AdditionalConfig></AnalysisConfig>")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig><ConfigSetting Id=""UnchangedFilesPath"" /></AdditionalConfig></AnalysisConfig>")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig><X Id=""UnchangedFilesPath"" Value=""42"" /></AdditionalConfig></AnalysisConfig>")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig></AdditionalConfig></AnalysisConfig>")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""></AnalysisConfig>")]
    public void MissingContent(string xml)
    {
        var path = TestFiles.WriteFile(TestContext, "SonarQubeAnalysisConfig.xml", xml);
        var sut = new AnalysisConfigReader(path);
        sut.UnchangedFiles().Should().BeEmpty();
        sut.IsCloud.Should().BeFalse();
    }

    [TestMethod]
    public void UnchangedFiles_NotPresent_ReturnsEmpty()
    {
        var path = AnalysisScaffolding.CreateAnalysisConfig(TestContext, "SomeOtherProperty", "This is not UnchangedFilesPath");
        var sut = new AnalysisConfigReader(path);
        sut.UnchangedFiles().Should().BeEmpty();
    }

    [TestMethod]
    public void UnchangedFiles_Empty_ReturnsEmpty()
    {
        var path = AnalysisScaffolding.CreateAnalysisConfig(TestContext, []);
        var sut = new AnalysisConfigReader(path);
        sut.UnchangedFiles().Should().BeEmpty();
    }

    [TestMethod]
    public void UnchangedFiles_Present_ReturnsContent()
    {
        var path = AnalysisScaffolding.CreateAnalysisConfig(TestContext, [@"C:\File1.cs", @"C:\File2.cs", "Any other string"]);
        var sut = new AnalysisConfigReader(path);
        sut.UnchangedFiles().Should().BeEquivalentTo(@"C:\File1.cs", @"C:\File2.cs", "Any other string");
    }

    [TestMethod]
    [DataRow("8.0.0.82356", true)]  // Actual SQ-C version
    [DataRow("8.0.0.29455", true)]  // While this is SQ-S 8.0 and not cloud, we expect true, because this analyzer will never be backported there
    [DataRow("8.0.0.99999", true)]
    [DataRow("2026.1.0.1234", false)]
    [DataRow("whatever", false)]
    [DataRow("", false)]
    public void IsSonarCloud(string version, bool expected)
    {
        var path = AnalysisScaffolding.CreateAnalysisConfig(TestContext, "key", "value", version);
        var sut = new AnalysisConfigReader(path);
        sut.IsCloud.Should().Be(expected);
    }
}
