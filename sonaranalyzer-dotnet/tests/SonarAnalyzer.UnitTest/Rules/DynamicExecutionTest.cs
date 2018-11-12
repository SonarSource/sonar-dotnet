/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using csharp::SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;
using System.Reflection.Metadata;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DynamicExecutionTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DynamicExecution_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DynamicExecution.cs",
                new DynamicExecution(new TestAnalyzerConfiguration(null, "S1523")));
        }

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DynamicExecution_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DynamicExecution.vb",
                new SonarAnalyzer.Rules.VisualBasic.DynamicExecution(new TestAnalyzerConfiguration(null, "S1523")));
        }

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DynamicExecution_CS_Disabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\DynamicExecution.cs",
                new DynamicExecution());
        }

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DynamicExecution_VB_Disabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\DynamicExecution.vb",
                new SonarAnalyzer.Rules.VisualBasic.DynamicExecution());
        }
    }
}

