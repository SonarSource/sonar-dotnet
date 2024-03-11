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
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BackslashShouldBeAvoidedInAspNetRoutes>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.BackslashShouldBeAvoidedInAspNetRoutes>();

#if NETFRAMEWORK

    // ASP.NET 4x MVC 3 and 4 don't support attribute routing, nor MapControllerRoute and similar
    public static IEnumerable<object[]> AspNet4xMvcVersionsUnderTest => [["5.2.7"] /* Most used */, [Constants.NuGetLatestVersion]];

    private static IEnumerable<MetadataReference> AspNet4xReferences(string aspNetMvcVersion) =>
        MetadataReferenceFacade.SystemWeb
            .Concat(NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion));

    [TestMethod]
    [DynamicData(nameof(AspNet4xMvcVersionsUnderTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNet4x_CS(string aspNetMvcVersion) =>
        builderCS
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNet4x.cs")
            .AddReferences(AspNet4xReferences(aspNetMvcVersion))
            .Verify();

    [TestMethod]
    [DynamicData(nameof(AspNet4xMvcVersionsUnderTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNet4x_CSharp11(string aspNetMvcVersion) =>
        builderCS
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNet4x.CSharp11.cs")
            .AddReferences(AspNet4xReferences(aspNetMvcVersion))
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .Verify();

    [TestMethod]
    [DynamicData(nameof(AspNet4xMvcVersionsUnderTest))]
    public void BackslashShouldBeAvoidedInAspNetRoutes_AspNet4x_VB(string aspNetMvcVersion) =>
        builderVB
            .AddPaths("BackslashShouldBeAvoidedInAspNetRoutes.AspNet4x.vb")
            .AddReferences(AspNet4xReferences(aspNetMvcVersion))
            .Verify();

#endif

}
