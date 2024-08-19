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
    private readonly VerifierBuilder sonar = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg)) // SE part
        .AddAnalyzer(() => new CS.InvalidCastToInterface())                                                 // Syntax-based part
        .WithBasePath(@"SymbolicExecution\Sonar")
        .WithOnlyDiagnostics(CS.InvalidCastToInterface.S1944);

    // The SE part that is empty and doesn't report anything. It exists to prevent the old SE rule from running.
    // When the old SE engine is removed, the SymbolicExecutionRunner runner should be removed from this class and test cases should be moved out of SymbolicExecution directory.
    private readonly VerifierBuilder roslynCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .AddAnalyzer(() => new CS.InvalidCastToInterface())                                         // Syntax-based part
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(CS.InvalidCastToInterface.S1944);

    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.InvalidCastToInterface>();  // Syntax-based part only

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void InvalidCastToInterface_Sonar_CS(ProjectType projectType) =>
        sonar.AddPaths("InvalidCastToInterface.cs")
            .AddReferences(TestHelper.ProjectTypeReference(projectType).Union(MetadataReferenceFacade.NetStandard21))
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void InvalidCastToInterface_Roslyn_CS(ProjectType projectType) =>
        roslynCS.AddPaths("InvalidCastToInterface.cs").AddReferences(TestHelper.ProjectTypeReference(projectType)).Verify();

    [TestMethod]
    public void InvalidCastToInterface_VB() =>
        builderVB.AddPaths("InvalidCastToInterface.vb").Verify();

#if NET

    [TestMethod]
    public void InvalidCastToInterface_Sonar_CSharp9() =>
        sonar.AddPaths("InvalidCastToInterface.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void InvalidCastToInterface_Roslyn_CSharp9() =>
        roslynCS.AddPaths("InvalidCastToInterface.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void InvalidCastToInterface_Sonar_CSharp10() =>
        sonar.AddPaths("InvalidCastToInterface.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

    [TestMethod]
    public void InvalidCastToInterface_Roslyn_CSharp10() =>
        roslynCS.AddPaths("InvalidCastToInterface.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif
}
