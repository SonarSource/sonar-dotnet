/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CookieShouldBeHttpOnlyTest
{
    private const string WebConfig = "Web.config";

    private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CookieShouldBeHttpOnly(AnalyzerConfiguration.AlwaysEnabled));

    public TestContext TestContext { get; set; }

    internal static IEnumerable<MetadataReference> AdditionalReferences => NuGetMetadataReference.Nancy();

    [TestMethod]
    public void CookieShouldBeHttpOnly_Nancy() =>
        builder.AddPaths("CookieShouldBeHttpOnly_Nancy.cs").AddReferences(AdditionalReferences).Verify();

    [TestMethod]
    public void CookieShouldBeHttpOnly_Latest() =>
        builder.AddPaths("CookieShouldBeHttpOnly.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(TestConstants.NuGetLatestVersion))
            .AddReferences(AdditionalReferences)
            .Verify();

    [TestMethod]
    public void CookieShouldBeHttpOnly_TopLevelStatements() =>
    builder.AddPaths("CookieShouldBeHttpOnly.TopLevelStatements.cs")
        .WithTopLevelStatements()
        .WithOptions(LanguageOptions.CSharpLatest)
        .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(TestConstants.NuGetLatestVersion))
        .AddReferences(AdditionalReferences)
        .Verify();

    [TestMethod]
    public void CookieShouldBeHttpOnly() =>
        builder.AddPaths("CookieShouldBeHttpOnly.cs").AddReferences(MetadataReferenceFacade.SystemWeb).WithNetFrameworkOnly().Verify();

    [TestMethod]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\HttpOnlyCookiesConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\Formatting")]
    public void CookieShouldBeHttpOnly_WithWebConfigValueSetToTrue(string root) =>
        builder.AddPaths("CookieShouldBeHttpOnly_WithWebConfig.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, Path.Combine(root, WebConfig)))
            .WithNetFrameworkOnly()
            .Verify();

    [TestMethod]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\NonHttpOnlyCookiesConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\UnrelatedConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\ConfigWithoutAttribute")]
    public void CookieShouldBeHttpOnly_WithWebConfigValueSetToFalse(string root) =>
        builder.AddPaths("CookieShouldBeHttpOnly.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, Path.Combine(root, WebConfig)))
            .WithNetFrameworkOnly()
            .Verify();
}
