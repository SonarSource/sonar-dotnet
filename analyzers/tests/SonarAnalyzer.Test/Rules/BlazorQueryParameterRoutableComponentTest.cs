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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class BlazorQueryParameterRoutableComponentTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<BlazorQueryParameterRoutableComponent>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void BlazorQueryParameterRoutableComponent_Blazor() =>
        builder.AddPaths("BlazorQueryParameterRoutableComponent.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void BlazorQueryParameterRoutableComponent_BlazorNoRoute() =>
        builder.AddPaths("BlazorQueryParameterRoutableComponent.NoRoute.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void BlazorQueryParameterRoutableComponent_Partial() =>
        builder.WithOptions(LanguageOptions.CSharpLatest)
            .AddPaths(
                "BlazorQueryParameterRoutableComponent.Latest.Partial.razor",
                "BlazorQueryParameterRoutableComponent.Latest.Partial.1.razor.cs",
                "BlazorQueryParameterRoutableComponent.Latest.Partial.2.razor.cs")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void BlazorQueryParameterRoutableComponent_CS() =>
        builder.AddPaths("BlazorQueryParameterRoutableComponent.Compliant.cs", "BlazorQueryParameterRoutableComponent.Noncompliant.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreComponents("7.0.13"))
            .Verify();
}
