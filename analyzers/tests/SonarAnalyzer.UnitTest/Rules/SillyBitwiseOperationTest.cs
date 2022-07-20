/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
        [Ignore][TestMethod]
        public void SillyBitwiseOperation_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SillyBitwiseOperation.cs", new CS.SillyBitwiseOperation());

#if NET
        [Ignore][TestMethod]
        public void SillyBitwiseOperation_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\SillyBitwiseOperation.CSharp9.cs", new CS.SillyBitwiseOperation());
#endif

        [Ignore][TestMethod]
        public void SillyBitwiseOperation_CS_CodeFix() =>
            OldVerifier.VerifyCodeFix<CS.SillyBitwiseOperationCodeFix>(
                @"TestCases\SillyBitwiseOperation.cs",
                @"TestCases\SillyBitwiseOperation.Fixed.cs",
                new CS.SillyBitwiseOperation());

        [Ignore][TestMethod]
        public void SillyBitwiseOperation_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SillyBitwiseOperation.vb", new VB.SillyBitwiseOperation());

        [Ignore][TestMethod]
        public void SillyBitwiseOperation_VB_CodeFix() =>
            OldVerifier.VerifyCodeFix<VB.SillyBitwiseOperationCodeFix>(
                @"TestCases\SillyBitwiseOperation.vb",
                @"TestCases\SillyBitwiseOperation.Fixed.vb",
                new VB.SillyBitwiseOperation());
    }
}
