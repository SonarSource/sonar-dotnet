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

#if NET

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ParameterTypeShouldMatchRouteTypeConstraintTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ParameterTypeShouldMatchRouteTypeConstraint>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void ParameterTypeShouldMatchRouteTypeConstraint_Blazor() =>
        builder.AddPaths("ParameterTypeShouldMatchRouteTypeConstraint.razor").Verify();

    [TestMethod]
    public void ParameterTypeShouldMatchRouteTypeConstraint_Partial() =>
        builder.AddPaths("ParameterTypeShouldMatchRouteTypeConstraint.Partial.razor", "ParameterTypeShouldMatchRouteTypeConstraint.Partial.razor.cs").Verify();

    [TestMethod]
    public void ParameterTypeShouldMatchRouteTypeConstraint_CS() =>
        builder.AddPaths("ParameterTypeShouldMatchRouteTypeConstraint.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreComponents("7.0.13"))
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .Verify();

    [TestMethod]
    public void ParameterTypeShouldMatchRouteTypeConstraint_Conversion() =>
        builder.AddPaths("ParameterTypeShouldMatchRouteTypeConstraint.Conversion.razor").Verify();
}

#endif
