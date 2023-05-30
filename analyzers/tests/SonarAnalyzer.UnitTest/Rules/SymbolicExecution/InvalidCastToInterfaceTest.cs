/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using SonarAnalyzer.Common;
using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class InvalidCastToInterfaceTest
{
    private readonly VerifierBuilder sonar = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg)) // SE part
        .AddAnalyzer(() => new CS.InvalidCastToInterface())                                                    // Syntax-based part
        .WithBasePath(@"SymbolicExecution\Sonar")
        .WithOnlyDiagnostics(ChecksCS.InvalidCastToInterface.S1944);

    private readonly VerifierBuilder roslynCS = new VerifierBuilder()
        // ToDo: .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))     // SE part
        .AddAnalyzer(() => new CS.InvalidCastToInterface())                                         // Syntax-based part
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.InvalidCastToInterface.S1944);

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void InvalidCastToInterface_Sonar_CS(ProjectType projectType) =>
        sonar.AddPaths("InvalidCastToInterface.cs")
            .AddReferences(TestHelper.ProjectTypeReference(projectType).Union(MetadataReferenceFacade.NETStandard21))
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void InvalidCastToInterface_Roslyn_CS(ProjectType projectType) =>
        roslynCS.AddPaths("InvalidCastToInterface.cs").AddReferences(TestHelper.ProjectTypeReference(projectType)).Verify();

#if NET

    [TestMethod]
    public void InvalidCastToInterface_Roslyn_CSharp8() =>
        roslynCS.AddPaths("InvalidCastToInterface.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

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
