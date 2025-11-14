/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    public class BooleanCheckInvertedTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BooleanCheckInverted>();

        [TestMethod]
        public void BooleanCheckInverted_CS() =>
            builderCS.AddPaths("BooleanCheckInverted.cs").Verify();

        [TestMethod]
        public void BooleanCheckInverted_CS_CodeFix() =>
            builderCS.AddPaths("BooleanCheckInverted.cs")
                .WithCodeFix<CS.BooleanCheckInvertedCodeFix>()
                .WithCodeFixedPaths("BooleanCheckInverted.Fixed.cs", "BooleanCheckInverted.Fixed.Batch.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void BooleanCheckInverted_VB() =>
            new VerifierBuilder<VB.BooleanCheckInverted>().AddPaths("BooleanCheckInverted.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();

#if NET
        [TestMethod]
        public void BooleanCheckInverted_CS_Latest() =>
            builderCS
                .WithOptions(LanguageOptions.CSharpLatest)
                .WithConcurrentAnalysis(false)
                .AddPaths("BooleanCheckInverted.Latest.cs").Verify();
#endif

    }
}
