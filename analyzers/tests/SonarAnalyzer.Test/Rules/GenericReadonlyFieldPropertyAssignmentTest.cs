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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class GenericReadonlyFieldPropertyAssignmentTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<GenericReadonlyFieldPropertyAssignment>();
    private readonly VerifierBuilder codeFix = new VerifierBuilder<GenericReadonlyFieldPropertyAssignment>().WithCodeFix<GenericReadonlyFieldPropertyAssignmentCodeFix>();

    [TestMethod]
    public void GenericReadonlyFieldPropertyAssignment() =>
        builder.AddPaths("GenericReadonlyFieldPropertyAssignment.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

    [TestMethod]
    public void GenericReadonlyFieldPropertyAssignment_CSharpLatest() =>
        builder.AddPaths("GenericReadonlyFieldPropertyAssignment.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void GenericReadonlyFieldPropertyAssignment_CSharpLatest_CodeFix_Remove_Statement() =>
        codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.Latest.cs")
            .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.Latest.Remove.Fixed.cs")
            .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleRemove)
            .WithOptions(LanguageOptions.CSharpLatest)
            .VerifyCodeFix();

    [TestMethod]
    public void GenericReadonlyFieldPropertyAssignment_CodeFix_Remove_Statement() =>
        codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.cs")
            .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.Remove.Fixed.cs")
            .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleRemove)
            .VerifyCodeFix();

    [TestMethod]
    public void GenericReadonlyFieldPropertyAssignment_CodeFix_Add_Generic_Type_Constraint() =>
        codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.cs")
            .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.AddConstraint.Fixed.cs")
            .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleAddClassConstraint)
            .VerifyCodeFix();
}
