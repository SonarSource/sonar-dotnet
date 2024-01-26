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
public class IssueLocationTest
{
    private static readonly IssueLocation Issue = new() { RuleId = "S1234", FilePath = "File.cs", LineNumber = 42, IsPrimary = true, Message = "Lorem ipsum", Start = 42, Length = 10 };

    [TestMethod]
    public void IssueLocationKey_CreatedFromIssue()
    {
        var sut = new IssueLocationKey(Issue);
        sut.FilePath.Should().Be("File.cs");
        sut.LineNumber.Should().Be(42);
        sut.IsPrimary.Should().BeTrue();
    }

    [TestMethod]
    public void IssueLocationKey_IsMatch_MatchesSameKey() =>
        new IssueLocationKey("File.cs", 42, true).IsMatch(Issue).Should().BeTrue();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentFilePath() =>
        new IssueLocationKey("Another.cs", 42, true).IsMatch(Issue).Should().BeFalse();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentLineNumber() =>
        new IssueLocationKey("File.cs", 1024, true).IsMatch(Issue).Should().BeFalse();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentIsPrimary() =>
        new IssueLocationKey("File.cs", 42, false).IsMatch(Issue).Should().BeFalse();
}
