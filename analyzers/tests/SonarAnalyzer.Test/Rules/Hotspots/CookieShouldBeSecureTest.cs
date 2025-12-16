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
public class CookieShouldBeSecureTest
{
    private const string WebConfig = "Web.config";

    private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled));

    public TestContext TestContext { get; set; }

    internal static IEnumerable<MetadataReference> AdditionalReferences => NuGetMetadataReference.Nancy();

    [TestMethod]
    public void CookieShouldBeSecure_Nancy() =>
        builder.AddPaths("CookieShouldBeSecure_Nancy.cs").AddReferences(AdditionalReferences).Verify();

    [TestMethod]
    public void CookieShouldBeSecure_Latest() =>
     builder.AddPaths("CookieShouldBeSecure.Latest.cs")
        .WithOptions(LanguageOptions.CSharpLatest)
        .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(TestConstants.NuGetLatestVersion))
        .AddReferences(AdditionalReferences)
        .Verify();

    [TestMethod]
    public void CookieShouldBeSecure_TopLevelStatements() =>
     builder.AddPaths("CookieShouldBeSecure.TopLevelStatements.cs")
        .WithOptions(LanguageOptions.CSharpLatest)
        .WithTopLevelStatements()
        .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(TestConstants.NuGetLatestVersion))
        .AddReferences(AdditionalReferences)
        .Verify();

    [TestMethod]
    public void CookieShouldBeSecure_NetFx() =>
         builder.AddPaths("CookieShouldBeSecure.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithNetFrameworkOnly()
            .Verify();

    [TestMethod]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\SecureCookieConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\Formatting")]
    public void CookieShouldBeSecure_WithWebConfigValueSetToTrue(string root)
    {
        var webConfigPath = Path.Combine(root, WebConfig);
        builder.AddPaths("CookieShouldBeSecure_WithWebConfig.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .WithNetFrameworkOnly()
            .Verify();
    }

    [TestMethod]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\NonSecureCookieConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\UnrelatedConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\ConfigWithoutAttribute")]
    public void CookieShouldBeSecure_WithWebConfigValueSetToFalse(string root)
    {
        var webConfigPath = Path.Combine(root, WebConfig);
        builder.AddPaths("CookieShouldBeSecure.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .WithNetFrameworkOnly()
            .Verify();
    }
}
