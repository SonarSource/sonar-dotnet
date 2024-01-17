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
    public class ConditionalStructureSameConditionTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ConditionalStructureSameCondition>();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ConditionalStructureSameCondition_CS() =>
            builderCS.AddPaths("ConditionalStructureSameCondition.cs").Verify();

#if NET

        [TestMethod]
        public void ConditionalStructureSameCondition_CS_CSharp9() =>
            builderCS.AddPaths("ConditionalStructureSameCondition.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void ConditionalStructureSameCondition_CS_CSharp10() =>
            builderCS.AddPaths("ConditionalStructureSameCondition.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void ConditionalStructureSameCondition_RazorFile_CorrectMessage() =>
            builderCS.AddSnippet(
                """
                @code
                {
                    public bool condition { get; set; }

                    public void Method()
                    {
                        var b = true;
                        if (b && condition)
                        //  ^^^^^^^^^^^^^^ Secondary
                        {

                        }
                        else if (b && condition) // Noncompliant {{This branch duplicates the one on line 8.}}
                        //       ^^^^^^^^^^^^^^
                        {

                        }
                    }
                }
                """,
                "SomeRazorFile.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

#endif

        [TestMethod]
        public void ConditionalStructureSameCondition_VisualBasic() =>
            new VerifierBuilder<VB.ConditionalStructureSameCondition>().AddPaths("ConditionalStructureSameCondition.vb").Verify();
    }
}
