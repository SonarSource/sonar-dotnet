/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ExpressionComplexityTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder().AddAnalyzer(() => new CS.ExpressionComplexity { Maximum = 3 });

        [TestMethod]
        public void ExpressionComplexity_CSharp8() =>
            builderCS.AddPaths("ExpressionComplexity.cs")
                .WithOptions(LanguageOptions.FromCSharp8)
                .Verify();

        [DataTestMethod]
        [DataRow("==")]
        [DataRow("!=")]
        [DataRow("<")]
        [DataRow("<=")]
        [DataRow(">")]
        [DataRow(">=")]
        public void ExpressionComplexity_TransparentComparissionOperators(string @operator) =>
            builderCS.AddSnippet($$$"""
            class C
            {
                public void M()
                {
                    var x = true && true && (1 {{{@operator}}} (true ? 1 : 1));         // Compliant (Make sure, the @operator is not increasing complexity)
                    var y = true && true && true && (1 {{{@operator}}} (true ? 1 : 1)); // Noncompliant {{Reduce the number of conditional operators (4) used in the expression (maximum allowed 3).}}
                }
            }
            """)
            .Verify();

        [DataTestMethod]
        [DataRow("o", "??")]
        [DataRow("i", "|")]
        [DataRow("i", "^")]
        [DataRow("i", "&")]
        [DataRow("i", ">>")]

#if NET

        [DataRow("i", ">>>")]

#endif

        [DataRow("i", "<<")]
        [DataRow("i", "+")]
        [DataRow("i", "-")]
        [DataRow("i", "*")]
        [DataRow("i", "/")]
        [DataRow("i", "%")]
        public void ExpressionComplexity_TransparentBinaryOperators(string parameter, string @operator) =>
            builderCS.AddSnippet($$"""
            class C
            {
                public void M(int i, object o)
                {
                    var x = true && true && (({{parameter}} {{@operator}} (true ? {{parameter}} : {{parameter}})) == {{parameter}});         // Compliant
                    var y = true && true && true && (({{parameter}} {{@operator}} (true ? {{parameter}} : {{parameter}})) == {{parameter}}); // Noncompliant
                }
            }
            """)

#if NET

            .WithOptions(LanguageOptions.FromCSharp11)

#endif

            .Verify();

#if NET

        [TestMethod]
        public void ExpressionComplexity_CSharp9() =>
            builderCS.AddPaths("ExpressionComplexity.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void ExpressionComplexity_CSharp10() =>
            builderCS.AddPaths("ExpressionComplexity.CSharp10.cs")
                .WithTopLevelStatements()
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

        [TestMethod]
        public void ExpressionComplexity_CSharp11() =>
            builderCS.AddPaths("ExpressionComplexity.CSharp11.cs")
                .WithTopLevelStatements()
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

#endif

        [TestMethod]
        public void ExpressionComplexity_VB() =>
            new VerifierBuilder().AddAnalyzer(() => new VB.ExpressionComplexity { Maximum = 3 }).AddPaths("ExpressionComplexity.vb").Verify();
    }
}
