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

using System.Reflection;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class BackslashShouldBeAvoidedInAspNetRoutesTest
{
    private const string AttributePlaceholder = "<attributeName>";
    private const string SlashAndBackslashConstants = """
        private const string ASlash = "/";
        private const string ABackSlash = @"\";
        private const string AConstStringIncludingABackslash = $"A{ABackSlash}";
        private const string AConstStringNotIncludingABackslash = $"A{ASlash}";
        """;

    private static readonly object[][] AttributesWithAllTypesOfStrings =
    [
        [$"[{AttributePlaceholder}(AConstStringIncludingABackslash)]", false, "ConstStringIncludingABackslash"],
        [$"[{AttributePlaceholder}(AConstStringNotIncludingABackslash)]", true, "ConstStringNotIncludingABackslash"],
        [$"""[{AttributePlaceholder}("\u002f[action]")]""", true, "EscapeCodeOfSlash"],
        [$"""[{AttributePlaceholder}("\u005c[action]")]""", false, "EscapeCodeOfBackslash"],
        [$$"""[{{AttributePlaceholder}}($"A{ASlash}[action]")]""", true, "InterpolatedString"],
        [$$"""[{{AttributePlaceholder}}($@"A{ABackSlash}[action]")]""", false, "InterpolatedVerbatimString"],
        [$""""[{AttributePlaceholder}("""\[action]""")]"""", false, "RawStringLiteralsTriple"],
        [$"""""[{AttributePlaceholder}(""""\[action]"""")]""""", false, "RawStringLiteralsQuadruple"],
        [$$$""""[{{{AttributePlaceholder}}}($$"""{{ABackSlash}}/[action]""")]"""", false, "InterpolatedRawStringLiteralsIncludingABackslash"],
        [$$$""""[{{{AttributePlaceholder}}}($$"""{{ASlash}}/[action]""")]"""", true, "InterpolatedRawStringLiteralsNotIncludingABackslash"],
    ];

    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BackslashShouldBeAvoidedInAspNetRoutes>().WithBasePath("AspNet");
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.BackslashShouldBeAvoidedInAspNetRoutes>().WithBasePath("AspNet");

    private static IEnumerable<object[]> RouteAttributesWithAllTypesOfStrings =>
        AttributesWithAllTypesOfStrings.Select(x => new object[] { ((string)x[0]).Replace(AttributePlaceholder, "Route"), x[1], x[2] });

    public static string AttributesWithAllTypesOfStringsDisplayNameProvider(MethodInfo methodInfo, object[] values) =>
        $"{methodInfo.Name}_{(string)values[2]}";

#if NETFRAMEWORK
    // ASP.NET 4x MVC 3 and 4 don't support attribute routing, nor MapControllerRoute and similar
    public static IEnumerable<object[]> AspNet4xMvcVersionsUnderTest =>
        [["5.2.7"] /* Most used */, [Constants.NuGetLatestVersion]];

    private static IEnumerable<MetadataReference> AspNet4xReferences(string aspNetMvcVersion) =>
        MetadataReferenceFacade.SystemWeb                                          // For HttpAttributeMethod and derived attributes
            .Concat(NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion));  // For Controller

    [TestMethod]
    [DynamicData(nameof(AspNet4xMvcVersionsUnderTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNet4x_CS(string aspNetMvcVersion) =>
        builderCS
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNet4x.cs")
            .AddReferences(AspNet4xReferences(aspNetMvcVersion))
            .Verify();

    [TestMethod]
    [DynamicData(
        nameof(RouteAttributesWithAllTypesOfStrings),
        DynamicDataDisplayName = nameof(AttributesWithAllTypesOfStringsDisplayNameProvider),
        DynamicDataDisplayNameDeclaringType = typeof(BackslashShouldBeAvoidedInAspNetRoutesTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNet4x_CSharp11(string actionDeclaration, bool compliant, string displayName)
    {
        actionDeclaration = actionDeclaration.Replace(AttributePlaceholder, "Route");
        var builder = builderCS.AddReferences(AspNet4xReferences("5.2.7")).WithOptions(ParseOptionsHelper.FromCSharp11).AddSnippet($$"""
            using System.Web.Mvc;

            public class WithAllTypesOfStringsController : Controller
            {
                {{SlashAndBackslashConstants}}

                {{(compliant ? actionDeclaration : actionDeclaration + " // Noncompliant")}}
                public ActionResult Index() => View();
            }
            """);

        if (compliant)
        {
            builder.VerifyNoIssueReported();
        }
        else
        {
            builder.Verify();
        }
    }

    [TestMethod]
    [DynamicData(nameof(AspNet4xMvcVersionsUnderTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNet4x_VB(string aspNetMvcVersion) =>
        builderVB
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNet4x.vb")
            .AddReferences(AspNet4xReferences(aspNetMvcVersion))
            .Verify();
#endif

#if NET
    private static IEnumerable<object[]> HttpMethodAttributesWithAllTypesOfStrings =>
        AttributesWithAllTypesOfStrings.Zip(
            ["HttpGet", "HttpPost", "HttpPatch", "HttpHead", "HttpDelete", "HttpOptions", "HttpGet", "HttpPost", "HttpPatch", "HttpHead"],
            (attribute, httpMethod) => new object[] { ((string)attribute[0]).Replace(AttributePlaceholder, httpMethod), attribute[1], attribute[2] });

    public static IEnumerable<object[]> AspNetCore2xVersionsUnderTest =>
        [["2.0.4"] /* Latest 2.0.x */, ["2.2.0"] /* 2nd most used */, [Constants.NuGetLatestVersion]];

    private static IEnumerable<MetadataReference> AspNetCore2xReferences(string aspNetCoreVersion) =>
        NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetCoreVersion)                       // For Controller
            .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspNetCoreVersion))  // For View
            .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcAbstractions(aspNetCoreVersion)); // For IActionResult

    private static IEnumerable<MetadataReference> AspNetCore3AndAboveReferences =>
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
    [DynamicData(nameof(AspNetCore2xVersionsUnderTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore2x_CS(string aspNetCoreVersion) =>
        builderCS
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNetCore2AndAbove.cs")
            .AddReferences(AspNetCore2xReferences(aspNetCoreVersion))
            .Verify();

    [TestMethod]
    [DynamicData(
        nameof(RouteAttributesWithAllTypesOfStrings),
        DynamicDataDisplayName = nameof(AttributesWithAllTypesOfStringsDisplayNameProvider),
        DynamicDataDisplayNameDeclaringType = typeof(BackslashShouldBeAvoidedInAspNetRoutesTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore2x_Route_CSharp11(string actionDeclaration, bool compliant, string displayName) =>
        TestAspNetCoreAttributeDeclaration(
            builderCS.AddReferences(AspNetCore2xReferences("2.2.0")).WithOptions(ParseOptionsHelper.FromCSharp11),
            actionDeclaration,
            compliant);

    [TestMethod]
    [DynamicData(
        nameof(HttpMethodAttributesWithAllTypesOfStrings),
        DynamicDataDisplayName = nameof(AttributesWithAllTypesOfStringsDisplayNameProvider),
        DynamicDataDisplayNameDeclaringType = typeof(BackslashShouldBeAvoidedInAspNetRoutesTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore2x_HttpMethods_CSharp11(string actionDeclaration, bool compliant, string displayName) =>
        TestAspNetCoreAttributeDeclaration(
            builderCS.AddReferences(AspNetCore2xReferences("2.2.0")).WithOptions(ParseOptionsHelper.FromCSharp11),
            actionDeclaration,
            compliant);

    [TestMethod]
    [DynamicData(
        nameof(RouteAttributesWithAllTypesOfStrings),
        DynamicDataDisplayName = nameof(AttributesWithAllTypesOfStringsDisplayNameProvider),
        DynamicDataDisplayNameDeclaringType = typeof(BackslashShouldBeAvoidedInAspNetRoutesTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore3AndAbove_Route_CSharp11(string actionDeclaration, bool compliant, string displayName) =>
        TestAspNetCoreAttributeDeclaration(
            builderCS.AddReferences(AspNetCore3AndAboveReferences).WithOptions(ParseOptionsHelper.FromCSharp11),
            actionDeclaration,
            compliant);

    [TestMethod]
    [DynamicData(
        nameof(HttpMethodAttributesWithAllTypesOfStrings),
        DynamicDataDisplayName = nameof(AttributesWithAllTypesOfStringsDisplayNameProvider),
        DynamicDataDisplayNameDeclaringType = typeof(BackslashShouldBeAvoidedInAspNetRoutesTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore3AndAbove_HttpMethods_CSharp11(string actionDeclaration, bool compliant, string displayName) =>
        TestAspNetCoreAttributeDeclaration(
            builderCS.AddReferences(AspNetCore3AndAboveReferences).WithOptions(ParseOptionsHelper.FromCSharp11),
            actionDeclaration,
            compliant);

    [TestMethod]
    [DynamicData(nameof(AspNetCore2xVersionsUnderTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore2x_VB(string aspNetCoreVersion) =>
        builderVB
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNetCore2x.vb")
            .AddReferences(AspNetCore2xReferences(aspNetCoreVersion))
            .Verify();

    [TestMethod]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore3AndAbove_CS() =>
        builderCS
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNetCore2AndAbove.cs")
            .AddReferences(AspNetCore3AndAboveReferences)
            .Verify();

    [TestMethod]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore3AndAbove_CSharp11() =>
        builderCS
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNetCore3AndAbove.CSharp11.cs")
            .AddReferences(AspNetCore3AndAboveReferences)
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .Verify();

    [TestMethod]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNetCore3AndAbove_VB() =>
        builderVB
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNetCore3AndAbove.vb")
            .AddReferences(AspNetCore3AndAboveReferences)
            .Verify();

    private static void TestAspNetCoreAttributeDeclaration(VerifierBuilder builder, string attributeDeclaration, bool compliant)
    {
        builder = builder.AddSnippet($$"""
            using Microsoft.AspNetCore.Mvc;

            public class WithAllTypesOfStringsController : Controller
            {
                {{SlashAndBackslashConstants}}

                {{(compliant ? attributeDeclaration : attributeDeclaration + " // Noncompliant")}}
                public IActionResult Index() => View();
            }
            """);

        if (compliant)
        {
            builder.VerifyNoIssueReported();
        }
        else
        {
            builder.Verify();
        }
    }
#endif
}
