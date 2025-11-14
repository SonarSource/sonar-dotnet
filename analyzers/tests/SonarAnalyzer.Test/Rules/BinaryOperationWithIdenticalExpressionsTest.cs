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
    public class BinaryOperationWithIdenticalExpressionsTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BinaryOperationWithIdenticalExpressions>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.BinaryOperationWithIdenticalExpressions>();

        [TestMethod]
        public void BinaryOperationWithIdenticalExpressions_CS() =>
            builderCS.AddPaths("BinaryOperationWithIdenticalExpressions.cs").Verify();

        [TestMethod]
        public void BinaryOperationWithIdenticalExpressions_TestProject_CS() =>
            builderCS.AddPaths("BinaryOperationWithIdenticalExpressions.cs")
                .AddTestReference()
                .VerifyNoIssues();

        [TestMethod]
        public void BinaryOperationWithIdenticalExpressions_VB() =>
            builderVB.AddPaths("BinaryOperationWithIdenticalExpressions.vb").Verify();

        [TestMethod]
        public void BinaryOperationWithIdenticalExpressions_TestProject_VB() =>
            builderVB.AddPaths("BinaryOperationWithIdenticalExpressions.vb")
                .AddTestReference()
                .VerifyNoIssues();

#if NET

        [TestMethod]
        public void BinaryOperationWithIdenticalExpressions_CSharp11() =>
            builderCS.AddPaths("BinaryOperationWithIdenticalExpressions.CSharp11.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

#endif

    }
}
