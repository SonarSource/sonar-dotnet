/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundantDeclarationTest
    {
        [TestMethod]
        public void RedundantDeclaration() =>
            Verifier.VerifyAnalyzer(@"TestCases\RedundantDeclaration.cs", new RedundantDeclaration());

        [TestMethod]
        public void RedundantDeclaration_UnusedLambdaParameters_BeforeCSharp9() =>
            Verifier.VerifyCSharpAnalyzer(
                @"using System; public class C { public void M() { Action<int, int> a = (p1, p2) => { }; /* Compliant - Lambda discard parameters have been introduced in C# 9 */ } }",
                new RedundantDeclaration(),
                ParseOptionsHelper.BeforeCSharp9);

#if NET
        [TestMethod]
        public void RedundantDeclaration_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\RedundantDeclaration.CSharp9.cs", new RedundantDeclaration());

        [TestMethod]
        public void RedundantDeclaration_CSharp9_CodeFix_TitleRedundantParameterName() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantDeclaration.CSharp9.cs",
                                   @"TestCases\RedundantDeclaration.CSharp9.Fixed.cs",
                                   new RedundantDeclaration(),
                                   new RedundantDeclarationCodeFixProvider(),
                                   RedundantDeclarationCodeFixProvider.TitleRedundantParameterName,
                                   ParseOptionsHelper.FromCSharp9);
#endif

        [TestMethod]
        public void RedundantDeclaration_CodeFix_ArraySize() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantDeclaration.cs",
                                   @"TestCases\RedundantDeclaration.ArraySize.Fixed.cs",
                                   new RedundantDeclaration(),
                                   new RedundantDeclarationCodeFixProvider(),
                                   RedundantDeclarationCodeFixProvider.TitleRedundantArraySize);

        [TestMethod]
        public void RedundantDeclaration_CodeFix_ArrayType() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantDeclaration.cs",
                                   @"TestCases\RedundantDeclaration.ArrayType.Fixed.cs",
                                   new RedundantDeclaration(),
                                   new RedundantDeclarationCodeFixProvider(),
                                   RedundantDeclarationCodeFixProvider.TitleRedundantArrayType);

        [TestMethod]
        public void RedundantDeclaration_CodeFix_DelegateParameterList() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantDeclaration.cs",
                                   @"TestCases\RedundantDeclaration.DelegateParameterList.Fixed.cs",
                                   new RedundantDeclaration(),
                                   new RedundantDeclarationCodeFixProvider(),
                                   RedundantDeclarationCodeFixProvider.TitleRedundantDelegateParameterList);

        [TestMethod]
        public void RedundantDeclaration_CodeFix_ExplicitDelegate() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantDeclaration.cs",
                                   @"TestCases\RedundantDeclaration.ExplicitDelegate.Fixed.cs",
                                   new RedundantDeclaration(),
                                   new RedundantDeclarationCodeFixProvider(),
                                   RedundantDeclarationCodeFixProvider.TitleRedundantExplicitDelegate);

        [TestMethod]
        public void RedundantDeclaration_CodeFix_ExplicitNullable() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantDeclaration.cs",
                                   @"TestCases\RedundantDeclaration.ExplicitNullable.Fixed.cs",
                                   new RedundantDeclaration(),
                                   new RedundantDeclarationCodeFixProvider(),
                                   RedundantDeclarationCodeFixProvider.TitleRedundantExplicitNullable);

        [TestMethod]
        public void RedundantDeclaration_CodeFix_LambdaParameterType() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantDeclaration.cs",
                                   @"TestCases\RedundantDeclaration.LambdaParameterType.Fixed.cs",
                                   new RedundantDeclaration(),
                                   new RedundantDeclarationCodeFixProvider(),
                                   RedundantDeclarationCodeFixProvider.TitleRedundantLambdaParameterType);

        [TestMethod]
        public void RedundantDeclaration_CodeFix_ObjectInitializer() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantDeclaration.cs",
                                   @"TestCases\RedundantDeclaration.ObjectInitializer.Fixed.cs",
                                   new RedundantDeclaration(),
                                   new RedundantDeclarationCodeFixProvider(),
                                   RedundantDeclarationCodeFixProvider.TitleRedundantObjectInitializer);
    }
}
