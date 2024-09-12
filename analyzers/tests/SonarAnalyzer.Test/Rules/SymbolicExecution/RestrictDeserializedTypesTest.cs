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
public class RestrictDeserializedTypesTest
{
    private readonly VerifierBuilder sonar = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
        .WithBasePath(@"SymbolicExecution\Sonar")
        .AddReferences(AdditionalReferences())
        .WithOnlyDiagnostics(RestrictDeserializedTypes.S5773);

    private readonly VerifierBuilder roslynCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .AddReferences(AdditionalReferences())
        .WithOnlyDiagnostics(ChecksCS.RestrictDeserializedTypes.S5773);

    private readonly VerifierBuilder roslynVB = new VerifierBuilder<VB.SymbolicExecutionRunner>()
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .AddReferences(AdditionalReferences())
        .WithOnlyDiagnostics(ChecksVB.RestrictDeserializedTypes.S5773);

#if NETFRAMEWORK // These serializers are available only when targeting .Net Framework

    [TestMethod]
    public void RestrictDeserializedTypesFormatters_Sonar()
    {
        sonar.AddPaths("RestrictDeserializedTypes.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();
    }

    [TestMethod]
    public void RestrictDeserializedTypesFormatters_Roslyn_CS() =>
        roslynCS.AddPaths("RestrictDeserializedTypes.cs")
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypesFormatters_Roslyn_VB() =>
        roslynVB.AddPaths("RestrictDeserializedTypes.vb")
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypesFormatters_Roslyn_CSharp8() =>
        roslynCS.AddPaths("RestrictDeserializedTypes.CSharp8.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypes_DoesNotRaiseIssuesForTestProject_Sonar() =>
        sonar.AddPaths("RestrictDeserializedTypes.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .AddTestReference()
            .VerifyNoIssues();

    [TestMethod]
    public void RestrictDeserializedTypes_DoesNotRaiseIssuesForTestProject_Roslyn_CS() =>
        roslynCS.AddPaths("RestrictDeserializedTypes.cs")
            .AddTestReference()
            .VerifyNoIssues();

    [TestMethod]
    public void RestrictDeserializedTypes_DoesNotRaiseIssuesForTestProject_Roslyn_VB() =>
        roslynVB.AddPaths("RestrictDeserializedTypes.vb")
            .AddTestReference()
            .VerifyNoIssues();

    [TestMethod]
    public void RestrictDeserializedTypesJavaScriptSerializer_Sonar() =>
        sonar.AddPaths("RestrictDeserializedTypes.JavaScriptSerializer.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypesJavaScriptSerializer_Roslyn_CS() =>
        roslynCS.AddPaths("RestrictDeserializedTypes.JavaScriptSerializer.cs")
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypesJavaScriptSerializer_Roslyn_VB() =>
        roslynVB.AddPaths("RestrictDeserializedTypes.JavaScriptSerializer.vb")
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypesLosFormatter_Sonar() =>
        sonar.AddPaths("RestrictDeserializedTypes.LosFormatter.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypesLosFormatter_Roslyn_CS() =>
        roslynCS.AddPaths("RestrictDeserializedTypes.LosFormatter.cs")
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypesLosFormatter_Roslyn_VB() =>
        roslynVB.AddPaths("RestrictDeserializedTypes.LosFormatter.vb")
            .Verify();

    private static IEnumerable<MetadataReference> AdditionalReferences() =>
        FrameworkMetadataReference.SystemRuntimeSerialization
        .Union(FrameworkMetadataReference.SystemRuntimeSerializationFormattersSoap)
        .Union(FrameworkMetadataReference.SystemWeb)
        .Union(FrameworkMetadataReference.SystemWebExtensions);

#endif

#if NET

    [TestMethod]
    public void RestrictDeserializedTypesFormatters_Sonar_CSharp9() =>
        sonar.AddPaths("RestrictDeserializedTypes.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void RestrictDeserializedTypesFormatters_Roslyn_CSharp9() =>
        roslynCS.AddPaths("RestrictDeserializedTypes.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

    private static IEnumerable<MetadataReference> AdditionalReferences() =>
        new[] { CoreMetadataReference.SystemRuntimeSerializationFormatters };

#endif

}
