/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2019 SonarSource SA
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
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MethodsShouldNotHaveTooManyLinesTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_DefaultValues_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_DefaultValues.cs",
                new CS.MethodsShouldNotHaveTooManyLines());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_CustomValues_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.cs",
                new CS.MethodsShouldNotHaveTooManyLines { Max = 2 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_DoesntReportInTest_CS()
        {
            Verifier.VerifyNoIssueReportedInTest(@"TestCases\MethodsShouldNotHaveTooManyLines_DefaultValues.cs",
                new CS.MethodsShouldNotHaveTooManyLines());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_InvalidSyntax_CS()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class Foo
{
    public string ()
    {
        return ""f"";
    }
}",
                new CS.MethodsShouldNotHaveTooManyLines { Max = 2 },
                checkMode: CompilationErrorBehavior.Ignore);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(0)]
        [DataRow(-1)]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_InvalidMaxThreshold_CS(int max)
        {
            Action action = () => Verifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.cs",
                new CS.MethodsShouldNotHaveTooManyLines { Max = max });

            action.Should().Throw<AssertFailedException>()
                .WithMessage("*error AD0001: * 'SonarAnalyzer.Rules.CSharp.MethodsShouldNotHaveTooManyLines' *System.InvalidOperationException* 'Invalid rule parameter: maximum number of lines = *. Must be at least 2.'.}.");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_DefaultValues_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_DefaultValues.vb",
                new VB.MethodsShouldNotHaveTooManyLines());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_CustomValues_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.vb",
                new VB.MethodsShouldNotHaveTooManyLines { Max = 2 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_DoesntReportInTest_VB()
        {
            Verifier.VerifyNoIssueReportedInTest(@"TestCases\MethodsShouldNotHaveTooManyLines_DefaultValues.vb",
                new VB.MethodsShouldNotHaveTooManyLines());
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(0)]
        [DataRow(-1)]
        [TestCategory("Rule")]
        public void MethodsShouldNotHaveTooManyLines_InvalidMaxThreshold_VB(int max)
        {
            Action action = () => Verifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.vb",
                new VB.MethodsShouldNotHaveTooManyLines { Max = max });

            action.Should().Throw<AssertFailedException>()
                .WithMessage("*error AD0001: * 'SonarAnalyzer.Rules.VisualBasic.MethodsShouldNotHaveTooManyLines' *System.InvalidOperationException* 'Invalid rule parameter: maximum number of lines = *. Must be at least 2.'.}.");
        }
    }
}

