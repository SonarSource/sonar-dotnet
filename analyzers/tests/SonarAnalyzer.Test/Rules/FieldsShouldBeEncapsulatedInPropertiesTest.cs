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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class FieldsShouldBeEncapsulatedInPropertiesTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<FieldsShouldBeEncapsulatedInProperties>();

    [TestMethod]
    public void FieldsShouldBeEncapsulatedInProperties() =>
        builder.AddPaths("FieldsShouldBeEncapsulatedInProperties.cs").Verify();

    [TestMethod]
    public void FieldsShouldBeEncapsulatedInProperties_Unity3D_Ignored() =>
        builder.AddPaths("FieldsShouldBeEncapsulatedInProperties.Unity3D.cs")
            // Concurrent analysis puts fake Unity3D class into the Concurrent namespace
            .WithConcurrentAnalysis(false)
            .Verify();

#if NET

    [TestMethod]
    public void FieldsShouldBeEncapsulatedInProperties_CSharp9() =>
        builder.AddPaths("FieldsShouldBeEncapsulatedInProperties.CSharp9.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .Verify();

    [TestMethod]
    public void FieldsShouldBeEncapsulatedInProperties_CSharp12() =>
        builder.AddPaths("FieldsShouldBeEncapsulatedInProperties.CSharp12.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .Verify();

#endif

}
