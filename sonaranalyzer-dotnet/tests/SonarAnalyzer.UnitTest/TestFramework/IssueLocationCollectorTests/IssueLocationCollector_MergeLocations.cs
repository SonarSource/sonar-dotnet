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
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IssueLocation = SonarAnalyzer.UnitTest.TestFramework.IssueLocationCollector.IssueLocation;

namespace SonarAnalyzer.UnitTest.TestFramework.IssueLocationCollectorTests
{
    [TestClass]
    public class IssueLocationCollector_MergeLocations
    {
        [TestMethod]
        public void MergeLocations_No_Issues()
        {
            var result = new IssueLocationCollector().MergeLocations(
                Enumerable.Empty<IssueLocation>(),
                Enumerable.Empty<IssueLocation>());

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void MergeLocations_Issues_Same_Line()
        {
            var result = new IssueLocationCollector().MergeLocations(
                new[] { new IssueLocation { LineNumber = 3, Message = "message 1" } },
                new[] { new IssueLocation { LineNumber = 3, Start = 10, Length = 5, Message = "message 2" } });

            result.Should().ContainSingle();

            result[0].Message.Should().Be("message 1");

            // We take only Start and Length when merging precise location comments
            result[0].Start.Should().Be(10);
            result[0].Length.Should().Be(5);
        }

        [TestMethod]
        public void MergeLocations_Issues_Different_Lines()
        {
            var result = new IssueLocationCollector().MergeLocations(
                new[] { new IssueLocation { LineNumber = 3, Message = "message 1" } },
                new[] { new IssueLocation { LineNumber = 10, Start = 10, Length = 5, Message = "message 2" } });

            result.Should().HaveCount(2);

            result[0].Message.Should().Be("message 1");
            result[0].Start.Should().NotHaveValue();
            result[0].Length.Should().NotHaveValue();

            result[1].Message.Should().Be("message 2");
            result[1].Start.Should().Be(10);
            result[1].Length.Should().Be(5);
        }

        [TestMethod]
        public void MergeLocations_More_Than_One_Precise_Location_For_Same_Issue()
        {
            Action action = () => new IssueLocationCollector().MergeLocations(
                new[] { new IssueLocation { LineNumber = 3 } },
                new[]
                {
                    new IssueLocation { LineNumber = 3 },
                    new IssueLocation { LineNumber = 3 }
                });

            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void MergeLocations_Empty_Issues_NonEmpty_PreciseLocations()
        {
            var result = new IssueLocationCollector().MergeLocations(
                Enumerable.Empty<IssueLocation>(),
                new[] { new IssueLocation { LineNumber = 3 } });

            result.Should().ContainSingle();
        }

        [TestMethod]
        public void MergeLocations_NonEmpty_Issues_Empty_PreciseLocations()
        {
            var result = new IssueLocationCollector().MergeLocations(
                new[] { new IssueLocation { LineNumber = 3 } },
                Enumerable.Empty<IssueLocation>());

            result.Should().ContainSingle();
        }
    }
}
