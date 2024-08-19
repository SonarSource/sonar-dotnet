/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Rules.CSharp;

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
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CSharp10() =>
            builder.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CSharp11() =>
            builder.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CSharp12() =>
            builder.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp12.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp12)
                .Verify();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_CSharp9() =>
            codeFixBuilderRemoveBaseCall.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp9.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.CSharp9.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_CSharp10() =>
            codeFixBuilderRemoveConstructor.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp10.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.CSharp10.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_CSharp11() =>
            codeFixBuilderRemoveConstructor.AddPaths("RedundancyInConstructorDestructorDeclaration.CSharp11.cs")
                .WithCodeFixedPaths("RedundancyInConstructorDestructorDeclaration.CSharp11.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
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
