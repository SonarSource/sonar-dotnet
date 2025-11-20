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
    public class RedundantToArrayCallTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<RedundantToArrayCall>();
        private readonly VerifierBuilder builderWithCodeFix = new VerifierBuilder<RedundantToArrayCall>().WithCodeFix<RedundantToArrayCallCodeFix>();

        [TestMethod]
        public void RedundantToArrayCall() =>
            builder.AddPaths("RedundantToArrayCall.cs").Verify();

        [TestMethod]
        public void RedundantToArrayCall_CodeFix() =>
            builderWithCodeFix
                .AddPaths("RedundantToArrayCall.cs")
                .WithCodeFixedPaths("RedundantToArrayCall.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantToArrayCall_CSharp11() =>
            builder.AddPaths("RedundantToArrayCall.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

        [TestMethod]
        public void RedundantToArrayCall_CSharp11_CodeFix() =>
            builderWithCodeFix
                .AddPaths("RedundantToArrayCall.CSharp11.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .WithCodeFixedPaths("RedundantToArrayCall.CSharp11.Fixed.cs")
                .VerifyCodeFix();
    }
}
