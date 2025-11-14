/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

#if NET

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class AvoidLambdaExpressionInLoopsInBlazorTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<AvoidLambdaExpressionInLoopsInBlazor>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void AvoidLambdaExpressionInLoopsInBlazor_Blazor() =>
        builder.AddPaths("AvoidLambdaExpressionInLoopsInBlazor.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void AvoidLambdaExpressionInLoopsInBlazor_BlazorLoopsWithNoBody() =>
        builder.AddPaths("AvoidLambdaExpressionInLoopsInBlazor.LoopsWithNoBody.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void AvoidLambdaExpressionInLoopsInBlazor_UsingRenderFragment() =>
        builder.AddPaths("AvoidLambdaExpressionInLoopsInBlazor.RenderFragment.razor", "AvoidLambdaExpressionInLoopsInBlazor.RenderFragmentConsumer.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void AvoidLambdaExpressionInLoopsInBlazor_CS() =>
        builder.AddPaths("AvoidLambdaExpressionInLoopsInBlazor.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreComponents("7.0.13"))
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreComponentsWeb("7.0.13"))
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();
}

#endif
