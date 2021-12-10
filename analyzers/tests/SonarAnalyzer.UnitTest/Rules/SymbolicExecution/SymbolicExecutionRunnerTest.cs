﻿/*
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
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class SymbolicExecutionRunnerTest
    {
        // This test is meant to run all the symbolic execution rules together and verify different scenarios.
        [TestMethod]
        public void VerifySymbolicExecutionRules() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\SymbolicExecutionRules.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.NETStandard21);

        [TestMethod]
        public void Initialize_MethodBase()
        {
            const string code =
@"public class Sample
{
    public void Main()
    {
        string s = null; // FN Should be Non-compliant { {Message for SMain} }
        s.ToString();    // Noncompliant FP: Compliant, should not raise S2259
    }
}";
            var sut = new SymbolicExecutionRunner();
            Verifier.VerifyCSharpAnalyzer(code, sut);
        }

        [TestMethod]
        public void Initialize_Property()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Initialize_Accessors()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Initialize_AnonymousFunction()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Analyze_DoNotRunWhenContainsDiagnostics()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Enabled_MainScope_MainProject()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Enabled_MainScope_TestProject()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Enabled_TestScope_MainProject()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Enabled_TestScope_TestProject()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Enabled_AllScope_MainProject()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Enabled_AllScope_TestProject_StandaloneRun()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Enabled_AllScope_TestProject_ScannerRun()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Analyze_DescriptorsWithSameType_ExecutesOnce()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Analyze_ShouldExecute_ExecutesWhenAll()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Analyze_ShouldExecute_ExecutesWhenOne()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Analyze_ShouldExecute_DoesNotExecutesWhenNone()
        {
            Assert.Inconclusive();
        }
    }
}
