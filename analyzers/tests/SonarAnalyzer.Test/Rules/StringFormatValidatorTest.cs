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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class StringFormatValidatorTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<StringFormatValidator>();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatValidator_RuntimeExceptionFree(ProjectType projectType) =>
            builder.AddPaths("StringFormatValidator.RuntimeExceptionFree.cs")
                .AddReferences(TestCompiler.ProjectTypeReference(projectType))
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatValidator_TypoFree(ProjectType projectType) =>
            builder.AddPaths("StringFormatValidator.TypoFree.cs")
                .AddReferences(TestCompiler.ProjectTypeReference(projectType))
                .Verify();

        [TestMethod]
        public void StringFormatValidator_EdgeCases() =>
            builder.AddPaths("StringFormatValidator.EdgeCases.cs").Verify();

#if NET

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatValidator_RuntimeExceptionFree_CSharp11(ProjectType projectType) =>
            builder.AddPaths("StringFormatValidator.RuntimeExceptionFree.CSharp11.cs")
                .AddReferences(TestCompiler.ProjectTypeReference(projectType))
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatValidator_TypoFree_CSharp11(ProjectType projectType) =>
            builder.AddPaths("StringFormatValidator.TypoFree.CSharp11.cs")
                .AddReferences(TestCompiler.ProjectTypeReference(projectType))
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

        [TestMethod]
        public void StringFormatValidator_Latest() =>
            builder.AddPaths("StringFormatValidator.Latest.cs")
                .AddReferences(TestCompiler.ProjectTypeReference(ProjectType.Product))
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();

#endif

    }
}
