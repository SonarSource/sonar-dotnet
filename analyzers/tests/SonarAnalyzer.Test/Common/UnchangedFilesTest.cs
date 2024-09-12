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
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class UnchangedFilesTest
{
    public TestContext TestContext { get; set; }

    [DataTestMethod]
    [DataRow(@"SymbolicExecution\Roslyn\NullPointerDereference.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_SymbolicExecutionRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder()
            .AddAnalyzer(() => new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
            .AddPaths(@"SymbolicExecution\Roslyn\NullPointerDereference.cs")
            .WithOnlyDiagnostics(NullPointerDereference.S2259);
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [DataTestMethod]
    [DataRow("ClassNotInstantiatable.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_SymbolBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder<ClassNotInstantiatable>().AddPaths("ClassNotInstantiatable.cs");
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [DataTestMethod]
    [DataRow("AbstractTypesShouldNotHaveConstructors.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_SyntaxNodesBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder<AbstractTypesShouldNotHaveConstructors>().AddPaths("AbstractTypesShouldNotHaveConstructors.cs");
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [DataTestMethod]
    [DataRow("FileLines20.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_SyntaxTreeBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder().AddAnalyzer(() => new FileLines { Maximum = 10 }).AddPaths("FileLines20.cs").WithAutogenerateConcurrentFiles(false);
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [DataTestMethod]
    [DataRow(@"Hotspots\LooseFilePermissions.Windows.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_CompilationStartBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder().AddAnalyzer(() => new LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled)).AddPaths(@"Hotspots\LooseFilePermissions.Windows.cs");
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [DataTestMethod]
    [DataRow("UnusedPrivateMember.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_ReportDiagnosticIfNonGeneratedBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder<UnusedPrivateMember>().AddPaths("UnusedPrivateMember.cs");
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    private void UnchangedFiles_Verify(VerifierBuilder builder, string unchangedFileName, bool expectEmptyResults)
    {
        builder = builder.WithConcurrentAnalysis(false).WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, unchangedFileName));
        if (expectEmptyResults)
        {
            builder.VerifyNoIssuesIgnoreErrors();
        }
        else
        {
            builder.Verify();
        }
    }
}
