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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class RouteTemplateShouldNotStartWithSlashTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.RouteTemplateShouldNotStartWithSlash>();

#if NET
    private static IEnumerable<MetadataReference> AspNetCoreReferences =>
        [
            AspNetCoreMetadataReference.MicrosoftAspNetCore,                    // For WebApplication
            AspNetCoreMetadataReference.MicrosoftExtensionsHostingAbstractions, // For IHost
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,    // For HttpContext, RouteValueDictionary
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpFeatures,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcRazorPages,       // For RazorPagesEndpointRouteBuilderExtensions.MapFallbackToPage
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreRouting,             // For IEndpointRouteBuilder
        ];

    [TestMethod]
    public void RouteTemplateShouldNotStartWithSlash_CS() =>
        builderCS
            .AddPaths("RouteTemplateShouldNotStartWithSlash.AspNetCore.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .AddReferences(AspNetCoreReferences)
            .Verify();
#endif

}
