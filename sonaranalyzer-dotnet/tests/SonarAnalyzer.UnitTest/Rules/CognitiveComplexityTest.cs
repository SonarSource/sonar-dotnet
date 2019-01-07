/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Helpers;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CognitiveComplexityTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void CognitiveComplexity_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CognitiveComplexity.cs",
                new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CognitiveComplexity_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CognitiveComplexity.vb",
                new VB.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CognitiveComplexity_StackOverflow_CS()
        {
            if (!TestContextHelper.IsAzureDevOpsContext) // FIX ME: Test throws OOM on Azure DevOps
            {
                Verifier.VerifyAnalyzer(@"TestCases\SyntaxWalker_InsufficientExecutionStackException.cs",
                    new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });
            }
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CognitiveComplexity_StackOverflow_VB()
        {
            if (!TestContextHelper.IsAzureDevOpsContext) // FIX ME: Test throws OOM on Azure DevOps
            {
                Verifier.VerifyAnalyzer(@"TestCases\SyntaxWalker_InsufficientExecutionStackException.vb",
                    new CS.CognitiveComplexity { Threshold = 0, PropertyThreshold = 0 });
            }
        }
    }
}
