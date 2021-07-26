/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MarkAssemblyWithComVisibleAttributeTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void MarkAssemblyWithComVisibleAttribute_CS() =>
            Verifier.VerifyConcurrentAnalyzerNoDuplication(new[] { @"TestCases\MarkAssemblyWithComVisibleAttribute.cs", @"TestCases\MarkAssemblyWithComVisibleAttribute2.cs", },
                                                           new CS.MarkAssemblyWithComVisibleAttribute());

        [TestMethod]
        [TestCategory("Rule")]
        public void MarkAssemblyWithComVisibleAttribute_VB() =>
            Verifier.VerifyConcurrentAnalyzerNoDuplication(new[] { @"TestCases\MarkAssemblyWithComVisibleAttribute.vb", @"TestCases\MarkAssemblyWithComVisibleAttribute2.vb", },
                                                           new VB.MarkAssemblyWithComVisibleAttribute());

        [TestMethod]
        [TestCategory("Rule")]
        public void MarkAssemblyWithComVisibleAttributeNoncompliant_CS()
        {
            Action action = () => Verifier.VerifyAnalyzer(@"TestCases\MarkAssemblyWithComVisibleAttributeNoncompliant.cs", new CS.MarkAssemblyWithComVisibleAttribute());
            action.Should().Throw<UnexpectedDiagnosticException>().WithMessage("*Provide a 'ComVisible' attribute for assembly 'project0'.*");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MarkAssemblyWithComVisibleAttributeNoncompliant_VB()
        {
            Action action = () => Verifier.VerifyAnalyzer(@"TestCases\MarkAssemblyWithComVisibleAttributeNoncompliant.vb", new VB.MarkAssemblyWithComVisibleAttribute());
            action.Should().Throw<UnexpectedDiagnosticException>().WithMessage("*Provide a 'ComVisible' attribute for assembly 'project0'.*");
        }

    }
}
