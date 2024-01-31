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
using SonarAnalyzer.Test.Helpers;

namespace SonarAnalyzer.Test.TestFramework.Tests
{
    [TestClass]
    public class DiagnosticVerifierTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<BinaryOperationWithIdenticalExpressions>();

        [TestMethod]
        public void PrimaryIssueNotExpected() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedSecondary
                {
                    public void Test(bool a, bool b)
                    {
                        // Secondary@+1
                        if (a == a)
                        { }
                    }
                }
                """, """
                There are differences for CSharp7 snippet1.cs:
                  Line 6: Unexpected issue 'Correct one of the identical expressions on both sides of operator '=='.' Rule S1764
                """);

        [TestMethod]
        public void SecondaryIssueNotExpected() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedSecondary
                {
                    public void Test(bool a, bool b)
                    {
                        if (a == a) // Noncompliant
                        { }
                    }
                }
                """, """
                There are differences for CSharp7 snippet1.cs:
                  Line 5 Secondary location: Unexpected issue ''
                """);

        [TestMethod]
        [Ignore] // ToDo: Fix in
        public void UnexpectedSecondaryIssueWrongId() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedSecondary
                {
                    public void Test(bool a, bool b)
                    {
                        // Secondary@+1 [myWrongId]
                        if (a == a) // Noncompliant [myId]
                        { }
                    }
                }
                """,
                "CSharp*: Unexpected secondary issue [myId] on line 7, span (6,12)-(6,13) with message ''." + Environment.NewLine +
                "See output to see all actual diagnostics raised on the file");

        [TestMethod]
        public void SecondaryIssueUnexpectedMessage() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedSecondary
                {
                    public void Test(bool a, bool b)
                    {
                        // Secondary@+1 {{Wrong message}}
                        if (a == a) // Noncompliant
                        { }
                    }
                }
                """, """
                There are differences for CSharp7 snippet1.cs:
                  Line 6 Secondary location: The expected message 'Wrong message' does not match the actual message ''
                """);

        [TestMethod]
        public void SecondaryIssueUnexpectedStartPosition() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedSecondary
                {
                    public void Test(bool a, bool b)
                    {
                        if (a == a)
                //               ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
                //        ^ Secondary@-1
                        { }
                    }
                }
                """, """
                There are differences for CSharp7 snippet1.cs:
                  Line 5 Secondary location: Should start on column 10 but got column 12
                """);

        [TestMethod]
        public void SecondaryIssueUnexpectedLength() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedSecondary
                {
                    public void Test(bool a, bool b)
                    {
                        if (a == a)
                //               ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
                //          ^^^^ Secondary@-1
                        { }
                    }
                }
                """, """
                There are differences for CSharp7 snippet1.cs:
                  Line 5 Secondary location: Should have a length of 4 but got a length of 1
                """);

        [TestMethod]
        public void ValidVerification() =>
            builder.AddSnippet("""
                public class UnexpectedSecondary
                {
                    public void Test(bool a, bool b)
                    {
                        // Secondary@+1
                        if (a == a) // Noncompliant
                        { }
                    }
                }
                """).Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void BuildError() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedBuildError
                {
                """, """
                There are differences for CSharp7 snippet1.cs:
                  Line 2: Unexpected issue '} expected' Rule CS1513
                """);

        [TestMethod]
        public void UnexpectedRemainingOpeningCurlyBrace() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedRemainingCurlyBrace
                {
                    public void Test(bool a, bool b)
                    {
                        if (a == a) // Noncompliant {Wrong format message}
                        { }
                    }
                }
                """,
                "Unexpected '{' or '}' found on line: 4. Either correctly use the '{{message}}' format or remove the curly braces on the line of the expected issue");

        [TestMethod]
        public void UnexpectedRemainingClosingCurlyBrace() =>
            VerifyThrows<AssertFailedException>("""
                public class UnexpectedRemainingCurlyBrace
                {
                    public void Test(bool a, bool b)
                    {
                        if (a == a) // Noncompliant (Another Wrong format message}
                        { }
                    }
                }
                """,
                "Unexpected '{' or '}' found on line: 4. Either correctly use the '{{message}}' format or remove the curly braces on the line of the expected issue");

        [TestMethod]
        public void ExpectedIssuesNotRaised() =>
            VerifyThrows<AssertFailedException>("""
                public class ExpectedIssuesNotRaised
                {
                    public void Test(bool a, bool b) // Noncompliant [MyId0]
                    {
                        if (a == b) // Noncompliant
                        { } // Secondary [MyId1]
                    }
                }
                """, """
                There are differences for CSharp7 snippet1.cs:
                  Line 3: Missing expected issue ID MyId0
                  Line 5: Missing expected issue
                  Line 6 Secondary location: Missing expected issue ID MyId1
                """);

        [TestMethod]
        public void ExpectedIssuesNotRaised_MultipleFiles() =>
            builder.WithBasePath("DiagnosticsVerifier")
                .AddPaths("ExpectedIssuesNotRaised.cs", "ExpectedIssuesNotRaised2.cs")
                .WithConcurrentAnalysis(false)
                .Invoking(x => x.Verify())
                .Should().Throw<AssertFailedException>().WithMessage("""
                    There are differences for CSharp7 DiagnosticsVerifier\ExpectedIssuesNotRaised.cs:
                      Line 3: Missing expected issue ID MyId0
                      Line 5: Missing expected issue
                      Line 6 Secondary location: Missing expected issue ID MyId1

                    There are differences for CSharp7 DiagnosticsVerifier\ExpectedIssuesNotRaised2.cs:
                      Line 3: Missing expected issue ID MyId0
                      Line 5: Missing expected issue
                      Line 6 Secondary location: Missing expected issue ID MyId1
                    """);

        private void VerifyThrows<TException>(string snippet, string expectedMessage) where TException : Exception =>
            builder.AddSnippet(snippet)
                .WithConcurrentAnalysis(false)
                .Invoking(x => x.Verify())
                .Should().Throw<TException>().Which.Message.Should().ContainIgnoringLineEndings(expectedMessage);
    }
}
