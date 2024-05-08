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
public class NullPointerDereferenceTest
{
    private readonly VerifierBuilder sonar = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
        .WithBasePath(@"SymbolicExecution\Sonar")
        .WithOnlyDiagnostics(ChecksCS.NullPointerDereference.S2259);

    private readonly VerifierBuilder roslynCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.NullPointerDereference.S2259);

    private readonly VerifierBuilder roslynVB = new VerifierBuilder<VB.SymbolicExecutionRunner>()
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksVB.NullPointerDereference.S2259);

    [TestMethod]
    public void NullPointerDereference_Sonar_CS() =>
        sonar.AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).Verify();

    [TestMethod]
    public void NullPointerDereference_Roslyn_CS() =>
        roslynCS.AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).Verify();

    [TestMethod]
    public void NullPointerDereference_VB() =>
        roslynVB.AddPaths("NullPointerDereference.vb").Verify();

    [TestMethod]
    public void NullPointerDereference_Sonar_DoesNotRaiseIssuesForTestProject() =>
        sonar.AddTestReference().AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).VerifyNoIssues();

    [TestMethod]
    public void NullPointerDereference_Roslyn_DoesNotRaiseIssuesForTestProject() =>
        roslynCS.AddTestReference().AddPaths("NullPointerDereference.cs").WithConcurrentAnalysis(false).VerifyNoIssues();

    [TestMethod]
    public void NullPointerDereference_Sonar_CSharp6() =>
        sonar.AddPaths("NullPointerDereference.CSharp6.cs").WithOptions(ParseOptionsHelper.FromCSharp6).Verify();

    [TestMethod]
    public void NullPointerDereference_Roslyn_CSharp6() =>
        roslynCS.AddPaths("NullPointerDereference.CSharp6.cs").WithOptions(ParseOptionsHelper.FromCSharp6).Verify();

    [TestMethod]
    public void NullPointerDereference_Sonar_CSharp7() =>
        sonar.AddPaths("NullPointerDereference.CSharp7.cs").WithOptions(ParseOptionsHelper.FromCSharp7).Verify();

    [TestMethod]
    public void NullPointerDereference_Roslyn_CSharp7() =>
        roslynCS.AddPaths("NullPointerDereference.CSharp7.cs").WithOptions(ParseOptionsHelper.FromCSharp7).Verify();

    [TestMethod]
    public void NullPointerDereference_Sonar_CSharp8() =>
        sonar.AddPaths("NullPointerDereference.CSharp8.cs").AddReferences(MetadataReferenceFacade.NetStandard21).WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

    [TestMethod]
    public void NullPointerDereference_Roslyn_CSharp8() =>
        roslynCS.AddPaths("NullPointerDereference.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

    [TestMethod]
    public void NullPointerDereference_Roslyn_CSharp8_Nullable() =>
        roslynCS.AddPaths("NullPointerDereference.CSharp8.Nullable.cs").WithOptions(ParseOptionsHelper.FromCSharp8).WithConcurrentAnalysis(false).Verify();

    [TestMethod]
    public void NullPointerDereference_Sonar_CSharp9() =>
        sonar.AddPaths("NullPointerDereference.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void NullPointerDereference_Roslyn_CSharp9() =>
        roslynCS.AddPaths("NullPointerDereference.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void NullPointerDereference_Sonar_CSharp10() =>
        sonar.AddPaths("NullPointerDereference.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10)
            .VerifyNoIssues(); // FN, mixed declarations and expressions in deconstruction is not supported

    [TestMethod]
    public void NullPointerDereference_Roslyn_CSharp10() =>
        roslynCS.AddPaths("NullPointerDereference.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

    [TestMethod]
    public void NullPointerDereference_Roslyn_CS_NetCore() =>
        roslynCS.AddPaths("NullPointerDereference.NetCore.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

    [TestMethod]
    public void NullPointerDereference_Sonar_CSharp11() =>
        sonar.AddPaths("NullPointerDereference.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

    [TestMethod]
    public void NullPointerDereference_Roslyn_CSharp11() =>
        roslynCS.AddPaths("NullPointerDereference.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

#else

    [TestMethod]
    public void NullPointerDereference_Roslyn_CS_NetFx() =>
        roslynCS.AddPaths("NullPointerDereference.NetFx.cs").VerifyNoIssues();

#endif

}
