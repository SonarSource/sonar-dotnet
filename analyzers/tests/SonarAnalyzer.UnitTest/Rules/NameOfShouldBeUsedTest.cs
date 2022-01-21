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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using RoslynCS = Microsoft.CodeAnalysis.CSharp;
using RoslynVB = Microsoft.CodeAnalysis.VisualBasic;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class NameOfShouldBeUsedTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.NameOfShouldBeUsed>().AddPaths("NameOfShouldBeUsed.cs");
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.NameOfShouldBeUsed>().AddPaths("NameOfShouldBeUsed.vb");

        [TestMethod]
        public void NameOfShouldBeUsed_FromCSharp6() =>
            builderCS.WithOptions(ParseOptionsHelper.FromCSharp6).Verify();

        [TestMethod]
        public void NameOfShouldBeUsed_CSharp5() =>
            builderCS.WithLanguageVersion(RoslynCS.LanguageVersion.CSharp5).WithErrorBehavior(CompilationErrorBehavior.Ignore).VerifyNoIssueReported();

        [TestMethod]
        public void NameOfShouldBeUsed_FromVB14() =>
            builderVB.WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();

        [TestMethod]
        public void NameOfShouldBeUsed_VB12() =>
            builderVB.WithLanguageVersion(RoslynVB.LanguageVersion.VisualBasic12).WithErrorBehavior(CompilationErrorBehavior.Ignore).VerifyNoIssueReported();
    }
}
