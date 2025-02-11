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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class SwitchCaseFallsThroughToDefaultTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<SwitchCaseFallsThroughToDefault>();

        [TestMethod]
        public void SwitchCaseFallsThroughToDefault() =>
            builder.AddPaths("SwitchCaseFallsThroughToDefault.cs").Verify();

#if NET

        [TestMethod]
        public void SwitchCaseFallsThroughToDefault_CSharp9() =>
            builder.AddPaths("SwitchCaseFallsThroughToDefault.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

#endif

        [TestMethod]
        public void SwitchCaseFallsThroughToDefault_CodeFix() =>
            builder.AddPaths("SwitchCaseFallsThroughToDefault.cs")
                .WithCodeFix<SwitchCaseFallsThroughToDefaultCodeFix>()
                .WithCodeFixedPaths("SwitchCaseFallsThroughToDefault.Fixed.cs")
                .VerifyCodeFix();
    }
}
