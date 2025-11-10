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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class LiteralSuffixUpperCaseTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<LiteralSuffixUpperCase>();

        [TestMethod]
        public void LiteralSuffixUpperCase() =>
            builder.AddPaths("LiteralSuffixUpperCase.cs").Verify();

        [TestMethod]
        public void LiteralSuffixUpperCase_CodeFix() =>
            builder.AddPaths("LiteralSuffixUpperCase.cs")
                .WithCodeFix<LiteralSuffixUpperCaseCodeFix>()
                .WithCodeFixedPaths("LiteralSuffixUpperCase.Fixed.cs")
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void LiteralSuffixUpperCase_CSharp10() =>
            builder.AddPaths("LiteralSuffixUpperCase.CSharp10.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

        [TestMethod]
        public void LiteralSuffixUpperCase_CSharp11() =>
            builder.AddPaths("LiteralSuffixUpperCase.CSharp11.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .VerifyNoIssues();

#endif

    }
}
