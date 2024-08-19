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
    public class RedundantInheritanceListTest
    {
        private readonly VerifierBuilder rule = new VerifierBuilder<RedundantInheritanceList>();
        private readonly VerifierBuilder codeFix = new VerifierBuilder<RedundantInheritanceList>().WithCodeFix<RedundantInheritanceListCodeFix>();

        [TestMethod]
        public void RedundantInheritanceList() =>
            rule.AddPaths("RedundantInheritanceList.cs").Verify();

#if NET

        [TestMethod]
        public void RedundantInheritanceList_CSharp9() =>
            rule.AddPaths("RedundantInheritanceList.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void RedundantInheritanceList_CSharp9_CodeFix() =>
            codeFix.AddPaths("RedundantInheritanceList.CSharp9.cs")
                .WithCodeFixedPaths("RedundantInheritanceList.CSharp9.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantInheritanceList_CSharp10() =>
            rule.AddPaths("RedundantInheritanceList.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void RedundantInheritanceList_CSharp10_CodeFix() =>
            codeFix.AddPaths("RedundantInheritanceList.CSharp10.cs")
                .WithCodeFixedPaths("RedundantInheritanceList.CSharp10.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .VerifyCodeFix();

#endif

        [TestMethod]
        public void RedundantInheritanceList_CodeFix() =>
            codeFix.AddPaths("RedundantInheritanceList.cs")
                .WithCodeFixedPaths("RedundantInheritanceList.Fixed.cs")
                .VerifyCodeFix();
    }
}
