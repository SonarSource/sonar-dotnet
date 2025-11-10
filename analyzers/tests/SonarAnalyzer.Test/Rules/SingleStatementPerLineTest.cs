/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class SingleStatementPerLineTest
    {
        public TestContext TestContext { get; set; }

        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.SingleStatementPerLine>();

        [TestMethod]
        public void SingleStatementPerLine_CS() =>
            builderCS.AddPaths("SingleStatementPerLine.cs").Verify();

#if NET

        [TestMethod]
        public void SingleStatementPerLine_CSharp9() =>
            builderCS.AddPaths("SingleStatementPerLine.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void SingleStatementPerLine_Razor() =>
            builderCS.AddSnippet(
"""
@if (true) { @currentCount } <!-- FN -->
@if (true) { <p>Test</p> } <!-- FN -->

@code
{
    private int currentCount = 0;
    void DoSomething(bool flag) { if (flag) Console.WriteLine("Test"); } // Noncompliant
}
""",
"SomeRazorFile.razor")
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .Verify();

#endif

        [TestMethod]
        public void SingleStatementPerLine_VB() =>
            new VerifierBuilder<VB.SingleStatementPerLine>().AddPaths("SingleStatementPerLine.vb", "SingleStatementPerLine2.vb").WithAutogenerateConcurrentFiles(false).Verify();
    }
}
