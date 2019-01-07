/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundancyInConstructorDestructorDeclarationTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void RedundancyInConstructorDestructorDeclaration()
        {
            Verifier.VerifyAnalyzer(@"TestCases\RedundancyInConstructorDestructorDeclaration.cs",
                new RedundancyInConstructorDestructorDeclaration());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_BaseCall()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\RedundancyInConstructorDestructorDeclaration.cs",
                @"TestCases\RedundancyInConstructorDestructorDeclaration.BaseCall.Fixed.cs",
                new RedundancyInConstructorDestructorDeclaration(),
                new RedundancyInConstructorDestructorDeclarationCodeFixProvider(),
                RedundancyInConstructorDestructorDeclarationCodeFixProvider.TitleRemoveBaseCall);
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_Constructor()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\RedundancyInConstructorDestructorDeclaration.cs",
                @"TestCases\RedundancyInConstructorDestructorDeclaration.Constructor.Fixed.cs",
                new RedundancyInConstructorDestructorDeclaration(),
                new RedundancyInConstructorDestructorDeclarationCodeFixProvider(),
                RedundancyInConstructorDestructorDeclarationCodeFixProvider.TitleRemoveConstructor);
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void RedundancyInConstructorDestructorDeclaration_CodeFix_Destructor()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\RedundancyInConstructorDestructorDeclaration.cs",
                @"TestCases\RedundancyInConstructorDestructorDeclaration.Destructor.Fixed.cs",
                new RedundancyInConstructorDestructorDeclaration(),
                new RedundancyInConstructorDestructorDeclarationCodeFixProvider(),
                RedundancyInConstructorDestructorDeclarationCodeFixProvider.TitleRemoveDestructor);
        }
    }
}
