/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using System.Globalization;
using System.IO;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DisablingRequestValidationTest
{
    private const string AspNetMvcVersion = "5.2.7";
    private const string WebConfig = "Web.config";

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void DisablingRequestValidation_CS() =>
        CreateBuilderCS(AnalyzerConfiguration.AlwaysEnabled)
            .AddPaths("DisablingRequestValidation.cs")
            .Verify();

    [TestMethod]
    public void DisablingRequestValidation_CS_Disabled() =>
         CreateBuilderCS(AnalyzerConfiguration.Hotspot)
            .AddPaths("DisablingRequestValidation.cs")
            .VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void DisablingRequestValidation_CS_NoIssuesInTestCode() =>
         CreateBuilderCS(AnalyzerConfiguration.AlwaysEnabled)
            .AddPaths("DisablingRequestValidation.cs")
            .AddTestReference()
            .VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    [DataRow(true, @"TestCases\WebConfig\DisablingRequestValidation\Values")]
    [DataRow(true, @"TestCases\WebConfig\DisablingRequestValidation\Formatting")]
    [DataRow(false, @"TestCases\WebConfig\DisablingRequestValidation\UnexpectedContent")]
    public void DisablingRequestValidation_CS_WebConfig(bool expectIssues, string root)
    {
        var webConfigPath = Path.Combine(root, WebConfig);
        VerifyAdditionalFiles(expectIssues, webConfigPath);
    }

    [TestMethod]
    public void DisablingRequestValidation_CS_CorruptAndNonExistingWebConfigs()
    {
        var root = @"TestCases\WebConfig\DisablingRequestValidation\Corrupt";
        var nonexisting = @"TestCases\WebConfig\DisablingRequestValidation\NonExsitingDirectory";
        var corruptFilePath = Path.Combine(root, WebConfig);
        var nonExistentFilePath = Path.Combine(nonexisting, WebConfig);
        VerifyAdditionalFiles(false, corruptFilePath, nonExistentFilePath);
    }

    [TestMethod]
    [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\MultipleFiles", "SubFolder")]
    [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\EdgeValues", "3.9", "5.6")]
    public void DisablingRequestValidation_CS_WebConfig_SubFolders(string rootDirectory, params string[] subFolders)
    {
        var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation();
        var newCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        // decimal.TryParse() from the implementation might not recognize "1.2" under different culture
        newCulture.NumberFormat.NumberDecimalSeparator = ",";
        using var scope = new CurrentCultureScope(newCulture);
        var rootFile = Path.Combine(rootDirectory, WebConfig);
        var filesToAnalyze = new List<string> { rootFile };
        foreach (var subFolder in subFolders)
        {
            filesToAnalyze.Add(Path.Combine(rootDirectory, subFolder, WebConfig));
        }
        var analyzer = new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled);
        filesToAnalyze.Add(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, filesToAnalyze.ToArray()));
        CreateBuilderCS(AnalyzerConfiguration.AlwaysEnabled)
            .AddSnippet("// Nothing to see here")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, filesToAnalyze.ToArray()))
            .AddAdditionalSourceFiles(filesToAnalyze.ToArray())
            .Verify();
    }

    [TestMethod]
    public void DisablingRequestValidation_CS_WebConfig_LowerCase()
    {
        var root = @"TestCases\WebConfig\DisablingRequestValidation\LowerCase";
        var webConfigPath = Path.Combine(root, "web.config");
        VerifyAdditionalFiles(true, webConfigPath);
    }

    [TestMethod]
    [DataRow(true, @"TestCases\WebConfig\DisablingRequestValidation\TransformCustom\Web.Custom.config")]
    [DataRow(false, @"TestCases\WebConfig\DisablingRequestValidation\TransformDebug\Web.Debug.config")]
    [DataRow(true, @"TestCases\WebConfig\DisablingRequestValidation\TransformRelease\Web.Release.config")]
    public void DisablingRequestValidation_CS_WebConfig_Transformation(bool expectIssues, string configPath) =>
        VerifyAdditionalFiles(expectIssues, configPath);

    [TestMethod]
    public void DisablingRequestValidation_VB() =>
        CreateBuilderVB(AnalyzerConfiguration.AlwaysEnabled)
            .AddPaths("DisablingRequestValidation.vb")
            .WithOptions(LanguageOptions.FromVisualBasic14)
            .Verify();

    [TestMethod]
    public void DisablingRequestValidation_VB_Disabled() =>
        CreateBuilderVB(AnalyzerConfiguration.Hotspot)
            .AddPaths("DisablingRequestValidation.vb")
            .VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void DisablingRequestValidation_VB_WebConfig()
    {
        var root = @"TestCases\WebConfig\DisablingRequestValidation\Values";
        var webConfigPath = Path.Combine(root, WebConfig);
        CreateBuilderCS(AnalyzerConfiguration.AlwaysEnabled)
            .AddSnippet("// Nothing to see here")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .AddAdditionalSourceFiles(webConfigPath)
            .Verify();
    }

    private static VerifierBuilder CreateBuilderCS(IAnalyzerConfiguration configuration) =>
        new VerifierBuilder()
            .WithBasePath("Hotspots")
            .AddAnalyzer(() => new CS.DisablingRequestValidation(configuration))
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

    private static VerifierBuilder CreateBuilderVB(IAnalyzerConfiguration configuration) =>
        new VerifierBuilder()
            .WithBasePath("Hotspots")
            .AddAnalyzer(() => new VB.DisablingRequestValidation(configuration))
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

    private void VerifyAdditionalFiles(bool expectIssues, string additionalSourceFile, params string[] additionalFilesToAnalyze)
    {
        var withAdditionalSourceFiles = CreateBuilderCS(AnalyzerConfiguration.AlwaysEnabled)
            .AddSnippet("// Nothing to see here")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, additionalFilesToAnalyze.Append(additionalSourceFile).ToArray()))
            .AddAdditionalSourceFiles(additionalSourceFile);
        if (expectIssues)
        {
            withAdditionalSourceFiles.Verify();
        }
        else
        {
            withAdditionalSourceFiles.VerifyNoIssues();
        }
    }
}
