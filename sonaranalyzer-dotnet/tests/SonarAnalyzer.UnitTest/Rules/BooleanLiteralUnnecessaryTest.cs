/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class BooleanLiteralUnnecessaryTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void BooleanLiteralUnnecessary_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\BooleanLiteralUnnecessary.cs",
                new CS.BooleanLiteralUnnecessary());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void BooleanLiteralUnnecessary_CodeFix_CS()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\BooleanLiteralUnnecessary.cs",
                @"TestCases\BooleanLiteralUnnecessary.Fixed.cs",
                new CS.BooleanLiteralUnnecessary(),
                new CS.BooleanLiteralUnnecessaryCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void BooleanLiteralUnnecessary_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\BooleanLiteralUnnecessary.vb",
                new VB.BooleanLiteralUnnecessary());
        }
    }
}
