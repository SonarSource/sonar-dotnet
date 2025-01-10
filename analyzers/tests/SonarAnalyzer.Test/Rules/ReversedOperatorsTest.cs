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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ReversedOperatorsTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ReversedOperators>();

        [TestMethod]
        public void ReversedOperators_CS() =>
            builderCS.AddPaths("ReversedOperators.cs").Verify();

#if NET

        [TestMethod]
        public void ReversedOperators_CSharp9() =>
            builderCS.AddPaths("ReversedOperators.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

#endif

        [TestMethod]
        public void ReversedOperators_VB() =>
            new VerifierBuilder<VB.ReversedOperators>().AddPaths("ReversedOperators.vb").Verify();
    }
}
