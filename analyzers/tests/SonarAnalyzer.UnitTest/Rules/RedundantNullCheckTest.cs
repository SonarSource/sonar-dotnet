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

using Microsoft.CodeAnalysis;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundantNullCheckTest
    {
        [TestMethod]
        public void RedundantNullCheck_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\RedundantNullCheck.cs", new CS.RedundantNullCheck());

#if NET
        [TestMethod]
        public void RedundantNullCheck_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\RedundantNullCheck.CSharp9.cs", new CS.RedundantNullCheck());

        [TestMethod]
        public void RedundantNullCheck_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Console(@"TestCases\RedundantNullCheck.CSharp10.cs", new CS.RedundantNullCheck());
#endif

        [TestMethod]
        public void RedundantNullCheck_CS_CodeFix() =>
            OldVerifier.VerifyCodeFix<CS.RedundantNullCheckCodeFix>(
                @"TestCases\RedundantNullCheck.cs",
                @"TestCases\RedundantNullCheck.Fixed.cs",
                @"TestCases\RedundantNullCheck.Fixed.Batch.cs",
                new CS.RedundantNullCheck());

#if NET
        [TestMethod]
        public void RedundantNullCheck_CSharp9_CodeFix() =>
            OldVerifier.VerifyCodeFix<CS.RedundantNullCheckCodeFix>(
                @"TestCases\RedundantNullCheck.CSharp9.cs",
                @"TestCases\RedundantNullCheck.CSharp9.Fixed.cs",
                new CS.RedundantNullCheck(),
                ParseOptionsHelper.FromCSharp9,
                OutputKind.ConsoleApplication);

        [TestMethod]
        public void RedundantNullCheck_CSharp10_CodeFix() =>
            OldVerifier.VerifyCodeFix<CS.RedundantNullCheckCodeFix>(
                @"TestCases\RedundantNullCheck.CSharp10.cs",
                @"TestCases\RedundantNullCheck.CSharp10.Fixed.cs",
                new CS.RedundantNullCheck(),
                ParseOptionsHelper.FromCSharp10,
                OutputKind.ConsoleApplication);
#endif

        [TestMethod]
        public void RedundantNullCheck_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\RedundantNullCheck.vb", new VB.RedundantNullCheck());
    }
}
