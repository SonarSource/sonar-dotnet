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
public class CookieShouldBeSecureTest
{

#if NETFRAMEWORK

    private const string WebConfig = "Web.config";

#endif

    private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled));

    public TestContext TestContext { get; set; }

    internal static IEnumerable<MetadataReference> AdditionalReferences =>
        NuGetMetadataReference.Nancy();

    [TestMethod]
    public void CookieShouldBeSecure_Nancy() =>
        builder.AddPaths("CookieShouldBeSecure_Nancy.cs")
            .AddReferences(AdditionalReferences)
            .Verify();

#if NETFRAMEWORK // HttpCookie is not available on .Net Core

    [TestMethod]
    public void CookieShouldBeSecure() =>
         builder.AddPaths("CookieShouldBeSecure.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .Verify();

    [DataTestMethod]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\SecureCookieConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\Formatting")]
    public void CookieShouldBeSecure_WithWebConfigValueSetToTrue(string root)
    {
        var webConfigPath = Path.Combine(root, WebConfig);

        builder.AddPaths("CookieShouldBeSecure_WithWebConfig.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .Verify();
    }

    [DataTestMethod]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\NonSecureCookieConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\UnrelatedConfig")]
    [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\ConfigWithoutAttribute")]
    public void CookieShouldBeSecure_WithWebConfigValueSetToFalse(string root)
    {
        var webConfigPath = Path.Combine(root, WebConfig);
        builder.AddPaths("CookieShouldBeSecure.cs")
            .AddReferences(MetadataReferenceFacade.SystemWeb)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .Verify();
    }

#else

    [TestMethod]
    public void CookieShouldBeSecure_Latest() =>
         builder.AddPaths("CookieShouldBeSecure.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithTopLevelStatements()
            .AddReferences(GetAdditionalReferences_NetCore())
            .AddReferences(NuGetMetadataReference.Nancy())
            .Verify();

    private static IEnumerable<MetadataReference> GetAdditionalReferences_NetCore() =>
        NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(TestConstants.NuGetLatestVersion);

#endif

}
