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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MemberOverrideCallsBaseMemberTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MemberOverrideCallsBaseMember>();

        [TestMethod]
        public void MemberOverrideCallsBaseMember() =>
            builder.AddPaths("MemberOverrideCallsBaseMember.cs").Verify();

#if NET
        [TestMethod]
        public void MemberOverrideCallsBaseMember_Latest() =>
            builder.AddPaths("MemberOverrideCallsBaseMember.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

        [TestMethod]
        public void MemberOverrideCallsBaseMember_Latest_CodeFix() =>
            builder.AddPaths("MemberOverrideCallsBaseMember.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithCodeFix<MemberOverrideCallsBaseMemberCodeFix>()
            .WithCodeFixedPaths("MemberOverrideCallsBaseMember.Latest.Fixed.cs")
            .VerifyCodeFix();
#endif

        [TestMethod]
        public void MemberOverrideCallsBaseMember_CodeFix() =>
            builder.AddPaths("MemberOverrideCallsBaseMember.cs")
            .WithCodeFix<MemberOverrideCallsBaseMemberCodeFix>()
            .WithCodeFixedPaths("MemberOverrideCallsBaseMember.Fixed.cs")
            .VerifyCodeFix();

        [TestMethod]
        public void MemberOverrideCallsBaseMember_ToString()
        {
            var toString = "public override string ToString() => base.ToString();";
            toString +=
#if NET
                "// Noncompliant {{Remove this method 'ToString' to simply inherit its behavior.}}";
#elif NETFRAMEWORK
                "// FN. ToString has a [__DynamicallyInvokable] attribute in .Net framework";
#endif
            builder.AddSnippet($@"
                class Test
                {{
                    {toString}
                }}
                ")
#if NET
                .Verify();
#elif NETFRAMEWORK
                .VerifyNoIssues();
#endif
        }
    }
}
