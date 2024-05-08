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

using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using ChecksVB = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CalculationsShouldNotOverflowTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.CalculationsShouldNotOverflow.S3949);

    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.SymbolicExecutionRunner>()
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksVB.CalculationsShouldNotOverflow.S3949);

    [TestMethod]
    public void CalculationsShouldNotOverflow_CS() =>
        builderCS.AddPaths("CalculationsShouldNotOverflow.cs").Verify();

    [TestMethod]
    public void CalculationsShouldNotOverflow_VB() =>
        builderVB.AddPaths("CalculationsShouldNotOverflow.vb").Verify();

    [TestMethod]
    public void CalculationsShouldNotOverflow_CSharp8() =>
        builderCS.AddPaths("CalculationsShouldNotOverflow.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).VerifyNoIssues(); // switch exressions are not supported yet

#if NET

    [TestMethod]
    public void CalculationsShouldNotOverflow_CSharp9() =>
        builderCS.AddPaths("CalculationsShouldNotOverflow.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void CalculationsShouldNotOverflow_CSharp10() =>
        builderCS.AddPaths("CalculationsShouldNotOverflow.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

    [TestMethod]
    public void CalculationsShouldNotOverflow_CSharp11() =>
        builderCS.AddPaths("CalculationsShouldNotOverflow.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).VerifyNoIssues();   // list patterns are not supported yet

#endif
}
