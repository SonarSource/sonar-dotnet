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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CognitiveComplexityTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder().AddAnalyzer(() => new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });
        private readonly VerifierBuilder builderVB = new VerifierBuilder().AddAnalyzer(() => new VB.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });

        [TestMethod]
        public void CognitiveComplexity_CS() =>
            builderCS.AddPaths("CognitiveComplexity.cs")
                .WithOptions(LanguageOptions.FromCSharp8)
                .Verify();

#if NET
        [TestMethod]
        public void CognitiveComplexity_CS_Latest() =>
            builderCS.AddPaths("CognitiveComplexity.Latest.cs")
                .WithTopLevelStatements()
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();
#endif

        [TestMethod]
        public void CognitiveComplexity_VB() => builderVB.AddPaths("CognitiveComplexity.vb").Verify();

        [TestMethod]
        public void CognitiveComplexity_StackOverflow_CS()
        {
            if (!TestEnvironment.IsAzureDevOpsContext) // ToDo: Test throws OOM on Azure DevOps
            {
                builderCS.AddPaths("SyntaxWalker_InsufficientExecutionStackException.cs").VerifyNoIssues();
            }
        }

        [TestMethod]
        public void CognitiveComplexity_StackOverflow_VB()
        {
            if (!TestEnvironment.IsAzureDevOpsContext) // ToDO: Test throws OOM on Azure DevOps
            {
                builderVB.AddPaths("SyntaxWalker_InsufficientExecutionStackException.vb").VerifyNoIssues();
            }
        }
    }
}
