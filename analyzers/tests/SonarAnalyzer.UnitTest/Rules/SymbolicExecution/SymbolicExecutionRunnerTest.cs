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

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

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
            Assert.Inconclusive();
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
        public void Analyze_Severity_ExecutesWhenAll() =>
            Verify(@"string s = null;   // Noncompliant    {{Message for SAll}}
                                        // Noncompliant@-1 {{Message for SMain}}
                    s.ToString();       // Noncompliant    {{'s' is null on at least one execution path.}} - rule S2259");

        [TestMethod]
        public void Analyze_Severity_ExecutesWhenMore() =>
            Verify(@"string s = null;   // Noncompliant    {{Message for SAll}}
                                        // Noncompliant@-1 {{Message for SMain}}
                    s.ToString();       // Compliant, should not raise S2259",
                AllScopeAssignmentRuleCheck.SAll,
                MainScopeAssignmentRuleCheck.SMain);


        [TestMethod]
        public void Analyze_Severity_ExecutesWhenOne() =>
            Verify(@"string s = null; // Noncompliant {{Message for SMain}}
                     s.ToString();    // Compliant, should not raise S2259",
                MainScopeAssignmentRuleCheck.SMain);

        [TestMethod]
        public void Analyze_Severity_DoesNotExecutesWhenNone() =>
            Verify(@"string s = null;   // Compliant, SMain and SAll are suppressed by test framework, because only 'SAnother' is active
                     s.ToString();      // Compliant, should not raise S2259",
                new DiagnosticDescriptor("SAnother", "Non-SE rule", "Message", "Category", DiagnosticSeverity.Warning, true, customTags: DiagnosticDescriptorBuilder.MainSourceScopeTag));

        private static void Verify(string body, params DiagnosticDescriptor[] onlyRules)
        {
            var code =
$@"public class Sample
{{
    public void Main()
    {{
        {body}
    }}
}}";
            var sut = new SymbolicExecutionRunner();
            sut.RegisterRule<AllScopeAssignmentRuleCheck>(AllScopeAssignmentRuleCheck.SAll);
            sut.RegisterRule<MainScopeAssignmentRuleCheck>(MainScopeAssignmentRuleCheck.SMain);
            Verifier.VerifyCSharpAnalyzer(code, sut, null, onlyRules);
        }
    }
}
