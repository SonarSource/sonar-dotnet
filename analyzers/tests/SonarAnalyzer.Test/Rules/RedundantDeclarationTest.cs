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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class RedundantDeclarationTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<RedundantDeclaration>();
    private readonly VerifierBuilder codeFixBuilder = new VerifierBuilder<RedundantDeclaration>().WithCodeFix<RedundantDeclarationCodeFix>();

    [TestMethod]
    public void RedundantDeclaration() =>
        builder.AddPaths("RedundantDeclaration.cs")
            .WithOptions(LanguageOptions.BeforeCSharp10)
            .Verify();

    [TestMethod]
    public void RedundantDeclaration_UnusedLambdaParameters_BeforeCSharp9() =>
        builder.AddSnippet(@"using System; public class C { public void M() { Action<int, int> a = (p1, p2) => { }; /* Compliant - Lambda discard parameters have been introduced in C# 9 */ } }")
            .WithOptions(LanguageOptions.BeforeCSharp9)
            .VerifyNoIssues();

#if NET

    [TestMethod]
    public void RedundantDeclaration_CSharp9() =>
        builder.AddPaths("RedundantDeclaration.CSharp9.cs")
            .WithTopLevelStatements()
            .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9)
            .Verify();

    [TestMethod]
    public void RedundantDeclaration_CSharp9_CodeFix_TitleRedundantParameterName() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.CSharp9.cs")
            .WithCodeFixedPaths("RedundantDeclaration.CSharp9.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantParameterName)
            .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CSharp10() =>
        builder.AddPaths("RedundantDeclaration.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

    [TestMethod]
    public void RedundantDeclaration_CSharp10_CodeFix_ExplicitDelegate() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.CSharp10.cs")
            .WithCodeFixedPaths("RedundantDeclaration.CSharp10.Fixed.cs")
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantExplicitDelegate)
            .WithOptions(LanguageOptions.FromCSharp10)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CSharp12() =>
        builder.AddPaths("RedundantDeclaration.CSharp12.cs")
            .WithOptions(LanguageOptions.FromCSharp12)
            .Verify();

    [TestMethod]
    public void RedundantDeclaration_CSharp12_CodeFix_ArraySize() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.CSharp12.cs")
            .WithCodeFix<RedundantDeclarationCodeFix>()
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantArraySize)
            .WithCodeFixedPaths("RedundantDeclaration.CSharp12.ArraySize.Fixed.cs")
            .WithOptions(LanguageOptions.FromCSharp12)
            .VerifyCodeFix();

    [TestMethod]
    public void RedundantDeclaration_CSharp12_CodeFix_LambdaParameterType() =>
        codeFixBuilder.AddPaths("RedundantDeclaration.CSharp12.cs")
            .WithCodeFix<RedundantDeclarationCodeFix>()
            .WithCodeFixTitle(RedundantDeclarationCodeFix.TitleRedundantLambdaParameterType)
            .WithCodeFixedPaths("RedundantDeclaration.CSharp12.LambdaParameterType.Fixed.cs")
            .WithOptions(LanguageOptions.FromCSharp12)
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
            .WithOptions(LanguageOptions.BeforeCSharp10)
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
