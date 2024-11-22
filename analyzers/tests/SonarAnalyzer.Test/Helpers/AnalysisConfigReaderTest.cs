/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.IO;

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class AnalysisConfigReaderTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void MissingFile_Throws()
    {
        var path = Path.GetFullPath("ThisFileDoesNotExist.xml");
        ((Func<AnalysisConfigReader>)(() => new AnalysisConfigReader(path))).Should().Throw<InvalidOperationException>().WithMessage($"File '{path}' could not be parsed.");
    }

    [TestMethod]
    [DataRow(@"<?xml that is invalid")]
    [DataRow(@"<ValidXml><UnexpectedContent /></ValidXml>")]
    [DataRow(@"<WrongRootCorrectNamespace xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1"" />")]
    [DataRow(@"<AnalysisConfig xmlns=""http://wrong.namespace"" />")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig><ConfigSetting /></AdditionalConfig></AnalysisConfig>")] // No Id attribute
    public void UnexpectedXml_Throws(string xml)
    {
        var path = TestHelper.WriteFile(TestContext, "SonarQubeAnalysisConfig.xml", xml);
        ((Func<AnalysisConfigReader>)(() => new AnalysisConfigReader(path))).Should().Throw<InvalidOperationException>().WithMessage($"File '{path}' could not be parsed.");
    }

    [TestMethod]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig><ConfigSetting Id=""wrong"" Value=""42"" /></AdditionalConfig></AnalysisConfig>")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig><ConfigSetting Id=""UnchangedFilesPath"" /></AdditionalConfig></AnalysisConfig>")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig><X Id=""UnchangedFilesPath"" Value=""42"" /></AdditionalConfig></AnalysisConfig>")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""><AdditionalConfig></AdditionalConfig></AnalysisConfig>")]
    [DataRow(@"<AnalysisConfig xmlns=""http://www.sonarsource.com/msbuild/integration/2015/1""></AnalysisConfig>")]
    public void MissingContent_ReturnsEmpty(string xml)
    {
        var path = TestHelper.WriteFile(TestContext, "SonarQubeAnalysisConfig.xml", xml);
        var sut = new AnalysisConfigReader(path);
        sut.UnchangedFiles().Should().BeEmpty();
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
}
