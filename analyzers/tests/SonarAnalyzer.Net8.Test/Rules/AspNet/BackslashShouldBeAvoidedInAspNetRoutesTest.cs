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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class BackslashShouldBeAvoidedInAspNetRoutesTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BackslashShouldBeAvoidedInAspNetRoutes>().WithBasePath("AspNet");
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.BackslashShouldBeAvoidedInAspNetRoutes>().WithBasePath("AspNet");

#if NET
    private static IEnumerable<MetadataReference> AspNetCore8AndAboveReferences => [
            AspNetCoreMetadataReference.MicrosoftAspNetCore,                    // For WebApplication
            AspNetCoreMetadataReference.MicrosoftExtensionsHostingAbstractions, // For IHost
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,    // For HttpContext, RouteValueDictionary
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcRazorPages,       // For RazorPagesEndpointRouteBuilderExtensions.MapFallbackToPage
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,     // For RazorPagesEndpointRouteBuilderExtensions.MapFallbackToPage
            AspNetCoreMetadataReference.MicrosoftAspNetCoreRouting,             // For IEndpointRouteBuilder

        ];

    [TestMethod]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore8AndAbove_CSharp11() =>
        builderCS
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNetCore8AndAbove.CSharp11.cs")
            .AddReferences(AspNetCore8AndAboveReferences)
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .Verify();

    [TestMethod]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore8AndAbove_VB() =>
        builderVB
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNetCore8AndAbove.vb")
            .AddReferences(AspNetCore8AndAboveReferences)
            .Verify();
#endif
}
