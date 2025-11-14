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
    public class GenericReadonlyFieldPropertyAssignmentTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<GenericReadonlyFieldPropertyAssignment>();
        private readonly VerifierBuilder codeFix = new VerifierBuilder<GenericReadonlyFieldPropertyAssignment>().WithCodeFix<GenericReadonlyFieldPropertyAssignmentCodeFix>();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment() =>
            builder.AddPaths("GenericReadonlyFieldPropertyAssignment.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CSharp9() =>
            builder.AddPaths("GenericReadonlyFieldPropertyAssignment.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CSharp10() =>
             builder.AddPaths("GenericReadonlyFieldPropertyAssignment.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CSharp10_CodeFix_Remove_Statement() =>
            codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.CSharp10.cs")
                .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.CSharp10.Remove.Fixed.cs")
                .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleRemove)
                .WithOptions(LanguageOptions.FromCSharp10)
                .VerifyCodeFix();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CSharp11() =>
             builder.AddPaths("GenericReadonlyFieldPropertyAssignment.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

#endif

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CodeFix_Remove_Statement() =>
            codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.cs")
                .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.Remove.Fixed.cs")
                .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleRemove)
                .WithOptions(LanguageOptions.FromCSharp8)
                .VerifyCodeFix();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CodeFix_Add_Generic_Type_Constraint() =>
            codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.cs")
                .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.AddConstraint.Fixed.cs")
                .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleAddClassConstraint)
                .WithOptions(LanguageOptions.FromCSharp8)
                .VerifyCodeFix();
    }
}
