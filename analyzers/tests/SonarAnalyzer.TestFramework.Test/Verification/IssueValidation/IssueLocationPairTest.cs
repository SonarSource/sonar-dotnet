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

using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Test.Verification.IssueValidation;

[TestClass]
public class IssueLocationPairTest
{
    private static readonly IssueLocation ActualPrimary = new(IssueType.Primary, "File.cs", 42, "Lorem ipsum", null, 42, 10, "S1234");
    private static readonly IssueLocation ActualSecondary = new(IssueType.Secondary, "File.cs", 42, "Lorem ipsum", "Flag1", 42, 10, "S1234");

    [TestMethod]
    public void CreateMessage_DifferentKeys() =>
        new IssueLocationPair(new(IssueType.Primary, "SomeFile.cs", 1, "Message", null, null, null), new(IssueType.Primary, "AnotherFile.cs", 1, "Message", null, null, null))
            .Invoking(x => x.CreateMessage())
            .Should()
            .Throw<InvalidOperationException>();

    [TestMethod]
    public void CreateMessage_PerfectMatch() =>
        new IssueLocationPair(ActualPrimary, ActualPrimary).Invoking(x => x.CreateMessage()).Should().Throw<InvalidOperationException>();

    [TestMethod]
    public void CreateMessage_MissingIssue_NoMessage() =>
        ValidateMessage(null, new(IssueType.Primary, "File.cs", 42, null, null, null, null), "Primary Missing", "  Line 42: Missing expected issue");

    [TestMethod]
    public void CreateMessage_MissingIssue_WithExpectedMessage() =>
        ValidateMessage(null, ActualPrimary, "Primary Missing", "  Line 42: Missing expected issue 'Lorem ipsum'");

    [TestMethod]
    public void CreateMessage_UnexpectedIssue() =>
        ValidateMessage(ActualPrimary, null, "Primary Unexpected", "  Line 42: Unexpected issue 'Lorem ipsum' Rule S1234");

    [TestMethod]
    public void CreateMessage_SecondaryText() =>
        ValidateMessage(
            new(IssueType.Secondary, "File.cs", 42, "Lorem ipsum", null, null, null, "S1234"),
            null,
            "Secondary Unexpected",
            "  Line 42 Secondary location: Unexpected issue 'Lorem ipsum' Rule S1234");

    [TestMethod]
    public void CreateMessage_WrongMessage() =>
        ValidateMessage(
            ActualSecondary,
            new(IssueType.Secondary, "File.cs", 42, "Dolor sit", "Flag1", 2, 2),
            "Secondary Different Message",
            "  Line 42 Secondary location: The expected message 'Dolor sit' does not match the actual message 'Lorem ipsum' Rule S1234 ID Flag1");

    [TestMethod]
    public void CreateMessage_WrongStartLocation()
    {
        ValidateMessage(
            ActualSecondary,
            new(IssueType.Secondary, "File.cs", 42, "Lorem ipsum", "Flag1", 2, 2),
            "Secondary Different Location",
            "  Line 42 Secondary location: Should start on column 2 but got column 42 Rule S1234 ID Flag1");
    }

    [TestMethod]
    public void CreateMessage_WrongLength() =>
        ValidateMessage(
            ActualSecondary,
            new(IssueType.Secondary, "File.cs", 42, "Lorem ipsum", "Flag1", 42, 2),
            "Secondary Different Length",
            "  Line 42 Secondary location: Should have a length of 2 but got a length of 10 Rule S1234 ID Flag1");

    [TestMethod]
    public void CreateMessage_WrongIssueId() =>
        ValidateMessage(
            ActualSecondary,
            new IssueLocation(IssueType.Secondary, "File.cs", 42, "Lorem ipsum", "DifferentId", 42, 10),
            "Secondary Different ID",
            "  Line 42 Secondary location: The expected issueId 'DifferentId' does not match the actual issueId 'Flag1' Rule S1234 ID Flag1");

    private static void ValidateMessage(IssueLocation actualIssue, IssueLocation expectedIssue, string expectedShortDescription, string expectedFullDescription)
    {
        var message = new IssueLocationPair(actualIssue, expectedIssue).CreateMessage();
        message.ShortDescription.Should().Be(expectedShortDescription);
        message.FullDescription.Should().Be(expectedFullDescription);
    }
}
