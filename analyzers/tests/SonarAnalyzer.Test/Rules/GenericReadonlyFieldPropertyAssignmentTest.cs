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
    public class GenericReadonlyFieldPropertyAssignmentTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<GenericReadonlyFieldPropertyAssignment>();
        private readonly VerifierBuilder codeFix = new VerifierBuilder<GenericReadonlyFieldPropertyAssignment>().WithCodeFix<GenericReadonlyFieldPropertyAssignmentCodeFix>();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment() =>
            builder.AddPaths("GenericReadonlyFieldPropertyAssignment.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CSharp9() =>
            builder.AddPaths("GenericReadonlyFieldPropertyAssignment.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CSharp10() =>
             builder.AddPaths("GenericReadonlyFieldPropertyAssignment.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CSharp10_CodeFix_Remove_Statement() =>
            codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.CSharp10.cs")
                .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.CSharp10.Remove.Fixed.cs")
                .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleRemove)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .VerifyCodeFix();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CSharp11() =>
             builder.AddPaths("GenericReadonlyFieldPropertyAssignment.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

#endif

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CodeFix_Remove_Statement() =>
            codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.cs")
                .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.Remove.Fixed.cs")
                .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleRemove)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .VerifyCodeFix();

        [TestMethod]
        public void GenericReadonlyFieldPropertyAssignment_CodeFix_Add_Generic_Type_Constraint() =>
            codeFix.AddPaths("GenericReadonlyFieldPropertyAssignment.cs")
                .WithCodeFixedPaths("GenericReadonlyFieldPropertyAssignment.AddConstraint.Fixed.cs")
                .WithCodeFixTitle(GenericReadonlyFieldPropertyAssignmentCodeFix.TitleAddClassConstraint)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .VerifyCodeFix();
    }
}
