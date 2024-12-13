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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class UnnecessaryBitwiseOperationTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UnnecessaryBitwiseOperation>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UnnecessaryBitwiseOperation>();

        [TestMethod]
        public void UnnecessaryBitwiseOperation_CS() =>
            builderCS.AddPaths("UnnecessaryBitwiseOperation.cs").Verify();

#if NET

        [TestMethod]
        public void UnnecessaryBitwiseOperation_CS_TopLevelStatements() =>
            builderCS.AddPaths("UnnecessaryBitwiseOperation.TopLevelStatements.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void UnnecessaryBitwiseOperation_CS_Latest() =>
            builderCS.AddPaths("UnnecessaryBitwiseOperation.Latest.cs")
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();

#endif

        [TestMethod]
        public void UnnecessaryBitwiseOperation_CS_CodeFix() =>
            builderCS.AddPaths("UnnecessaryBitwiseOperation.cs")
                .WithCodeFix<CS.UnnecessaryBitwiseOperationCodeFix>()
                .WithCodeFixedPaths("UnnecessaryBitwiseOperation.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void UnnecessaryBitwiseOperation_VB() =>
            builderVB.AddPaths("UnnecessaryBitwiseOperation.vb").Verify();

        [TestMethod]
        public void UnnecessaryBitwiseOperation_VB_CodeFix() =>
            builderVB.AddPaths("UnnecessaryBitwiseOperation.vb")
                .WithCodeFix<VB.UnnecessaryBitwiseOperationCodeFix>()
                .WithCodeFixedPaths("UnnecessaryBitwiseOperation.Fixed.vb")
                .VerifyCodeFix();
    }
}
