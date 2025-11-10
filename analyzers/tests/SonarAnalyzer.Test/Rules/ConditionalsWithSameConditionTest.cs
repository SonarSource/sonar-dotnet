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
    public class ConditionalsWithSameConditionTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<ConditionalsWithSameCondition>();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ConditionalsWithSameCondition() =>
            builder.AddPaths("ConditionalsWithSameCondition.cs").Verify();

#if NET

        [TestMethod]
        public void ConditionalsWithSameCondition_CSharp9() =>
            builder.AddPaths("ConditionalsWithSameCondition.CSharp9.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .Verify();

        [TestMethod]
        public void ConditionalsWithSameCondition_CSharp9_TopLevelStatements() =>
            builder.AddPaths("ConditionalsWithSameCondition.CSharp9.TopLevelStatements.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void ConditionalsWithSameCondition_CSharp10() =>
            builder.AddPaths("ConditionalsWithSameCondition.CSharp10.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

        [TestMethod]
        public void ConditionalsWithSameCondition_RazorFile_CorrectMessage() =>
            builder.AddSnippet(
                """
                @code
                {
                    public void doTheThing(object o) { }

                    public void Method(int a, int b)
                    {
                        if (a == b)
                        {
                            doTheThing(b);
                        }
                        if (a == b) // Noncompliant {{This condition was just checked on line 7.}}
                        //  ^^^^^^
                        {
                            doTheThing(b);
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
