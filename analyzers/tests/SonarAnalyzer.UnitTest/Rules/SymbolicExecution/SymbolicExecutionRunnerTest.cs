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
        public void Initialize_MethodBase() =>
            VerifyClassMain(@"
public Sample() // ConstructorDeclaration
{
    string s = null;   // Noncompliant {{Message for SMain}}
}

public Sample(string s) => // ConstructorDeclaration
    s = null;   // Noncompliant {{Message for SMain}}

~Sample() // DestructorDeclaration
{
    string s = null;   // Noncompliant {{Message for SMain}}
}

public static implicit operator byte(Sample arg)
{
    string s = null;   // Noncompliant {{Message for SMain}}
    return 0;
}

public static explicit operator Sample(byte arg)
{
    string s = null;   // Noncompliant {{Message for SMain}}
    return null;
}

public static int operator +(Sample a, Sample b)  // OperatorDeclaration
{
    string s = null;   // Noncompliant {{Message for SMain}}
    return 0;
}

public void MethodDeclaration()
{
    string s = null;   // Noncompliant {{Message for SMain}}
}

public void MethodDeclaration(string s) =>
    s = null;   // Noncompliant {{Message for SMain}}");

        [TestMethod]
        public void Initialize_Property() =>
            VerifyClassMain(@"
private int target;
public int Property => target = 42;     // Noncompliant {{Message for SMain}}");

        [TestMethod]
        public void Initialize_Accessors() =>
            VerifyClassMain(@"
private string target;

public string BodyProperty
{
    get
    {
        string s = null;   // Noncompliant {{Message for SMain}}
        return s;
    }
    set
    {
        string s = value;   // Noncompliant {{Message for SMain}}
    }
}

public string ArrowProperty
{
    get => target = null;   // Noncompliant {{Message for SMain}}
    set => target = value;  // Noncompliant {{Message for SMain}}
}

public event EventHandler BlockEvent
{
    add
    {
        string s = null;   // Noncompliant {{Message for SMain}}
    }
    remove
    {
        string s = null;   // Noncompliant {{Message for SMain}}
    }
}

public event EventHandler ArrowEvent
{
    add => target = null;       // Noncompliant {{Message for SMain}}
    remove => target = null;    // Noncompliant {{Message for SMain}}
}");

#if NET

        [TestMethod]
        public void Initialize_Accessors_Init() =>
            VerifyClass(@"
private string target;

public string InitOnlyPropertyBlock
{
    get => null;
    init
    {
        string s = null;   // Noncompliant {{Message for SMain}}
    }
}

public string InitOnlyPropertyArrow
{
    get => null;
    init => target = value; // Noncompliant {{Message for SMain}}
}",
                    MainScopeAssignmentRuleCheck.SMain);

#endif

        [TestMethod]
        public void Initialize_AnonymousFunction() =>
            VerifyClassMain(@"
delegate void VoidDelegate();

public void Method()
{
    Action parenthesizedLambda = () =>  // Noncompliant {{Message for SMain}} - scaffolding noise
        {
            string s = null;            // Noncompliant {{Message for SMain}}
        };

    Action<int> simpleLambda = x =>     // Noncompliant {{Message for SMain}} - scaffolding noise
        {
            string s = null;            // Noncompliant {{Message for SMain}}
        };

    VoidDelegate anonymousMethod = delegate     // Noncompliant {{Message for SMain}} - scaffolding noise
    {
        string s = null;                // Noncompliant {{Message for SMain}}
    };
}");

        [TestMethod]
        public void Analyze_DoNotRunWhenContainsDiagnostics()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Enabled_MainScope_MainProject() =>
            Verify(@"string s = null;   // Noncompliant    {{Message for SAll}}
                                        // Noncompliant@-1 {{Message for SMain}}");


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
            var code =
@"public class Sample
{
    public void Method()
    {
        string s = null;   // Noncompliant {{Message for SMain}} - this should be raised only once
    }
}";
            var another = new DiagnosticDescriptor("SAnother", "Title", "Message", "Category", DiagnosticSeverity.Warning, true, customTags: DiagnosticDescriptorBuilder.MainSourceScopeTag);
            var sut = new SymbolicExecutionRunner();
            sut.RegisterRule<MainScopeAssignmentRuleCheck>(MainScopeAssignmentRuleCheck.SMain);
            sut.RegisterRule<MainScopeAssignmentRuleCheck>(another);     // Register the same RuleCheck with another ID
            Verifier.VerifyCSharpAnalyzer(code, sut, ParseOptionsHelper.FromCSharp9, onlyDiagnostics: new[] { MainScopeAssignmentRuleCheck.SMain, another });
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
$@"public void Main()
{{
    {body}
}}";
            VerifyClass(code, onlyRules);
        }

        private static void VerifyClassMain(string members) =>
            VerifyClass(members, MainScopeAssignmentRuleCheck.SMain);

        private static void VerifyClass(string members, params DiagnosticDescriptor[] onlyRules)
        {
            var code =
$@"using System;
using System.Linq;
public class Sample
{{
    {members}
}}";
            var sut = new SymbolicExecutionRunner();
            sut.RegisterRule<AllScopeAssignmentRuleCheck>(AllScopeAssignmentRuleCheck.SAll);
            sut.RegisterRule<MainScopeAssignmentRuleCheck>(MainScopeAssignmentRuleCheck.SMain);
            sut.RegisterRule<TestScopeAssignmentRuleCheck>(TestScopeAssignmentRuleCheck.STest);
            Verifier.VerifyCSharpAnalyzer(code, sut, ParseOptionsHelper.FromCSharp9, onlyDiagnostics: onlyRules);
        }
    }
}
