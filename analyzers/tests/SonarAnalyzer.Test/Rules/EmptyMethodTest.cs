/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class EmptyMethodTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.EmptyMethod>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.EmptyMethod>();

    [TestMethod]
    public void EmptyMethod() =>
        builderCS.AddPaths("EmptyMethod.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .AddReferences(MetadataReferenceFacade.NetStandard21)
            .Verify();

#if NET

    [TestMethod]
    public void EmptyMethod_CSharp9() =>
        builderCS.AddPaths("EmptyMethod.CSharp9.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void EmptyMethod_CSharp9_CodeFix_Throw() =>
        builderCS.WithCodeFix<CS.EmptyMethodCodeFix>()
            .AddPaths("EmptyMethod.CSharp9.cs")
            .WithTopLevelStatements()
            .WithCodeFixedPaths("EmptyMethod.CSharp9.Throw.Fixed.cs")
            .WithCodeFixTitle(CS.EmptyMethodCodeFix.TitleThrow)
            .VerifyCodeFix();

    [TestMethod]
    public void EmptyMethod_CSharp9_CodeFix_Comment() =>
        builderCS.WithCodeFix<CS.EmptyMethodCodeFix>()
            .AddPaths("EmptyMethod.CSharp9.cs")
            .WithTopLevelStatements()
            .WithCodeFixedPaths("EmptyMethod.CSharp9.Comment.Fixed.cs")
            .WithCodeFixTitle(CS.EmptyMethodCodeFix.TitleComment)
            .VerifyCodeFix();

    [TestMethod]
    public void EmptyMethod_CS_Latest() =>
        builderCS.AddPaths("EmptyMethod.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

    [TestMethod]
    public void EmptyMethod_CodeFix_Throw() =>
        builderCS.WithCodeFix<CS.EmptyMethodCodeFix>()
            .AddPaths("EmptyMethod.cs")
            .WithCodeFixedPaths("EmptyMethod.Throw.Fixed.cs")
            .WithCodeFixTitle(CS.EmptyMethodCodeFix.TitleThrow)
            .VerifyCodeFix();

    [TestMethod]
    public void EmptyMethod_CodeFix_Comment() =>
        builderCS.WithCodeFix<CS.EmptyMethodCodeFix>()
            .AddPaths("EmptyMethod.cs")
            .WithCodeFixedPaths("EmptyMethod.Comment.Fixed.cs")
            .WithCodeFixTitle(CS.EmptyMethodCodeFix.TitleComment)
            .VerifyCodeFix();

    [TestMethod]
    public void EmptyMethod_WithoutClosingBracket_CodeFix_Comment() =>
        builderCS.WithCodeFix<CS.EmptyMethodCodeFix>()
            .AddPaths("EmptyMethod.WithoutClosingBracket.cs")
            .WithCodeFixedPaths("EmptyMethod.WithoutClosingBracket.Comment.Fixed.cs")
            .WithCodeFixTitle(CS.EmptyMethodCodeFix.TitleComment)
            .VerifyCodeFix();

    [TestMethod]
    public void EmptyMethod_VB() =>
        builderVB.AddPaths("EmptyMethod.vb").Verify();

    [TestMethod]
    public void EmptyMethod_WithVirtualOverride_RaisesIssueForMainProject_CS() =>
        builderCS.AddPaths("EmptyMethod.OverrideVirtual.cs").Verify();

    [TestMethod]
    public void EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_CS() =>
        builderCS.AddPaths("EmptyMethod.OverrideVirtual.cs").AddTestReference().VerifyNoIssues();

    [TestMethod]
    public void EmptyMethod_WithVirtualOverride_RaisesIssueForMainProject_VB() =>
        builderVB.AddPaths("EmptyMethod.OverrideVirtual.vb").Verify();

    [TestMethod]
    public void EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_VB() =>
        builderVB.AddPaths("EmptyMethod.OverrideVirtual.vb").AddTestReference().VerifyNoIssues();
}
