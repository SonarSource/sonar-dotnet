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
public class SetPropertiesInsteadOfMethodsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.SetPropertiesInsteadOfMethods>();

    [TestMethod]
    public void SetPropertiesInsteadOfMethods_CS() =>
        builderCS.AddPaths("SetPropertiesInsteadOfMethods.cs").Verify();

#if NET

    [TestMethod]
    public void SetPropertiesInsteadOfMethods_CS_Immutable() =>
        builderCS.AddPaths("SetPropertiesInsteadOfMethods.Immutable.cs")
            .WithTopLevelStatements()
            .AddReferences(MetadataReferenceFacade.SystemCollections)
            .Verify();

#endif

    [TestMethod]
    public void SetPropertiesInsteadOfMethods_VB() =>
        new VerifierBuilder<VB.SetPropertiesInsteadOfMethods>().AddPaths("SetPropertiesInsteadOfMethods.vb").Verify();
}
