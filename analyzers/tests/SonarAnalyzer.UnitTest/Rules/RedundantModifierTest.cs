/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundantModifierTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<RedundantModifier>();

        [TestMethod]
        public void RedundantModifier() =>
            builder.AddPaths("RedundantModifier.cs").Verify();

        [TestMethod]
        public void RedundantModifier_CodeFix() =>
            builder.AddPaths("RedundantModifier.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Fixed.cs")
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void RedundantModifier_CSharp9() =>
            builder.AddPaths("RedundantModifier.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void RedundantModifier_CodeFix_CSharp9() =>
            builder.AddPaths("RedundantModifier.CSharp9.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp9.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_CSharp10() =>
            builder.AddPaths("RedundantModifier.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void RedundantModifier_CodeFix_CSharp10() =>
            builder.AddPaths("RedundantModifier.CSharp10.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp10.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .VerifyCodeFix();

#endif

    }
}
