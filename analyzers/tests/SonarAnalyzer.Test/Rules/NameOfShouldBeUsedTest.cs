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
using RoslynCS = Microsoft.CodeAnalysis.CSharp;
using RoslynVB = Microsoft.CodeAnalysis.VisualBasic;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class NameOfShouldBeUsedTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.NameOfShouldBeUsed>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.NameOfShouldBeUsed>();

        [TestMethod]
        public void NameOfShouldBeUsed_CSharp6() =>
            builderCS.AddPaths("NameOfShouldBeUsed.cs").WithOptions(LanguageOptions.FromCSharp6).Verify();

        [TestMethod]
        public void NameOfShouldBeUsed_CSharp5() =>
            builderCS.AddPaths("NameOfShouldBeUsed.cs")
            .WithLanguageVersion(RoslynCS.LanguageVersion.CSharp5)
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssues();

#if NET

        [TestMethod]
        public void NameOfShouldBeUsed_CSharp11() =>
            builderCS.AddPaths("NameOfShouldBeUsed.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

#endif

        [TestMethod]
        public void NameOfShouldBeUsed_FromVB14() =>
            builderVB.AddPaths("NameOfShouldBeUsed.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();

        [TestMethod]
        public void NameOfShouldBeUsed_VB12() =>
            builderVB.AddPaths("NameOfShouldBeUsed.vb")
            .WithLanguageVersion(RoslynVB.LanguageVersion.VisualBasic12)
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssues();
    }
}
