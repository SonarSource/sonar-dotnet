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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MultipleVariableDeclarationTest
    {
        [TestMethod]
        public void MultipleVariableDeclaration_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\MultipleVariableDeclaration.cs", new CS.MultipleVariableDeclaration());

        [TestMethod]
        public void MultipleVariableDeclaration_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\MultipleVariableDeclaration.vb", new VB.MultipleVariableDeclaration());

        [TestMethod]
        public void MultipleVariableDeclaration_CodeFix_CS() =>
            OldVerifier.VerifyCodeFix<CS.MultipleVariableDeclarationCodeFix>(
                @"TestCases\MultipleVariableDeclaration.cs",
                @"TestCases\MultipleVariableDeclaration.Fixed.cs",
                new CS.MultipleVariableDeclaration(),
                SonarAnalyzer.Rules.Common.MultipleVariableDeclarationCodeFixBase.Title);

        [TestMethod]
        public void MultipleVariableDeclaration_CodeFix_VB() =>
            OldVerifier.VerifyCodeFix<VB.MultipleVariableDeclarationCodeFix>(
                @"TestCases\MultipleVariableDeclaration.vb",
                @"TestCases\MultipleVariableDeclaration.Fixed.vb",
                new VB.MultipleVariableDeclaration(),
                SonarAnalyzer.Rules.Common.MultipleVariableDeclarationCodeFixBase.Title);
    }
}
