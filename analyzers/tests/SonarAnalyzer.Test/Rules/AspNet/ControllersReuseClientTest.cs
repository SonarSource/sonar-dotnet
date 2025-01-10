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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

#if NET

[TestClass]
public class ControllersReuseClientTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ControllersReuseClient>()
        .WithBasePath("AspNet")
        .AddReferences(
        [
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
            .. MetadataReferenceFacade.SystemThreadingTasks,
            .. NuGetMetadataReference.SystemNetHttp(),
            .. NuGetMetadataReference.MicrosoftExtensionsHttp()
        ]);

    [TestMethod]
    public void ControllersReuseClient_CS() =>
        builder
            .AddPaths("ControllersReuseClient.cs")
            .Verify();

    [TestMethod]
    public void ControllersReuseClient_CS8() =>
        builder
            .AddPaths("ControllersReuseClient.CSharp8.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void ControllersReuseClient_CS9() =>
        builder
            .AddPaths("ControllersReuseClient.CSharp9.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void ControllersReuseClient_CS12() =>
        builder
            .AddPaths("ControllerReuseClient.CSharp12.cs")
            .WithOptions(LanguageOptions.FromCSharp12).Verify();
}
#endif
