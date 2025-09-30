/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class UnchangedFilesTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("ClassNotInstantiatable.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_SymbolBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder<ClassNotInstantiatable>().AddPaths("ClassNotInstantiatable.cs");
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [TestMethod]
    [DataRow("AbstractTypesShouldNotHaveConstructors.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_SyntaxNodesBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder<AbstractTypesShouldNotHaveConstructors>().AddPaths("AbstractTypesShouldNotHaveConstructors.cs");
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [TestMethod]
    [DataRow("FileLines20.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_SyntaxTreeBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder().AddAnalyzer(() => new FileLines { Maximum = 10 }).AddPaths("FileLines20.cs").WithAutogenerateConcurrentFiles(false);
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [TestMethod]
    [DataRow(@"Hotspots\LooseFilePermissions.Windows.cs", true)]
    [DataRow("SomeOtherFile.cs", false)]
    public void UnchangedFiles_CompilationStartBasedRule(string unchangedFileName, bool expectEmptyResults)
    {
        var builder = new VerifierBuilder().AddAnalyzer(() => new LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled)).AddPaths(@"Hotspots\LooseFilePermissions.Windows.cs");
        UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
    }

    [TestMethod]
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
