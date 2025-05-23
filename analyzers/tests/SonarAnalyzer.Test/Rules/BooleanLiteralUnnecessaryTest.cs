﻿/*
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
    public class BooleanLiteralUnnecessaryTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BooleanLiteralUnnecessary>();

        [TestMethod]
        public void BooleanLiteralUnnecessary_CS() =>
            builderCS.AddPaths("BooleanLiteralUnnecessary.cs").Verify();

        [TestMethod]
        public void BooleanLiteralUnnecessary_CodeFix_CS() =>
            builderCS.AddPaths("BooleanLiteralUnnecessary.cs")
                .WithCodeFix<CS.BooleanLiteralUnnecessaryCodeFix>()
                .WithCodeFixedPaths("BooleanLiteralUnnecessary.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void BooleanLiteralUnnecessary_CSharp8() =>
            builderCS.AddPaths("BooleanLiteralUnnecessary.CSharp8.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void BooleanLiteralUnnecessary_CodeFix_CSharp8() =>
            builderCS.AddPaths("BooleanLiteralUnnecessary.CSharp8.cs")
                .WithCodeFix<CS.BooleanLiteralUnnecessaryCodeFix>()
                .WithCodeFixedPaths("BooleanLiteralUnnecessary.CSharp8.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void BooleanLiteralUnnecessary_CSharp9() =>
            builderCS.AddPaths("BooleanLiteralUnnecessary.CSharp9.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void BooleanLiteralUnnecessary_CodeFix_CSharp9() =>
            builderCS.AddPaths("BooleanLiteralUnnecessary.CSharp9.cs")
                .WithCodeFix<CS.BooleanLiteralUnnecessaryCodeFix>()
                .WithCodeFixedPaths("BooleanLiteralUnnecessary.CSharp9.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .VerifyCodeFix();

#endif

        [TestMethod]
        public void BooleanLiteralUnnecessary_VB() =>
            new VerifierBuilder<VB.BooleanLiteralUnnecessary>().AddPaths("BooleanLiteralUnnecessary.vb").Verify();
    }
}
