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

using SonarAnalyzer.CSharp.Rules;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class UnaryPrefixOperatorRepeatedTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UnaryPrefixOperatorRepeated>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UnaryPrefixOperatorRepeated>();

        [TestMethod]
        public void UnaryPrefixOperatorRepeated() =>
            builderCS.AddPaths("UnaryPrefixOperatorRepeated.cs").Verify();

#if NET

        [TestMethod]
        public void UnaryPrefixOperatorRepeated_CS_TopLevelStatements() =>
            builderCS.AddPaths("UnaryPrefixOperatorRepeated.TopLevelStatements.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void UnaryPrefixOperatorRepeated_CS_Latest() =>
            builderCS.AddPaths("UnaryPrefixOperatorRepeated.Latest.cs")
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();

#endif

        [TestMethod]
        public void UnaryPrefixOperatorRepeated_CodeFix() =>
            builderCS.WithCodeFix<UnaryPrefixOperatorRepeatedCodeFix>()
                .AddPaths("UnaryPrefixOperatorRepeated.cs")
                .WithCodeFixedPaths("UnaryPrefixOperatorRepeated.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void UnaryPrefixOperatorRepeated_VB() =>
            builderVB.AddPaths("UnaryPrefixOperatorRepeated.vb").Verify();
    }
}
