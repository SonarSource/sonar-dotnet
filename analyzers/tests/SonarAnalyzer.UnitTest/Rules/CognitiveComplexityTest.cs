/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CognitiveComplexityTest
    {
        [TestMethod]
        public void CognitiveComplexity_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\CognitiveComplexity.cs", new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 }, ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        public void CognitiveComplexity_CS_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\CognitiveComplexity.CSharp9.cs", new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });

        [TestMethod]
        public void CognitiveComplexity_CS_CSharp10() =>
            Verifier.VerifyAnalyzerFromCSharp10Library(@"TestCases\CognitiveComplexity.CSharp10.cs", new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });
#endif

        [TestMethod]
        public void CognitiveComplexity_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\CognitiveComplexity.vb", new VB.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });

        [TestMethod]
        public void CognitiveComplexity_StackOverflow_CS()
        {
            if (!TestContextHelper.IsAzureDevOpsContext) // ToDo: Test throws OOM on Azure DevOps
            {
                Verifier.VerifyAnalyzer(@"TestCases\SyntaxWalker_InsufficientExecutionStackException.cs", new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });
            }
        }

        [TestMethod]
        public void CognitiveComplexity_StackOverflow_VB()
        {
            if (!TestContextHelper.IsAzureDevOpsContext) // ToDO: Test throws OOM on Azure DevOps
            {
                Verifier.VerifyAnalyzer(@"TestCases\SyntaxWalker_InsufficientExecutionStackException.vb", new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });
            }
        }
    }
}
