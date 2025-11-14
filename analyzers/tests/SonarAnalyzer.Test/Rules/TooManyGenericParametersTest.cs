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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class TooManyGenericParametersTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<TooManyGenericParameters>();

        [TestMethod]
        public void TooManyGenericParameters_DefaultValues() =>
            builder.AddPaths("TooManyGenericParameters.DefaultValues.cs").Verify();

        [TestMethod]
        public void TooManyGenericParameters_CustomValues() =>
            new VerifierBuilder()
            .AddAnalyzer(() => new TooManyGenericParameters { MaxNumberOfGenericParametersInClass = 4, MaxNumberOfGenericParametersInMethod = 4 })
            .AddPaths("TooManyGenericParameters.CustomValues.cs")
            .Verify();

#if NET

        [TestMethod]
        public void TooManyGenericParameters_CSharp9() =>
            builder.AddPaths("TooManyGenericParameters.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void TooManyGenericParameters_CSharp10() =>
            builder.AddPaths("TooManyGenericParameters.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void TooManyGenericParameters_CSharp11() =>
            builder.AddPaths("TooManyGenericParameters.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

        [TestMethod]
        public void TooManyGenericParameters_CSharp12() =>
            builder.AddPaths("TooManyGenericParameters.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).Verify();

#endif

    }
}
