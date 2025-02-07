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

#if NETFRAMEWORK
using System.IO;
#endif

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CookieShouldBeHttpOnlyTest
{
#if NETFRAMEWORK

    private const string WebConfig = "Web.config";

#endif

    private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CookieShouldBeHttpOnly(AnalyzerConfiguration.AlwaysEnabled));

    public TestContext TestContext { get; set; }

    internal static IEnumerable<MetadataReference> AdditionalReferences =>
        NuGetMetadataReference.Nancy();

    [TestMethod]
    public void CookiesShouldBeHttpOnly_Nancy() =>
        builder.AddPaths("CookieShouldBeHttpOnly_Nancy.cs")
            .AddReferences(AdditionalReferences)
            .Verify();

#if NETFRAMEWORK // The analyzed code is valid only for .Net Framework

    [TestMethod]
    public void CookiesShouldBeHttpOnly() =>
        builder.AddPaths("CookieShouldBeHttpOnly.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .Verify();

    [DataTestMethod]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\HttpOnlyCookiesConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\Formatting")]
    public void CookiesShouldBeHttpOnly_WithWebConfigValueSetToTrue(string root)
    {
        var webConfigPath = Path.Combine(root, WebConfig);
        builder.AddPaths("CookieShouldBeHttpOnly_WithWebConfig.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .Verify();
    }

    [DataTestMethod]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\NonHttpOnlyCookiesConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\UnrelatedConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeHttpOnly\ConfigWithoutAttribute")]
    public void CookiesShouldBeHttpOnly_WithWebConfigValueSetToFalse(string root)
    {
        var webConfigPath = Path.Combine(root, WebConfig);
        builder.AddPaths("CookieShouldBeHttpOnly.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .Verify();
    }

#else

    [TestMethod]
    public void CookiesShouldBeHttpOnly_Latest() =>
        builder.AddPaths("CookieShouldBeHttpOnly.Latest.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(GetAdditionalReferences_NetCore())
            .AddReferences(NuGetMetadataReference.Nancy())
            .Verify();

    private static IEnumerable<MetadataReference> GetAdditionalReferences_NetCore() =>
        NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(TestConstants.NuGetLatestVersion);

#endif

}
