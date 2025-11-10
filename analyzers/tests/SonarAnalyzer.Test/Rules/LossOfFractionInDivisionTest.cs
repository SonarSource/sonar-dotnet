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
    public class LossOfFractionInDivisionTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<LossOfFractionInDivision>();

        [TestMethod]
        public void LossOfFractionInDivision() =>
            builder.AddPaths("LossOfFractionInDivision.cs").Verify();

#if NET

        [TestMethod]
        public void LossOfFractionInDivision_FromCSharp9() =>
            builder.AddPaths("LossOfFractionInDivision.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void LossOfFractionInDivision_FromCSharp10() =>
            builder.AddPaths("LossOfFractionInDivision.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void LossOfFractionInDivision_FromCSharp11() =>
            builder.AddPaths("LossOfFractionInDivision.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

#endif

    }
}
