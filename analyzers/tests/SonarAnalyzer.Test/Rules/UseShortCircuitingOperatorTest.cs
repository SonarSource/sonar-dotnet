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
    public class UseShortCircuitingOperatorTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseShortCircuitingOperator>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseShortCircuitingOperator>();

        [TestMethod]
        public void UseShortCircuitingOperators_VisualBasic() =>
            builderVB.AddPaths("UseShortCircuitingOperator.vb").Verify();

        [TestMethod]
        public void UseShortCircuitingOperators_VisualBasic_CodeFix() =>
            builderVB.WithCodeFix<VB.UseShortCircuitingOperatorCodeFix>()
                .AddPaths("UseShortCircuitingOperator.vb")
                .WithCodeFixedPaths("UseShortCircuitingOperator.Fixed.vb")
                .VerifyCodeFix();

        [TestMethod]
        public void UseShortCircuitingOperators_CSharp() =>
            builderCS.AddPaths("UseShortCircuitingOperator.cs").Verify();

#if NET

        [TestMethod]
        public void UseShortCircuitingOperators_CSharp9() =>
            builderCS.AddPaths("UseShortCircuitingOperator.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void UseShortCircuitingOperators_CSharp9_CodeFix() =>
            builderCS.WithCodeFix<CS.UseShortCircuitingOperatorCodeFix>()
                .AddPaths("UseShortCircuitingOperator.CSharp9.cs")
                .WithCodeFixedPaths("UseShortCircuitingOperator.CSharp9.Fixed.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .VerifyCodeFix();

#endif

        [TestMethod]
        public void UseShortCircuitingOperators_CSharp_CodeFix() =>
            builderCS.WithCodeFix<CS.UseShortCircuitingOperatorCodeFix>()
                .AddPaths("UseShortCircuitingOperator.cs")
                .WithCodeFixedPaths("UseShortCircuitingOperator.Fixed.cs")
                .VerifyCodeFix();
    }
}
