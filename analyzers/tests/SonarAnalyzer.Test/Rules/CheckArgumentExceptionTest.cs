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
    public class CheckArgumentExceptionTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CheckArgumentException>();

        [TestMethod]
        public void CheckArgumentException() =>
            builder.AddPaths("CheckArgumentException.cs").Verify();

        [TestMethod]
        public void CheckArgumentException_TopLevelStatements() =>
            builder.AddPaths("CheckArgumentException.TopLevelStatements.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void CheckArgumentException_CSharp9() =>
            builder.AddPaths("CheckArgumentException.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void CheckArgumentException_CSharp10() =>
            builder.AddPaths("CheckArgumentException.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void CheckArgumentException_CSharp12() =>
            builder.AddPaths("CheckArgumentException.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).Verify();
    }
}
