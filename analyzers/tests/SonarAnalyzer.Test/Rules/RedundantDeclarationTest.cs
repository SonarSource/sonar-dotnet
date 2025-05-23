﻿/*
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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class RedundantDeclarationTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<RedundantDeclaration>();
    private readonly VerifierBuilder codeFixBuilder = new VerifierBuilder<RedundantDeclaration>().WithCodeFix<RedundantDeclarationCodeFix>();

    [TestMethod]
    public void RedundantDeclaration() =>
        builder.AddPaths("RedundantDeclaration.cs")
            .WithOptions(ParseOptionsHelper.BeforeCSharp10)
            .Verify();

    [TestMethod]
    public void RedundantDeclaration_UnusedLambdaParameters_BeforeCSharp9() =>
        builder.AddSnippet(@"using System; public class C { public void M() { Action<int, int> a = (p1, p2) => { }; /* Compliant - Lambda discard parameters have been introduced in C# 9 */ } }")
            .WithOptions(ParseOptionsHelper.BeforeCSharp9)
            .VerifyNoIssues();

#if NET

    [TestMethod]
    public void RedundantDeclaration_CSharp9() =>
        builder.AddPaths("RedundantDeclaration.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void RedundantDeclaration_CSharp9_CodeFix_TitleRedundantParameterName() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.CSharp9.cs")
            .WithCodeFixedPaths("RedundantDeclaration.CSharp9.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantParameterName)
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CSharp10() =>
        builder.AddPaths("RedundantDeclaration.CSharp10.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .Verify();

    [TestMethod]
    public void RedundantDeclaration_CSharp10_CodeFix_ExplicitDelegate() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.CSharp10.cs")
            .WithCodeFixedPaths("RedundantDeclaration.CSharp10.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantExplicitDelegate)
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CSharp12() =>
        builder.AddPaths("RedundantDeclaration.CSharp12.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .Verify();

    [TestMethod]
    public void RedundantDeclaration_CSharp12_CodeFix_ArraySize() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.CSharp12.cs")
            .WithCodeFix<RedundantDeclarationCodeFix>()
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantArraySize)
            .WithCodeFixedPaths("RedundantDeclaration.CSharp12.ArraySize.Fixed.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CSharp12_CodeFix_LambdaParameterType() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.CSharp12.cs")
            .WithCodeFix<RedundantDeclarationCodeFix>()
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantLambdaParameterType)
            .WithCodeFixedPaths("RedundantDeclaration.CSharp12.LambdaParameterType.Fixed.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .VerifyCodeFix();

#endif

    [TestMethod]
    public void RedundantDeclaration_CodeFix_ArraySize() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.cs")
            .WithCodeFixedPaths("RedundantDeclaration.ArraySize.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantArraySize)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CodeFix_ArrayType() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.cs")
            .WithCodeFixedPaths("RedundantDeclaration.ArrayType.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantArrayType)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CodeFix_DelegateParameterList() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.cs")
            .WithCodeFixedPaths("RedundantDeclaration.DelegateParameterList.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantDelegateParameterList)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CodeFix_ExplicitDelegate() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.cs")
            .WithCodeFixedPaths("RedundantDeclaration.ExplicitDelegate.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantExplicitDelegate)
            .WithOptions(ParseOptionsHelper.BeforeCSharp10)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CodeFix_ExplicitNullable() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.cs")
            .WithCodeFixedPaths("RedundantDeclaration.ExplicitNullable.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantExplicitNullable)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CodeFix_LambdaParameterType() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.cs")
            .WithCodeFixedPaths("RedundantDeclaration.LambdaParameterType.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantLambdaParameterType)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CodeFix_ObjectInitializer() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.cs")
            .WithCodeFixedPaths("RedundantDeclaration.ObjectInitializer.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantObjectInitializer)
            .VerifyCodeFix();
}
