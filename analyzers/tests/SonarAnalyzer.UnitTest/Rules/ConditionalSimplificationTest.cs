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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ConditionalSimplificationTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<ConditionalSimplification>();
        private readonly VerifierBuilder codeFix = new VerifierBuilder<ConditionalSimplification>().WithCodeFix<ConditionalSimplificationCodeFix>();

        [TestMethod]
        public void ConditionalSimplification_BeforeCSharp8() =>
            builder.AddPaths("ConditionalSimplification.BeforeCSharp8.cs").WithOptions(ParseOptionsHelper.BeforeCSharp8).Verify();

        [TestMethod]
        public void ConditionalSimplification_CSharp8() =>
            builder.AddPaths("ConditionalSimplification.CSharp8.cs").WithLanguageVersion(LanguageVersion.CSharp8).Verify();

        [TestMethod]
        public void ConditionalSimplification_CSharp8_CodeFix() =>
            codeFix.AddPaths("ConditionalSimplification.CSharp8.cs").WithLanguageVersion(LanguageVersion.CSharp8).WithCodeFixedPaths("ConditionalSimplification.CSharp8.Fixed.cs").VerifyCodeFix();

        [TestMethod]
        public void ConditionalSimplification_FromCSharp8() =>
            builder.AddPaths("ConditionalSimplification.FromCSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [TestMethod]
        public void ConditionalSimplification_BeforeCSharp8_CodeFix() =>
            codeFix.AddPaths("ConditionalSimplification.BeforeCSharp8.cs")
                .WithCodeFixedPaths("ConditionalSimplification.BeforeCSharp8.Fixed.cs")
                .WithOptions(ParseOptionsHelper.BeforeCSharp8).VerifyCodeFix();

        [TestMethod]
        public void ConditionalSimplification_FromCSharp8_CodeFix() =>
            codeFix.AddPaths("ConditionalSimplification.FromCSharp8.cs")
                .WithCodeFixedPaths("ConditionalSimplification.FromCSharp8.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void ConditionalSimplification_FromCSharp9() =>
            builder.AddPaths("ConditionalSimplification.FromCSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void ConditionalSimplification_FromCSharp10() =>
            builder.AddPaths("ConditionalSimplification.FromCSharp10.cs").WithTopLevelStatements().WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void ConditionalSimplification_FromCSharp9_CodeFix() =>
            codeFix.AddPaths("ConditionalSimplification.FromCSharp9.cs")
                .WithCodeFixedPaths("ConditionalSimplification.FromCSharp9.Fixed.cs")
                .WithTopLevelStatements()
                .VerifyCodeFix();

        [TestMethod]
        public void ConditionalSimplification_FromCSharp10_CodeFix() =>
            codeFix.AddPaths("ConditionalSimplification.FromCSharp10.cs")
                .WithCodeFixedPaths("ConditionalSimplification.FromCSharp10.Fixed.cs")
                .WithTopLevelStatements()
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .VerifyCodeFix();

#endif
    }
}
