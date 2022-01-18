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

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundantConditionalAroundAssignmentTest
    {
        [TestMethod]
        public void RedundantConditionalAroundAssignment() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\RedundantConditionalAroundAssignment.cs", new RedundantConditionalAroundAssignment());

#if NET
        [TestMethod]
        public void RedundantConditionalAroundAssignment_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\RedundantConditionalAroundAssignment.CSharp9.cs", new RedundantConditionalAroundAssignment());

        [TestMethod]
        public void RedundantConditionalAroundAssignment_CSharp9_CodeFix() =>
            OldVerifier.VerifyCodeFix<RedundantConditionalAroundAssignmentCodeFixProvider>(
                @"TestCases\RedundantConditionalAroundAssignment.CSharp9.cs",
                @"TestCases\RedundantConditionalAroundAssignment.CSharp9.Fixed.cs",
                new RedundantConditionalAroundAssignment(),
                ParseOptionsHelper.FromCSharp9,
                OutputKind.ConsoleApplication);

        [TestMethod]
        public void RedundantConditionalAroundAssignment_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Console(@"TestCases\RedundantConditionalAroundAssignment.CSharp10.cs", new RedundantConditionalAroundAssignment());

        [TestMethod]
        public void RedundantConditionalAroundAssignment_CSharp10_CodeFix() =>
            OldVerifier.VerifyCodeFix<RedundantConditionalAroundAssignmentCodeFixProvider>(
                @"TestCases\RedundantConditionalAroundAssignment.CSharp10.cs",
                @"TestCases\RedundantConditionalAroundAssignment.CSharp10.Fixed.cs",
                new RedundantConditionalAroundAssignment(),
                ParseOptionsHelper.FromCSharp10,
                OutputKind.ConsoleApplication);
#endif

        [TestMethod]
        public void RedundantConditionalAroundAssignment_CodeFix() =>
            OldVerifier.VerifyCodeFix<RedundantConditionalAroundAssignmentCodeFixProvider>(
                @"TestCases\RedundantConditionalAroundAssignment.cs",
                @"TestCases\RedundantConditionalAroundAssignment.Fixed.cs",
                new RedundantConditionalAroundAssignment());
    }
}
