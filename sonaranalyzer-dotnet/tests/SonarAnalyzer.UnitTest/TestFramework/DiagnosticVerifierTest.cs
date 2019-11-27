extern alias csharp;
using System;
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

using csharp::SonarAnalyzer.Rules.CSharp;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    [TestClass]
    public class DiagnosticVerifierTest
    {
        [TestMethod]
        public void PrimaryIssueNotExpected()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedSecondary
    {
        public void Test(bool a, bool b)
        {
            // Secondary@+1
            if (a == a)
            { }
        }
    }",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().Throw<UnexpectedDiagnosticException>()
                  .WithMessage("Primary issue with message 'Correct one of the identical expressions on both sides of operator '=='.' not expected on line 7");
        }

        [TestMethod]
        public void SecondaryIssueNotExpected()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedSecondary
    {
        public void Test(bool a, bool b)
        {
            if (a == a) // Noncompliant
            { }
        }
    }",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().Throw<UnexpectedDiagnosticException>()
                  .WithMessage("Secondary issue with message '' not expected on line 6");
        }

        [TestMethod]
        public void UnexpectedSecondaryIssueWrongId()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedSecondary
    {
        public void Test(bool a, bool b)
        {
            // Secondary@+1 [myWrongId]
            if (a == a) // Noncompliant [myId]
            { }
        }
    }",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().Throw<UnexpectedDiagnosticException>()
                  .WithMessage("Secondary issue [myId] with message '' not expected on line 7");
        }

        [TestMethod]
        public void SecondaryIssueUnexpectedMessage()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedSecondary
    {
        public void Test(bool a, bool b)
        {
            // Secondary@+1 {{Wrong message}}
            if (a == a) // Noncompliant
            { }
        }
    }",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().Throw<UnexpectedDiagnosticException>()
                  .WithMessage("Expected secondary message on line 7 to be 'Wrong message', but got ''.");
        }

        [TestMethod]
        public void SecondaryIssueUnexpectedStartPosition()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedSecondary
    {
        public void Test(bool a, bool b)
        {
            if (a == a)
//                   ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
//            ^ Secondary@-1
            { }
        }
    }",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().Throw<UnexpectedDiagnosticException>()
                  .WithMessage("Expected secondary issue on line 6 to start on column 14 but got column 16.");
        }

        [TestMethod]
        public void SecondaryIssueUnexpectedLength()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedSecondary
    {
        public void Test(bool a, bool b)
        {
            if (a == a)
//                   ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
//              ^^^^ Secondary@-1
            { }
        }
    }",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().Throw<UnexpectedDiagnosticException>()
                  .WithMessage("Expected secondary issue on line 6 to have a length of 4 but got a length of 1.");
        }

        [TestMethod]
        public void ValidVerification()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedSecondary
    {
        public void Test(bool a, bool b)
        {
            // Secondary@+1
            if (a == a) // Noncompliant
            { }
        }
    }",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().NotThrow<UnexpectedDiagnosticException>();
        }

        [TestMethod]
        public void BuildError()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedSecondaryWithBuildError

        public void Test(bool a, bool b) { }
}",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().Throw<UnexpectedDiagnosticException>()
                  .WithMessage("Unexpected build error [CS1514]: { expected on line 2");
        }

        [TestMethod]
        public void UnexpectedRemainingCurlyBrace()
        {
            Action action =
                () => Verifier.VerifyCSharpAnalyzer(@"
public class UnexpectedRemainingCurlyBrace
    {
        public void Test(bool a, bool b)
        {
            if (a == a) // Noncompliant {Wrong format message}
            { }
        }
    }",
                    new BinaryOperationWithIdenticalExpressions());

            action.Should().Throw<AssertFailedException>()
                  .WithMessage("Unexpected '{' found on line: 5. Either correctly use the '{{message}}' format or remove the curly brace on the line of the expected issue");
        }
    }
}
