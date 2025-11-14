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
    public class EmptyNamespaceTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<EmptyNamespace>();

        [TestMethod]
        public void EmptyNamespace() =>
            builder.AddPaths("EmptyNamespace.cs").Verify();

#if NET

        [TestMethod]
        public void EmptyNamespace_CSharp10() =>
            builder.AddPaths("EmptyNamespace.CSharp10.Empty.cs", "EmptyNamespace.CSharp10.NotEmpty.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .WithConcurrentAnalysis(false)
                .Verify();

        [TestMethod]
        public void EmptyNamespace_CSharp10_CodeFix() =>
            builder.AddPaths("EmptyNamespace.CSharp10.Empty.cs")
                .WithCodeFix<EmptyNamespaceCodeFix>()
                .WithOptions(LanguageOptions.FromCSharp10)
                .WithAutogenerateConcurrentFiles(false)
                .WithCodeFixedPaths("EmptyNamespace.CSharp10.Fixed.cs")
                .VerifyCodeFix();

#endif

        [TestMethod]
        public void EmptyNamespace_CodeFix() =>
            builder.AddPaths("EmptyNamespace.cs")
                .WithCodeFix<EmptyNamespaceCodeFix>()
                .WithCodeFixedPaths("EmptyNamespace.Fixed.cs", "EmptyNamespace.Fixed.Batch.cs")
                .VerifyCodeFix();
    }
}
