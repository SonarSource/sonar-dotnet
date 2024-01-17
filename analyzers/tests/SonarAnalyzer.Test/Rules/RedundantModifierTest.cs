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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class RedundantModifierTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<RedundantModifier>();

        [TestMethod]
        public void RedundantModifier() =>
            builder.AddPaths("RedundantModifier.cs").Verify();

        [TestMethod]
        public void RedundantModifier_Unsafe_CodeFix() =>
            builder.AddPaths("RedundantModifier.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Unsafe.Fixed.cs")
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleUnsafe)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Checked_CodeFix() =>
            builder.AddPaths("RedundantModifier.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Checked.Fixed.cs")
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleChecked)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Partial_CodeFix() =>
            builder.AddPaths("RedundantModifier.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Partial.Fixed.cs")
                .WithCodeFixTitle(RedundantModifierCodeFix.TitlePartial)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Sealed_CodeFix() =>
            builder.AddPaths("RedundantModifier.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Sealed.Fixed.cs")
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleSealed)
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void RedundantModifier_CSharp9() =>
            builder.AddPaths("RedundantModifier.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void RedundantModifier_Checked_CodeFix_CSharp9() =>
            builder.AddPaths("RedundantModifier.CSharp9.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp9.Checked.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleChecked)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Partial_CodeFix_CSharp9() =>
            builder.AddPaths("RedundantModifier.CSharp9.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp9.Partial.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitlePartial)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Sealed_CodeFix_CSharp9() =>
            builder.AddPaths("RedundantModifier.CSharp9.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp9.Sealed.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleSealed)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Unsafe_CodeFix_CSharp9() =>
            builder.AddPaths("RedundantModifier.CSharp9.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp9.Unsafe.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleUnsafe)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_CSharp10() =>
            builder.AddPaths("RedundantModifier.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void RedundantModifier_Unsafe_CodeFix_CSharp10() =>
            builder.AddPaths("RedundantModifier.CSharp10.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp10.Unsafe.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleUnsafe)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Checked_CodeFix_CSharp10() =>
            builder.AddPaths("RedundantModifier.CSharp10.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp10.Checked.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleChecked)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_CSharp11() =>
            builder.AddPaths("RedundantModifier.CSharp11.cs", "RedundantModifier.CSharp11.Partial.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .Verify();

        [TestMethod]
        public void RedundantModifier_Partial_CodeFix_CSharp11() =>
            builder.AddPaths("RedundantModifier.CSharp11.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp11.Partial.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitlePartial)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Unsafe_CodeFix_CSharp11() =>
            builder.AddPaths("RedundantModifier.CSharp11.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp11.Unsafe.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleUnsafe)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Checked_CodeFix_CSharp11() =>
            builder.AddPaths("RedundantModifier.CSharp11.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.CSharp11.Checked.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleChecked)
                .VerifyCodeFix();

#endif

    }
}
