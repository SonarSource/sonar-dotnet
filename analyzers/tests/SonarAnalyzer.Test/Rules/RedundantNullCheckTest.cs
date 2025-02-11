/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public class RedundantNullCheckTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.RedundantNullCheck>();
        private readonly VerifierBuilder codeFixbuilderCS = new VerifierBuilder<CS.RedundantNullCheck>().WithCodeFix<CS.RedundantNullCheckCodeFix>();

        [TestMethod]
        public void RedundantNullCheck_CS() =>
            builderCS.AddPaths("RedundantNullCheck.cs").Verify();

        [TestMethod]
        public void RedundantNullCheck_CS_CodeFix() =>
            codeFixbuilderCS.AddPaths("RedundantNullCheck.cs")
                .WithCodeFixedPaths("RedundantNullCheck.Fixed.cs", "RedundantNullCheck.Fixed.Batch.cs")
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void RedundantNullCheck_CSharp9() =>
            builderCS.AddPaths("RedundantNullCheck.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void RedundantNullCheck_CSharp10() =>
            builderCS.AddPaths("RedundantNullCheck.CSharp10.cs")
                .WithTopLevelStatements()
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

        [TestMethod]
        public void RedundantNullCheck_CSharp11() =>
            builderCS.AddPaths("RedundantNullCheck.CSharp11.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .VerifyNoIssues();

        [TestMethod]
        public void RedundantNullCheck_CSharp9_CodeFix() =>
            codeFixbuilderCS.AddPaths("RedundantNullCheck.CSharp9.cs")
                .WithCodeFixedPaths("RedundantNullCheck.CSharp9.Fixed.cs")
                .WithTopLevelStatements()
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantNullCheck_CSharp10_CodeFix() =>
            codeFixbuilderCS.AddPaths("RedundantNullCheck.CSharp10.cs")
                .WithCodeFixedPaths("RedundantNullCheck.CSharp10.Fixed.cs")
                .WithTopLevelStatements()
                .WithOptions(LanguageOptions.FromCSharp10)
                .VerifyCodeFix();

#endif

        [TestMethod]
        public void RedundantNullCheck_VB() =>
            new VerifierBuilder<VB.RedundantNullCheck>().AddPaths("RedundantNullCheck.vb").Verify();
    }
}
