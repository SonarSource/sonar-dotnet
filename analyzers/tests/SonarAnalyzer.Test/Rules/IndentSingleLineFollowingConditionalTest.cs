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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class IndentSingleLineFollowingConditionalTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<IndentSingleLineFollowingConditional>();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void IndentSingleLineFollowingConditional() =>
            builder.AddPaths("IndentSingleLineFollowingConditional.cs").Verify();

#if NET

        [TestMethod]
        public void IndentSingleLineFollowingConditional_FromCSharp9() =>
            builder.AddPaths("IndentSingleLineFollowingConditional.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void IndentSingleLineFollowingConditional_FromCSharp11() =>
            builder.AddPaths("IndentSingleLineFollowingConditional.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void IndentSingleLineFollowingConditional_RazorFile_CorrectMessage() =>
            builder
                .AddSnippet(
                    """
                    @code
                    {
                        public int Method(int j)
                        {
                            var total = 0;
                            for(int i = 0; i < 10; i++) // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'for'}}
                    //      ^^^^^^^^^^^^^^^^^^^^^^^^^^^
                            total = total + i;               // trivia not included in secondary location for single line statements...
                    //      ^^^^^^^^^^^^^^^^^^ Secondary

                            if (j > 400)
                                return 4;
                            else if (j > 500) // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'else if'}}
                    //      ^^^^^^^^^^^^^^^^^
                        return 5;
                    //  ^^^^^^^^^ Secondary

                            return 1623;
                        }
                    }
                    """,
                    "SomeRazorFile.razor")
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .Verify();

#endif

    }
}
