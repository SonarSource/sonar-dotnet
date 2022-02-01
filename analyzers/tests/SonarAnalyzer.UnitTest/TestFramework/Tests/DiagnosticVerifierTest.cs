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

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    [TestClass]
    public class DiagnosticVerifierTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<BinaryOperationWithIdenticalExpressions>();

        [TestMethod]
        public void PrimaryIssueNotExpected() =>
            VerifyThrows<UnexpectedDiagnosticException>(@"
public class UnexpectedSecondary
{
    public void Test(bool a, bool b)
    {
        // Secondary@+1
        if (a == a)
        { }
    }
}",
                "CSharp*: Unexpected primary issue on line 7, span (6,17)-(6,18) with message 'Correct one of the identical expressions on both sides of operator '=='.'." + Environment.NewLine +
                "See output to see all actual diagnostics raised on the file");

        [TestMethod]
        public void SecondaryIssueNotExpected() =>
            VerifyThrows<UnexpectedDiagnosticException>(@"
public class UnexpectedSecondary
{
    public void Test(bool a, bool b)
    {
        if (a == a) // Noncompliant
        { }
    }
}",
                "CSharp*: Unexpected secondary issue on line 6, span (5,12)-(5,13) with message ''." + Environment.NewLine +
                "See output to see all actual diagnostics raised on the file");

        [TestMethod]
        public void UnexpectedSecondaryIssueWrongId() =>
            VerifyThrows<UnexpectedDiagnosticException>(@"
public class UnexpectedSecondary
{
    public void Test(bool a, bool b)
    {
        // Secondary@+1 [myWrongId]
        if (a == a) // Noncompliant [myId]
        { }
    }
}",
                "CSharp*: Unexpected secondary issue [myId] on line 7, span (6,12)-(6,13) with message ''." + Environment.NewLine +
                "See output to see all actual diagnostics raised on the file");

        [TestMethod]
        public void SecondaryIssueUnexpectedMessage() =>
            VerifyThrows<UnexpectedDiagnosticException>(@"
public class UnexpectedSecondary
{
    public void Test(bool a, bool b)
    {
        // Secondary@+1 {{Wrong message}}
        if (a == a) // Noncompliant
        { }
    }
}",
                @"CSharp*: Expected secondary message on line 7 does not match actual message." + Environment.NewLine +
                "Expected: 'Wrong message'" + Environment.NewLine +
                "Actual  : ''");

        [TestMethod]
        public void SecondaryIssueUnexpectedStartPosition() =>
            VerifyThrows<UnexpectedDiagnosticException>(@"
public class UnexpectedSecondary
{
    public void Test(bool a, bool b)
    {
        if (a == a)
//               ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
//        ^ Secondary@-1
        { }
    }
}",
                "CSharp*: Expected secondary issue on line 6 to start on column 10 but got column 12.");

        [TestMethod]
        public void SecondaryIssueUnexpectedLength() =>
            VerifyThrows<UnexpectedDiagnosticException>(@"
public class UnexpectedSecondary
{
    public void Test(bool a, bool b)
    {
        if (a == a)
//               ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
//          ^^^^ Secondary@-1
        { }
    }
}",
                "CSharp*: Expected secondary issue on line 6 to have a length of 4 but got a length of 1.");

        [TestMethod]
        public void ValidVerification() =>
            builder.AddSnippet(@"
public class UnexpectedSecondary
{
    public void Test(bool a, bool b)
    {
        // Secondary@+1
        if (a == a) // Noncompliant
        { }
    }
}").Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void BuildError() =>
            VerifyThrows<UnexpectedDiagnosticException>(@"
public class UnexpectedBuildError
{",
                "CSharp*: Unexpected build error [CS1513]: } expected on line 3");

        [TestMethod]
        public void UnexpectedRemainingOpeningCurlyBrace() =>
            VerifyThrows<AssertFailedException>(@"
public class UnexpectedRemainingCurlyBrace
{
    public void Test(bool a, bool b)
    {
        if (a == a) // Noncompliant {Wrong format message}
        { }
    }
}",
                "Unexpected '{' or '}' found on line: 5. Either correctly use the '{{message}}' format or remove the curly braces on the line of the expected issue");

        [TestMethod]
        public void UnexpectedRemainingClosingCurlyBrace() =>
            VerifyThrows<AssertFailedException>(@"
public class UnexpectedRemainingCurlyBrace
{
    public void Test(bool a, bool b)
    {
        if (a == a) // Noncompliant (Another Wrong format message}
        { }
    }
}",
                "Unexpected '{' or '}' found on line: 5. Either correctly use the '{{message}}' format or remove the curly braces on the line of the expected issue");

        [TestMethod]
        public void ExpectedIssuesNotRaised() =>
            VerifyThrows<AssertFailedException>(@"
public class ExpectedIssuesNotRaised
{
    public void Test(bool a, bool b) // Noncompliant [MyId0]
    {
        if (a == b) // Noncompliant
        { } // Secondary [MyId1]
    }
}",
                @"CSharp*: Issue(s) expected but not raised in file(s):" + Environment.NewLine +
                "File: snippet1.cs" + Environment.NewLine +
                "Line: 4, Type: primary, Id: 'MyId0'" + Environment.NewLine +
                "Line: 6, Type: primary, Id: ''" + Environment.NewLine +
                "Line: 7, Type: secondary, Id: 'MyId1'");

        [TestMethod]
        public void ExpectedIssuesNotRaised_MultipleFiles() =>
            builder.WithBasePath("DiagnosticsVerifier")
                .AddPaths("ExpectedIssuesNotRaised.cs", "ExpectedIssuesNotRaised2.cs")
                .WithConcurrentAnalysis(false)
                .Invoking(x => x.Verify())
                .Should().Throw<AssertFailedException>().WithMessage(
                    @"CSharp*: Issue(s) expected but not raised in file(s):" + Environment.NewLine +
                    "File: ExpectedIssuesNotRaised.cs" + Environment.NewLine +
                    "Line: 3, Type: primary, Id: 'MyId0'" + Environment.NewLine +
                    "Line: 5, Type: primary, Id: ''" + Environment.NewLine +
                    "Line: 6, Type: secondary, Id: 'MyId1'" + Environment.NewLine +
                    Environment.NewLine +
                    "File: ExpectedIssuesNotRaised2.cs" + Environment.NewLine +
                    "Line: 3, Type: primary, Id: 'MyId0'" + Environment.NewLine +
                    "Line: 5, Type: primary, Id: ''" + Environment.NewLine +
                    "Line: 6, Type: secondary, Id: 'MyId1'");

        private void VerifyThrows<TException>(string snippet, string expectedMessage) where TException : Exception =>
            builder.AddSnippet(snippet)
                .WithConcurrentAnalysis(false)
                .Invoking(x => x.Verify())
                .Should().Throw<TException>().WithMessage(expectedMessage);
    }
}
