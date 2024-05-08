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

using SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;

using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using ChecksVB = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ConditionEvaluatesToConstantTest
{
    private readonly VerifierBuilder sonar = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
        .WithBasePath(@"SymbolicExecution\Sonar")
        .WithOnlyDiagnostics(ConditionEvaluatesToConstant.S2583, ConditionEvaluatesToConstant.S2589);

    private readonly VerifierBuilder roslynCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.ConditionEvaluatesToConstant.S2583, ChecksCS.ConditionEvaluatesToConstant.S2589);

    private readonly VerifierBuilder roslynVB = new VerifierBuilder<VB.SymbolicExecutionRunner>()
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksVB.ConditionEvaluatesToConstant.S2583, ChecksVB.ConditionEvaluatesToConstant.S2589);

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void ConditionEvaluatesToConstant_Sonar_CS(ProjectType projectType) =>
        sonar.AddPaths("ConditionEvaluatesToConstant.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsPrimitives("3.1.7").Concat(TestHelper.ProjectTypeReference(projectType)))
            .Verify();

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void ConditionEvaluatesToConstant_Roslyn_CS(ProjectType projectType) =>
        roslynCS.AddPaths("ConditionEvaluatesToConstant.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsPrimitives("3.1.7"))
            .AddReferences(MetadataReferenceFacade.SystemComponentModelTypeConverter)
            .AddReferences(TestHelper.ProjectTypeReference(projectType))
            .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_VB() =>
        roslynVB.AddPaths("ConditionEvaluatesToConstant.vb").WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_VB14() =>
    roslynVB.AddPaths("ConditionEvaluatesToConstant.VB14.vb")
        .WithOptions(ParseOptionsHelper.FromVisualBasic14)
        .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Sonar_CSharp7() =>
        sonar.AddPaths("ConditionEvaluatesToConstant.CSharp7.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp7)
            .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_CSharp7() =>
        roslynCS.AddPaths("ConditionEvaluatesToConstant.CSharp7.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp7)
            .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Sonar_CSharp8() =>
        sonar.AddPaths("ConditionEvaluatesToConstant.CSharp8.cs")
            .AddReferences(MetadataReferenceFacade.NetStandard21)
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_CSharp8() =>
        roslynCS.AddPaths("ConditionEvaluatesToConstant.CSharp8.cs")
            .AddReferences(MetadataReferenceFacade.NetStandard21)
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

#if NET

    [TestMethod]
    public void ConditionEvaluatesToConstant_Sonar_CSharp9() =>
        sonar.AddPaths("ConditionEvaluatesToConstant.CSharp9.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_CSharp9() =>
        roslynCS.AddPaths("ConditionEvaluatesToConstant.CSharp9.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Sonar_CSharp9_TopLevelStatements() =>
        sonar.AddPaths("ConditionEvaluatesToConstant.CSharp9.TopLevelStatements.cs")
            .WithTopLevelStatements()
            .VerifyNoIssues();   // top level statements are not supported

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_CSharp9_TopLevelStatements() =>
        roslynCS.AddPaths("ConditionEvaluatesToConstant.CSharp9.TopLevelStatements.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Sonar_CSharp10() =>
        sonar.AddPaths("ConditionEvaluatesToConstant.CSharp10.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .WithConcurrentAnalysis(false)
            .VerifyNoIssues();   // parameterless constructors for structs are not supported

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_CSharp10() =>
        roslynCS.AddPaths("ConditionEvaluatesToConstant.CSharp10.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void ConditionEvaluatesToConstant_Sonar_CSharp11() =>
        sonar.AddPaths("ConditionEvaluatesToConstant.CSharp11.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .VerifyNoIssues();   // list patterns and array sensitivity are not supported yet

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_CSharp11() =>
        roslynCS.AddPaths("ConditionEvaluatesToConstant.CSharp11.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .VerifyNoIssues();   // list patterns and array sensitivity are not supported yet

    [TestMethod]
    public void ConditionEvaluatesToConstant_Roslyn_CSharp12() =>
        roslynCS.AddPaths("ConditionEvaluatesToConstant.CSharp12.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .Verify();

#endif

}
