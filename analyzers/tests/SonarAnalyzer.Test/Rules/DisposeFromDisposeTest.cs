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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DisposeFromDisposeTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<DisposeFromDispose>();

        [TestMethod]
        public void DisposeFromDispose_CSharp7_2() =>
            // Readonly structs have been introduced in C# 7.2.
            // In C# 8, readonly structs can be disposed of, and the behavior is different.
            new VerifierBuilder<DisposeFromDispose>()
                .AddPaths("DisposeFromDispose.CSharp7_2.cs")
                .WithLanguageVersion(LanguageVersion.CSharp7_2)
                .Verify();

        [TestMethod]
        public void DisposeFromDispose_CSharp8() =>
            builder.AddPaths("DisposeFromDispose.CSharp8.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void DisposeFromDispose_CSharp9() =>
            builder.AddPaths("DisposeFromDispose.CSharp9.Part1.cs", "DisposeFromDispose.CSharp9.Part2.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void DisposeFromDispose_CSharp10() =>
            builder.AddPaths("DisposeFromDispose.CSharp10.cs")
                .WithTopLevelStatements()
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

#endif

    }
}
