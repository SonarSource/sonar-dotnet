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
    public class ImplementISerializableCorrectlyTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<ImplementISerializableCorrectly>();

        [TestMethod]
        public void ImplementISerializableCorrectly() =>
            builder.AddPaths("ImplementISerializableCorrectly.cs").Verify();

#if NET

        [TestMethod]
        public void ImplementISerializableCorrectly_FromCSharp9() =>
            builder.AddPaths("ImplementISerializableCorrectly.CSharp9.Part1.cs", "ImplementISerializableCorrectly.CSharp9.Part2.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void ImplementISerializableCorrectly_FromCSharp10() =>
            builder.AddPaths("ImplementISerializableCorrectly.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void ImplementISerializableCorrectly_FromCSharp12() =>
            builder.AddPaths("ImplementISerializableCorrectly.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).Verify();

#endif

    }
}
