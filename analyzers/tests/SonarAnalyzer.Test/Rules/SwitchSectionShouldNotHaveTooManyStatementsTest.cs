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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class SwitchSectionShouldNotHaveTooManyStatementsTest
    {
        [TestMethod]
        public void SwitchSectionShouldNotHaveTooManyStatements_DefaultValue_CS() =>
            new VerifierBuilder<CS.SwitchSectionShouldNotHaveTooManyStatements>().AddPaths("SwitchSectionShouldNotHaveTooManyStatements_DefaultValue.cs").Verify();

        [TestMethod]
        public void SwitchSectionShouldNotHaveTooManyStatements_CustomValue_CSharp8() =>
            new VerifierBuilder().AddAnalyzer(() => new CS.SwitchSectionShouldNotHaveTooManyStatements { Threshold = 1 })
                .AddPaths("SwitchSectionShouldNotHaveTooManyStatements_CustomValue.cs")
                .WithOptions(LanguageOptions.FromCSharp8)
                .Verify();

        [TestMethod]
        public void SwitchSectionShouldNotHaveTooManyStatements_DefaultValue_VB() =>
            new VerifierBuilder<VB.SwitchSectionShouldNotHaveTooManyStatements>().AddPaths("SwitchSectionShouldNotHaveTooManyStatements_DefaultValue.vb").Verify();

        [TestMethod]
        public void SwitchSectionShouldNotHaveTooManyStatements_CustomValue_VB() =>
            new VerifierBuilder().AddAnalyzer(() => new VB.SwitchSectionShouldNotHaveTooManyStatements { Threshold = 1 })
                .AddPaths("SwitchSectionShouldNotHaveTooManyStatements_CustomValue.vb")
                .Verify();
    }
}
