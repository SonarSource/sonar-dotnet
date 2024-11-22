/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
                .WithCodeFixedPaths("RedundantModifier.Fixed.Unsafe.cs")
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleUnsafe)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Checked_CodeFix() =>
            builder.AddPaths("RedundantModifier.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Fixed.Checked.cs")
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleChecked)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Partial_CodeFix() =>
            builder.AddPaths("RedundantModifier.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Fixed.Partial.cs")
                .WithCodeFixTitle(RedundantModifierCodeFix.TitlePartial)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Sealed_CodeFix() =>
            builder.AddPaths("RedundantModifier.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Fixed.Sealed.cs")
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleSealed)
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void RedundantModifier_Latest() =>
            builder.AddPaths("RedundantModifier.Latest.cs", "RedundantModifier.Latest.Partial.cs")
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .Verify();

        [TestMethod]
        public void RedundantModifier_Checked_CodeFix_Latest() =>
            builder.AddPaths("RedundantModifier.Latest.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Latest.Fixed.Checked.cs")
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleChecked)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Partial_CodeFix_Latest() =>
            builder.AddPaths("RedundantModifier.Latest.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Latest.Fixed.Partial.cs")
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitlePartial)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Sealed_CodeFix_Latest() =>
            builder.AddPaths("RedundantModifier.Latest.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Latest.Fixed.Sealed.cs")
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleSealed)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantModifier_Unsafe_CodeFix_Latest() =>
            builder.AddPaths("RedundantModifier.Latest.cs")
                .WithCodeFix<RedundantModifierCodeFix>()
                .WithCodeFixedPaths("RedundantModifier.Latest.Fixed.Unsafe.cs")
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .WithCodeFixTitle(RedundantModifierCodeFix.TitleUnsafe)
                .VerifyCodeFix();

#endif
    }
}
