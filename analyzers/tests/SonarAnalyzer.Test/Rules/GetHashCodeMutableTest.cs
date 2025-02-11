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
    public class GetHashCodeMutableTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<GetHashCodeMutable>();

        [TestMethod]
        public void GetHashCodeMutable() =>
            builder.AddPaths("GetHashCodeMutable.cs").Verify();

#if NET

        [TestMethod]
        public void GetHashCodeMutable_CSharp10() =>
            builder.AddPaths("GetHashCodeMutable.CSharp10.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

#endif

        [TestMethod]
        public void GetHashCodeMutable_CodeFix() =>
            builder.WithCodeFix<GetHashCodeMutableCodeFix>()
                .AddPaths("GetHashCodeMutable.cs")
                .WithCodeFixedPaths("GetHashCodeMutable.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void GetHashCodeMutable_InvalidCode() =>
            builder.AddSnippet("""
                class
                {
                    int i;                              
                    public override int GetHashCode()   // Noncompliant
                    {
                        return i;                       // Secondary
                    }
                }
                """)
                .WithErrorBehavior(CompilationErrorBehavior.Ignore)
                .Verify();
    }
}
