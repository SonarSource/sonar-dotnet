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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

#if NET

[TestClass]
public class ControllersHaveMixedResponsibilitiesTest
{
    private readonly VerifierBuilder builder =
        new VerifierBuilder<ControllersHaveMixedResponsibilities>().AddReferences(References).WithBasePath("AspNet");

    private static IEnumerable<MetadataReference> References =>
    [
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
        CoreMetadataReference.SystemComponentModel, // For IServiceProvider
        .. NuGetMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions("8.0.1"), // For IServiceProvider extensions
    ];

    [TestMethod]
    public void ControllersHaveMixedResponsibilities_CS() =>
        builder
            .AddPaths("ControllersHaveMixedResponsibilities.Latest.cs", "ControllersHaveMixedResponsibilities.Latest.Partial.cs")
            .WithLanguageVersion(LanguageVersion.Latest)
            .Verify();
}

#endif
