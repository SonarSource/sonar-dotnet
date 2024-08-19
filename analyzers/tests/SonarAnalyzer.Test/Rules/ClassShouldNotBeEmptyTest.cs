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
public class ClassShouldNotBeEmptyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ClassShouldNotBeEmpty>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ClassShouldNotBeEmpty>();

    [TestMethod]
    public void ClassShouldNotBeEmpty_CS() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.cs")
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_VB() =>
        builderVB
            .AddPaths("ClassShouldNotBeEmpty.vb")
            .Verify();

#if NET

    private static readonly MetadataReference[] AdditionalReferences =
    [
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcRazorPages
    ];

    [TestMethod]
    public void ClassShouldNotBeEmpty_CSharp9() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.CSharp9.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_CSharp10() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.CSharp10.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_CSharp12() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.CSharp12.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_Inheritance_CS() =>
        builderCS
            .AddPaths("ClassShouldNotBeEmpty.Inheritance.cs")
            .AddReferences(AdditionalReferences)
            .Verify();

    [TestMethod]
    public void ClassShouldNotBeEmpty_Inheritance_VB() =>
        builderVB
            .AddPaths("ClassShouldNotBeEmpty.Inheritance.vb")
            .AddReferences(AdditionalReferences)
            .Verify();

#endif
}
