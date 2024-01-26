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

using System.Text;
using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Test.Verification.IssueValidation;

[TestClass]
public class IssueLocationPairTest
{
    private static readonly IssueLocation Actual = new() { RuleId = "S1234", LineNumber = 42, IsPrimary = true, Message = "Lorem ipsum", Start = 42, Length = 10 };

    [TestMethod]
    public void AppendAssertionMessage_DifferentKeys()
    {
        var sut = new IssueLocationPair(new IssueLocation() { FilePath = "SomeFile.cs" }, new IssueLocation() { FilePath = "AnotherFile.cs" });
        sut.Invoking(x => x.AppendAssertionMessage(new())).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void AppendAssertionMessage_PerfectMatch() =>
        AssertionMessage(Actual, Actual).Should().BeEmpty();

    [TestMethod]
    public void AppendAssertionMessage_MissingIssue_NoMessage() =>
        AssertionMessage(null, new() { LineNumber = 42, IsPrimary = true }).Should().Be("  Line 42: Missing expected issue");

    [TestMethod]
    public void AppendAssertionMessage_MissingIssue_WithExpectedMessage() =>
        AssertionMessage(null, Actual).Should().Be("  Line 42: Missing issue 'Lorem ipsum'");

    [TestMethod]
    public void AppendAssertionMessage_UnexpectedIssue() =>
        AssertionMessage(Actual, null).Should().Be("  Line 42: Unexpected issue 'Lorem ipsum' Rule S1234");

    [TestMethod]
    public void AppendAssertionMessage_SecondaryText() =>
        AssertionMessage(new IssueLocation { RuleId = "S1234", LineNumber = 42, Message = "Lorem ipsum" }, null).Should().Be("  Line 42 Secondary location: Unexpected issue 'Lorem ipsum' Rule S1234");

    [TestMethod]
    public void AppendAssertionMessage_WrongMessage()
    {
        var expected = new IssueLocation { LineNumber = 42, IsPrimary = true, Message = "Dolor sit", Start = 2, Length = 2, IssueId = "Flag1" };
        AssertionMessage(Actual, expected).Should().Be("  Line 42: The expected message 'Dolor sit' does not match the actual message 'Lorem ipsum' Rule S1234 ID Flag1");
    }

    [TestMethod]
    public void AppendAssertionMessage_WrongStartLocation()
    {
        var expected = new IssueLocation { LineNumber = 42, IsPrimary = true, Message = "Lorem ipsum", Start = 2, Length = 2, IssueId = "Flag1" };
        AssertionMessage(Actual, expected).Should().Be("  Line 42: Should start on column 2 but got column 42 Rule S1234 ID Flag1");
    }

    [TestMethod]
    public void AppendAssertionMessage_WrongLength()
    {
        var expected = new IssueLocation { LineNumber = 42, IsPrimary = true, Message = "Lorem ipsum", Start = 42, Length = 2, IssueId = "Flag1" };
        AssertionMessage(Actual, expected).Should().Be("  Line 42: Should have a length of 2 but got a length of 10 Rule S1234 ID Flag1");
    }

    [TestMethod]
    public void AppendAssertionMessage_NoExpectationForMessageAndLocation()
    {
        var expected = new IssueLocation { LineNumber = 42, IsPrimary = true };
        AssertionMessage(Actual, expected).Should().BeEmpty();
    }

    private static string AssertionMessage(IssueLocation actual, IssueLocation expected)
    {
        var builder = new StringBuilder();
        new IssueLocationPair(actual, expected).AppendAssertionMessage(builder);
        var line = builder.ToString();
        if (line.Length == 0)
        {
            return line;
        }
        else
        {
            line.Should().EndWith(Environment.NewLine);
            return line.Substring(0, line.Length - Environment.NewLine.Length);
        }
    }
}
