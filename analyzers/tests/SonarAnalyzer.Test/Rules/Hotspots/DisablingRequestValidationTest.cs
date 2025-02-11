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
        new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled))
            .AddPaths("DisablingRequestValidation.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion))
            .Verify();

    [TestMethod]
    public void DisablingRequestValidation_CS_Disabled() =>
         new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CS.DisablingRequestValidation(AnalyzerConfiguration.Hotspot))
            .AddPaths("DisablingRequestValidation.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion))
            .VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void DisablingRequestValidation_CS_NoIssuesInTestCode() =>
         new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled))
            .AddPaths("DisablingRequestValidation.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion))
            .AddTestReference()
            .VerifyNoIssuesIgnoreErrors();

    [DataTestMethod]
    [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\Values")]
    [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\Formatting")]
    [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\UnexpectedContent")]
    public void DisablingRequestValidation_CS_WebConfig(string root)
    {
        var webConfigPath = Path.Combine(root, WebConfig);
        DiagnosticVerifier.Verify(
            SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
            new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
            AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath),
            null,
            [webConfigPath]);
    }

    [TestMethod]
    public void DisablingRequestValidation_CS_CorruptAndNonExistingWebConfigs()
    {
        var root = @"TestCases\WebConfig\DisablingRequestValidation\Corrupt";
        var nonexisting = @"TestCases\WebConfig\DisablingRequestValidation\NonExsitingDirectory";
        var corruptFilePath = Path.Combine(root, WebConfig);
        var nonExistingFilePath = Path.Combine(nonexisting, WebConfig);
        DiagnosticVerifier.Verify(
            SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
            new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
            AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, corruptFilePath, nonExistingFilePath),
            null,
            [corruptFilePath]);
    }

    [DataTestMethod]
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
        var additionalFilePath = AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, filesToAnalyze.ToArray());
        DiagnosticVerifier.Verify(compilation, analyzer, additionalFilePath, null, filesToAnalyze.ToArray());
    }

    [TestMethod]
    public void DisablingRequestValidation_CS_WebConfig_LowerCase()
    {
        var root = @"TestCases\WebConfig\DisablingRequestValidation\LowerCase";
        var webConfigPath = Path.Combine(root, "web.config");
        DiagnosticVerifier.Verify(
            SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
            new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
            AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath),
            null,
            [webConfigPath]);
    }

    [DataTestMethod]
    [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\TransformCustom\Web.Custom.config")]
    [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\TransformDebug\Web.Debug.config")]
    [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\TransformRelease\Web.Release.config")]
    public void DisablingRequestValidation_CS_WebConfig_Transformation(string configPath) =>
        DiagnosticVerifier.Verify(
            SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
            new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
            AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, configPath),
            null,
            [configPath]);

    [TestMethod]
    public void DisablingRequestValidation_VB() =>
        new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new VB.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled))
            .AddPaths("DisablingRequestValidation.vb")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion))
            .WithOptions(LanguageOptions.FromVisualBasic14)
            .Verify();

    [TestMethod]
    public void DisablingRequestValidation_VB_Disabled() =>
        new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new VB.DisablingRequestValidation(AnalyzerConfiguration.Hotspot))
            .AddPaths("DisablingRequestValidation.vb")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion))
            .VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void DisablingRequestValidation_VB_WebConfig()
    {
        var root = @"TestCases\WebConfig\DisablingRequestValidation\Values";
        var webConfigPath = Path.Combine(root, WebConfig);
        DiagnosticVerifier.Verify(
            SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).GetCompilation(),
            new VB.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
            AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath),
            null,
            [webConfigPath]);
    }
}
