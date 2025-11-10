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

#if NET

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ParameterTypeShouldMatchRouteTypeConstraintTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ParameterTypeShouldMatchRouteTypeConstraint>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void ParameterTypeShouldMatchRouteTypeConstraint_Blazor() =>
        builder.AddPaths("ParameterTypeShouldMatchRouteTypeConstraint.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void ParameterTypeShouldMatchRouteTypeConstraint_Partial() =>
        builder.AddPaths("ParameterTypeShouldMatchRouteTypeConstraint.Partial.razor", "ParameterTypeShouldMatchRouteTypeConstraint.Partial.razor.cs")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    public void ParameterTypeShouldMatchRouteTypeConstraint_CS() =>
        builder.AddPaths("ParameterTypeShouldMatchRouteTypeConstraint.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreComponents("7.0.13"))
            .WithOptions(LanguageOptions.FromCSharp11)
            .Verify();

    [TestMethod]
    public void ParameterTypeShouldMatchRouteTypeConstraint_Conversion() =>
        builder.AddPaths("ParameterTypeShouldMatchRouteTypeConstraint.Conversion.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();
}

#endif
