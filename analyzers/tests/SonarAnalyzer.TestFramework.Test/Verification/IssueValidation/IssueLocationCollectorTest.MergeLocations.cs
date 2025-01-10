/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification.IssueValidation;

public partial class IssueLocationCollectorTest
{
    [TestMethod]
    public void MergeLocations_NoIssues() =>
        IssueLocationCollector.MergeLocations(Array.Empty<IssueLocation>(), new List<IssueLocation>()).Should().BeEmpty();

    [TestMethod]
    public void MergeLocations_IssuesSameLine()
    {
        var result = IssueLocationCollector.MergeLocations(
            [new(IssueType.Primary, "File.cs", 3, "message 1", null, null, null)],
            [new(IssueType.Primary, "File.cs", 3, "message 2", null, 10, 5)]);

        result.Should().ContainSingle();

        result[0].Message.Should().Be("message 1");

        // We take only Start and Length when merging precise location comments
        result[0].Start.Should().Be(10);
        result[0].Length.Should().Be(5);
    }

    [TestMethod]
    public void MergeLocations_DifferentIssues_SameSecondaryLocations()
    {
        var primary = new IssueLocation[]
        {
            new(IssueType.Primary, "File.cs", 1, "Primary 1", null, null, null),
            new(IssueType.Primary, "File.cs", 2, "Primary 2", null, null, null)
        };
        var preciseSecondary = new List<IssueLocation>
        {
            new(IssueType.Secondary, "File.cs", 3, "Secondary with same message and location", null, 10, 5),
            new(IssueType.Secondary, "File.cs", 3, "Secondary with same message and location", null, 10, 5)
        };
        IssueLocationCollector.MergeLocations(primary, preciseSecondary).Should().BeEquivalentTo(primary.Concat(preciseSecondary));
    }

    [TestMethod]
    public void MergeLocations_IssuesDifferentLines()
    {
        var result = IssueLocationCollector.MergeLocations(
            [new(IssueType.Primary, "File.cs", 3, "message 1", null, null, null)],
            [new(IssueType.Primary, "File.cs", 10, "message 2", null, 10, 5)]);

        result.Should().HaveCount(2);

        result[0].Message.Should().Be("message 1");
        result[0].Start.Should().NotHaveValue();
        result[0].Length.Should().NotHaveValue();

        result[1].Message.Should().Be("message 2");
        result[1].Start.Should().Be(10);
        result[1].Length.Should().Be(5);
    }

    [TestMethod]
    public void MergeLocations_MoreThanOnePreciseLocationForSameIssue()
    {
        Action action = () => IssueLocationCollector.MergeLocations(
            [new(IssueType.Primary, "File.cs", 3, "Message", null, null, null)],
            [
                new(IssueType.Primary, "File.cs", 3, "Message", null, null, null),
                new(IssueType.Primary, "File.cs", 3, "Message", null, null, null)
            ]);
        action.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void MergeLocations_EmptyIssues_NonEmptyPreciseLocations() =>
        IssueLocationCollector.MergeLocations([], [new(IssueType.Primary, "File.cs", 3, "Message", null, null, null)]).Should().ContainSingle();

    [TestMethod]
    public void MergeLocations_NonEmptyIssues_EmptyPreciseLocations() =>
        IssueLocationCollector.MergeLocations([new(IssueType.Primary, "File.cs", 3, "Message", null, null, null)], []).Should().ContainSingle();
}
