/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class PreferGuidEmptyTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.PreferGuidEmpty>().WithOptions(LanguageOptions.FromCSharp8);
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.PreferGuidEmpty>();

        [TestMethod]
        public void PreferGuidEmpty_CS() =>
            builderCS.AddPaths("PreferGuidEmpty.cs").Verify();

#if NET
        [TestMethod]
        public void PreferGuidEmpty_CSharp9() =>
            builderCS.WithOptions(LanguageOptions.FromCSharp9).AddPaths("PreferGuidEmpty.CSharp9.cs").Verify();
#endif

        [TestMethod]
        public void PreferGuidEmpty_VB() =>
            builderVB.AddPaths("PreferGuidEmpty.vb").Verify();

        [TestMethod]
        public void PreferGuidEmpty_CodeFix_CS() =>
            builderCS
            .AddPaths("PreferGuidEmpty.cs")
            .WithCodeFix<CS.PreferGuidEmptyCodeFix>()
            .WithCodeFixedPaths("PreferGuidEmpty.Fixed.cs")
            .VerifyCodeFix();
    }
}
