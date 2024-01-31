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

namespace SonarAnalyzer.Test.TestFramework.Tests
{
    public partial class IssueLocationCollectorTest
    {
        [TestMethod]
        public void MergeLocations_NoIssues() =>
            IssueLocationCollector.MergeLocations(Array.Empty<IssueLocation>(), new List<IssueLocation>()).Should().BeEmpty();

        [TestMethod]
        public void MergeLocations_IssuesSameLine()
        {
            var result = IssueLocationCollector.MergeLocations(
                new[] { new IssueLocation { FilePath = "File.cs", LineNumber = 3, Message = "message 1" } },
                new[] { new IssueLocation { FilePath = "File.cs", LineNumber = 3, Start = 10, Length = 5, Message = "message 2" } }.ToList());

            result.Should().ContainSingle();

            result[0].Message.Should().Be("message 1");

            // We take only Start and Length when merging precise location comments
            result[0].Start.Should().Be(10);
            result[0].Length.Should().Be(5);
        }

        [TestMethod]
        public void MergeLocations_DifferentIssues_SameSecondaryLocations()
        {
            var primary = new[]
            {
                new IssueLocation { FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Primary 1" },
                new IssueLocation { FilePath = "File.cs", LineNumber = 2, IsPrimary = true, Message = "Primary 2" }
            };
            var preciseSecondary = new List<IssueLocation>
            {
                new() { FilePath = "File.cs", LineNumber = 3, IsPrimary = false, Message = "Secondary with same message and location", Start = 10, Length = 5 },
                new() { FilePath = "File.cs", LineNumber = 3, IsPrimary = false, Message = "Secondary with same message and location", Start = 10, Length = 5 }
            };
            IssueLocationCollector.MergeLocations(primary, preciseSecondary).Should().BeEquivalentTo(primary.Concat(preciseSecondary));
        }

        [TestMethod]
        public void MergeLocations_IssuesDifferentLines()
        {
            var result = IssueLocationCollector.MergeLocations(
                new[] { new IssueLocation { FilePath = "File.cs", LineNumber = 3, Message = "message 1" } },
                new[] { new IssueLocation { FilePath = "File.cs", LineNumber = 10, Start = 10, Length = 5, Message = "message 2" } }.ToList());

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
                new[] { new IssueLocation { LineNumber = 3 } },
                new List<IssueLocation>
                {
                    new() { LineNumber = 3 },
                    new() { LineNumber = 3 }
                });

            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void MergeLocations_EmptyIssues_NonEmptyPreciseLocations() =>
            IssueLocationCollector.MergeLocations(Array.Empty<IssueLocation>(), new List<IssueLocation> { new IssueLocation { FilePath = "File.cs", LineNumber = 3 } }).Should().ContainSingle();

        [TestMethod]
        public void MergeLocations_NonEmptyIssues_EmptyPreciseLocations() =>
            IssueLocationCollector.MergeLocations(new[] { new IssueLocation { LineNumber = 3 } }, new List<IssueLocation>()).Should().ContainSingle();
    }
}
