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

using SonarAnalyzer.Test.Helpers;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class FunctionComplexityTest
    {
        [TestMethod]
        public void FunctionComplexity_CS() =>
            CreateCSBuilder(3).AddPaths("FunctionComplexity.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET
        [TestMethod]
        public void FunctionComplexity_CS_Latest() =>
            CreateCSBuilder(3).AddPaths("FunctionComplexity.Latest.cs").WithTopLevelStatements().WithOptions(ParseOptionsHelper.CSharpLatest).Verify();

#endif

        [TestMethod]
        public void FunctionComplexity_InsufficientExecutionStack_CS()
        {
            if (!TestContextHelper.IsAzureDevOpsContext) // ToDo: Test doesn't work on Azure DevOps
            {
                CreateCSBuilder(3).AddPaths("SyntaxWalker_InsufficientExecutionStackException.cs").VerifyNoIssues();
            }
        }

        [TestMethod]
        public void FunctionComplexity_VB() =>
            new VerifierBuilder().AddAnalyzer(() => new VB.FunctionComplexity { Maximum = 3 })
                .AddPaths("FunctionComplexity.vb")
                .Verify();

        private static VerifierBuilder CreateCSBuilder(int maxComplexityScore) =>
            new VerifierBuilder().AddAnalyzer(() => new CS.FunctionComplexity { Maximum = maxComplexityScore });
    }
}
