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
    public class BooleanLiteralUnnecessaryTest
    {
        [TestMethod]
        public void BooleanLiteralUnnecessary_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\BooleanLiteralUnnecessary.cs", new CS.BooleanLiteralUnnecessary());

        [TestMethod]
        public void BooleanLiteralUnnecessary_CodeFix_CS() =>
            OldVerifier.VerifyCodeFix(
                @"TestCases\BooleanLiteralUnnecessary.cs",
                @"TestCases\BooleanLiteralUnnecessary.Fixed.cs",
                new CS.BooleanLiteralUnnecessary(),
                new CS.BooleanLiteralUnnecessaryCodeFixProvider());

        [TestMethod]
        public void BooleanLiteralUnnecessary_CSharp8() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\BooleanLiteralUnnecessary.CSharp8.cs", new CS.BooleanLiteralUnnecessary(), ParseOptionsHelper.FromCSharp8);

        [TestMethod]
        public void BooleanLiteralUnnecessary_CodeFix_CSharp8() =>
            OldVerifier.VerifyCodeFix(
                @"TestCases\BooleanLiteralUnnecessary.CSharp8.cs",
                @"TestCases\BooleanLiteralUnnecessary.CSharp8.Fixed.cs",
                new CS.BooleanLiteralUnnecessary(),
                new CS.BooleanLiteralUnnecessaryCodeFixProvider(),
                ParseOptionsHelper.FromCSharp8);

#if NET

        [TestMethod]
        public void BooleanLiteralUnnecessary_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\BooleanLiteralUnnecessary.CSharp9.cs", new CS.BooleanLiteralUnnecessary());

        [TestMethod]
        public void BooleanLiteralUnnecessary_CodeFix_CSharp9() =>
            OldVerifier.VerifyCodeFix(
                @"TestCases\BooleanLiteralUnnecessary.CSharp9.cs",
                @"TestCases\BooleanLiteralUnnecessary.CSharp9.Fixed.cs",
                new CS.BooleanLiteralUnnecessary(),
                new CS.BooleanLiteralUnnecessaryCodeFixProvider(),
                ParseOptionsHelper.FromCSharp9);

#endif

        [TestMethod]
        public void BooleanLiteralUnnecessary_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\BooleanLiteralUnnecessary.vb", new VB.BooleanLiteralUnnecessary());
    }
}
