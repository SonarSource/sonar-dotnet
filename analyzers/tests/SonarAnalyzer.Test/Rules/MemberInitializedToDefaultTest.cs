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
    public class MemberInitializedToDefaultTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MemberInitializedToDefault>();

        [TestMethod]
        public void MemberInitializedToDefault() =>
            builder.AddPaths("MemberInitializedToDefault.cs").Verify();

        [TestMethod]
        public void MemberInitializedToDefault_CodeFix() =>
            builder
            .WithCodeFix<MemberInitializedToDefaultCodeFix>()
            .AddPaths("MemberInitializedToDefault.cs")
            .WithCodeFixedPaths("MemberInitializedToDefault.Fixed.cs")
            .VerifyCodeFix();

#if NET

        [TestMethod]
        public void MemberInitializedToDefault_CSharp8() =>
            builder.AddPaths("MemberInitializedToDefault.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8)
                .VerifyNoIssues();  // FN, rule does not raise in nullable context, despite a lack of a bang operator

        [TestMethod]
        public void MemberInitializedToDefault_CSharp9() =>
            builder.AddPaths("MemberInitializedToDefault.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void MemberInitializedToDefault_CSharp10() =>
            builder.AddPaths("MemberInitializedToDefault.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void MemberInitializedToDefault_CSharp11() =>
            builder.AddPaths("MemberInitializedToDefault.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

        [TestMethod]
        public void MemberInitializedToDefault_CSharp11_CodeFix() =>
            builder
            .WithCodeFix<MemberInitializedToDefaultCodeFix>()
            .AddPaths("MemberInitializedToDefault.CSharp11.cs")
            .WithCodeFixedPaths("MemberInitializedToDefault.CSharp11.Fixed.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .VerifyCodeFix();

#endif

    }
}
