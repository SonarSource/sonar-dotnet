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
public class InvalidCastToInterfaceTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.InvalidCastToInterface>();  // Syntax-based part of the rule, there also exists Sonar SE part
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.InvalidCastToInterface>();  // Syntax-based part only

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void InvalidCastToInterface_CS(ProjectType projectType) =>
        builderCS.AddPaths("InvalidCastToInterface.cs")
            .AddReferences(TestHelper.ProjectTypeReference(projectType).Union(MetadataReferenceFacade.NetStandard21))
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [TestMethod]
    public void InvalidCastToInterface_VB() =>
        builderVB.AddPaths("InvalidCastToInterface.vb").Verify();

#if NET

    [TestMethod]
    public void InvalidCastToInterface_CSharp9() =>
        builderCS.AddPaths("InvalidCastToInterface.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void InvalidCastToInterface_CSharp10() =>
        builderCS.AddPaths("InvalidCastToInterface.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif
}
