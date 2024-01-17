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
                .WithOptions(ParseOptionsHelper.FromCSharp9)
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
