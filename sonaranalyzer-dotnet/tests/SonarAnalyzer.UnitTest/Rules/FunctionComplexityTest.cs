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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class FunctionComplexityTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void FunctionComplexity_CSharp()
        {
            var diagnostic = new SonarAnalyzer.Rules.CSharp.FunctionComplexity { Maximum = 3 };
            Verifier.VerifyAnalyzer(@"TestCases\FunctionComplexity.cs", diagnostic);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void FunctionComplexity_InsufficientExecutionStack_CSharp()
        {
            if (!TestContextHelper.IsAzureDevOpsContext) // FIX ME: Test doesn't work on Azure DevOps
            {
                var diagnostic = new SonarAnalyzer.Rules.CSharp.FunctionComplexity { Maximum = 3 };
                Verifier.VerifyAnalyzer(@"TestCases\SyntaxWalker_InsufficientExecutionStackException.cs", diagnostic);
            }
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void FunctionComplexity_VisualBasic()
        {
            var diagnostic = new SonarAnalyzer.Rules.VisualBasic.FunctionComplexity { Maximum = 3 };
            Verifier.VerifyAnalyzer(@"TestCases\FunctionComplexity.vb", diagnostic);
        }
    }
}
