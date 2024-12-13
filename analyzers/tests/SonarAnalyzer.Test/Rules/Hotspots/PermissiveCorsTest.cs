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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class PermissiveCorsTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder()
        .AddAnalyzer(() => new PermissiveCors(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"Hotspots\")
        .AddReferences(AdditionalReferences);

#if NET

    internal static IEnumerable<MetadataReference> AdditionalReferences =>
        [
            AspNetCoreMetadataReference.MicrosoftAspNetCoreCors,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpFeatures,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvc,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
            AspNetCoreMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions,
            AspNetCoreMetadataReference.MicrosoftExtensionsPrimitives,
            AspNetCoreMetadataReference.MicrosoftNetHttpHeadersHeaderNames
        ];

    [TestMethod]
    public void PermissiveCors_Latest() =>
        builder.AddPaths("PermissiveCors.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#else

    private static IEnumerable<MetadataReference> AdditionalReferences =>
        NuGetMetadataReference.MicrosoftNetHttpHeaders("2.1.14")
                              .Concat(NuGetMetadataReference.MicrosoftAspNetMvc(Constants.NuGetLatestVersion))
                              .Concat(NuGetMetadataReference.MicrosoftAspNetWebApiCors(Constants.NuGetLatestVersion))
                              .Concat(NuGetMetadataReference.MicrosoftNetWebApiCore(Constants.NuGetLatestVersion))
                              .Concat(FrameworkMetadataReference.SystemWeb)
                              .Concat(FrameworkMetadataReference.SystemNetHttp);

    [TestMethod]
    public void PermissiveCors_AspNet_WebApi() =>
        builder
            .AddPaths("PermissiveCors.NetFramework.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

#endif

}
