/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SillyBitwiseOperationTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.SillyBitwiseOperation>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.SillyBitwiseOperation>();

        [TestMethod]
        public void SillyBitwiseOperation_CS() =>
            builderCS.AddPaths("SillyBitwiseOperation.cs").Verify();

#if NET

        [TestMethod]
        public void SillyBitwiseOperation_CSharp9() =>
            builderCS.AddPaths("SillyBitwiseOperation.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

#endif

        [TestMethod]
        public void SillyBitwiseOperation_CS_CodeFix() =>
            builderCS.AddPaths("SillyBitwiseOperation.cs")
                .WithCodeFix<CS.SillyBitwiseOperationCodeFix>()
                .WithCodeFixedPaths("SillyBitwiseOperation.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void SillyBitwiseOperation_VB() =>
            builderVB.AddPaths("SillyBitwiseOperation.vb").Verify();

        [TestMethod]
        public void SillyBitwiseOperation_VB_CodeFix() =>
            builderVB.AddPaths("SillyBitwiseOperation.vb")
                .WithCodeFix<VB.SillyBitwiseOperationCodeFix>()
                .WithCodeFixedPaths("SillyBitwiseOperation.Fixed.vb")
                .VerifyCodeFix();
    }
}
