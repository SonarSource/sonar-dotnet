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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ConditionalStructureSameImplementationTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ConditionalStructureSameImplementation>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ConditionalStructureSameImplementation>();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ConditionalStructureSameImplementation_If_CSharp() =>
            builderCS.AddPaths("ConditionalStructureSameImplementation_If.cs").Verify();

        [TestMethod]
        public void ConditionalStructureSameImplementation_Switch_CSharp() =>
            builderCS.AddPaths("ConditionalStructureSameImplementation_Switch.cs").Verify();

        [TestMethod]
        public void ConditionalStructureSameImplementation_If_VisualBasic() =>
            builderVB.AddPaths("ConditionalStructureSameImplementation_If.vb").Verify();

        [TestMethod]
        public void ConditionalStructureSameImplementation_Switch_VisualBasic() =>
            builderVB.AddPaths("ConditionalStructureSameImplementation_Switch.vb").Verify();

#if NET
        [TestMethod]
        public void ConditionalStructureSameImplementation_Switch_CSharp_Latest() =>
            builderCS.AddPaths("ConditionalStructureSameImplementation_Switch.Latest.cs").WithOptions(ParseOptionsHelper.CSharpLatest).VerifyNoIssues();

        [TestMethod]
        public void ConditionalStructureSameImplementation_RazorFile_CorrectMessage() =>
            builderCS.AddSnippet(
                """
                @code
                {
                    public bool someCondition1 { get; set; }
                    public void DoSomething1() { }

                    public void Method()
                    {
                        if (someCondition1)
                        { // Secondary
                            DoSomething1();
                            DoSomething1();
                        }
                        else
                        { // Noncompliant {{Either merge this branch with the identical one on line 9 or change one of the implementations.}}
                            DoSomething1();
                            DoSomething1();
                        }
                    }
                }
                """,
                "SomeRazorFile.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();
#endif
    }
}
