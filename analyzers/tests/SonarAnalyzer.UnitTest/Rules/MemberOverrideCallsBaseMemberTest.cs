/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MemberOverrideCallsBaseMemberTest
    {
        private readonly VerifierBuilder verifier = new VerifierBuilder<MemberOverrideCallsBaseMember>();

        [TestMethod]
        public void MemberOverrideCallsBaseMember() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\MemberOverrideCallsBaseMember.cs", new MemberOverrideCallsBaseMember());

#if NET
        [TestMethod]
        public void MemberOverrideCallsBaseMember_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\MemberOverrideCallsBaseMember.CSharp9.cs", new MemberOverrideCallsBaseMember());

        [TestMethod]
        public void MemberOverrideCallsBaseMember_CSharp9_CodeFix() =>
            OldVerifier.VerifyCodeFix<MemberOverrideCallsBaseMemberCodeFix>(
                @"TestCases\MemberOverrideCallsBaseMember.CSharp9.cs",
                @"TestCases\MemberOverrideCallsBaseMember.CSharp9.Fixed.cs",
                new MemberOverrideCallsBaseMember(),
                ParseOptionsHelper.FromCSharp9);
#endif

        [TestMethod]
        public void MemberOverrideCallsBaseMember_CodeFix() =>
            OldVerifier.VerifyCodeFix<MemberOverrideCallsBaseMemberCodeFix>(
                @"TestCases\MemberOverrideCallsBaseMember.cs",
                @"TestCases\MemberOverrideCallsBaseMember.Fixed.cs",
                new MemberOverrideCallsBaseMember());
    }
}
