/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;

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
