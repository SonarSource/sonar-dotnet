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
    public class RedundancyInConstructorDestructorDeclarationTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<RedundancyInConstructorDestructorDeclaration>();
        private readonly VerifierBuilder codeFixBuilderRemoveBaseCall = new VerifierBuilder<RedundancyInConstructorDestructorDeclaration>()
            .WithCodeFix<RedundancyInConstructorDestructorDeclarationCodeFix>()
            .WithCodeFixTitle(RedundancyInConstructorDestructorDeclarationCodeFix.TitleRemoveBaseCall);
        private readonly VerifierBuilder codeFixBuilderRemoveConstructor = new VerifierBuilder<RedundancyInConstructorDestructorDeclaration>()
            .WithCodeFix<RedundancyInConstructorDestructorDeclarationCodeFix>()
            .WithCodeFixTitle(RedundancyInConstructorDestructorDeclarationCodeFix.TitleRemoveConstructor);

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration() =>
            builder.AddPaths("RedundancyInConstructorDestructorDeclaration.cs").Verify();

#if NET

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CSharp9() =>
            builder.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp9.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .Verify();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CSharp10() =>
            builder.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp10.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CSharp11() =>
            builder.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp11.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CSharp12() =>
            builder.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp12.cs")
                .WithOptions(LanguageOptions.FromCSharp12)
                .Verify();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_CSharp9() =>
            codeFixBuilderRemoveBaseCall.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp9.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.CSharp9.Fixed.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_CSharp10() =>
            codeFixBuilderRemoveConstructor.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp10.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.CSharp10.Fixed.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_CSharp11() =>
            codeFixBuilderRemoveConstructor.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp11.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.CSharp11.Fixed.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .VerifyCodeFix();

#endif

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_BaseCall() =>
            codeFixBuilderRemoveBaseCall.AddPaths("RedundancyInConstructorDestructorDeclaration.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.BaseCall.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_Constructor() =>
            codeFixBuilderRemoveConstructor.AddPaths("RedundancyInConstructorDestructorDeclaration.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.Constructor.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_Destructor() =>
            new VerifierBuilder<RedundancyInConstructorDestructorDeclaration>()
                .WithCodeFix<RedundancyInConstructorDestructorDeclarationCodeFix>()
                .AddPaths("RedundancyInConstructorDestructorDeclaration.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.Destructor.Fixed.cs")
                .WithCodeFixTitle(RedundancyInConstructorDestructorDeclarationCodeFix.TitleRemoveDestructor)
                .VerifyCodeFix();
    }
}
