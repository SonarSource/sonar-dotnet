/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

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
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

        [TestMethod]
        public void IndentSingleLineFollowingConditional_RazorFile_CorrectMessage() =>
            builder.AddSnippet(
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
