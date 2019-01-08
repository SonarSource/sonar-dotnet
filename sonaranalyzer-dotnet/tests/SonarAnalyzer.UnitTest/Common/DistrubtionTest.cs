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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class DistributionTest
    {
        [TestMethod]
        [TestCategory(MetricsTest.MetricsTestCategoryName)]
        public void Distribution()
        {
            var distribution = new Distribution(new[] { 0, 10, 20 });
            distribution.Ranges.Should().BeEquivalentTo(0, 10, 20);
            distribution.Values.Should().BeEquivalentTo(0, 0, 0);
            distribution.ToString().Should().BeEquivalentTo("0=0;10=0;20=0");

            distribution.Add(0);
            distribution.Values.Should().BeEquivalentTo(1, 0, 0);
            distribution.ToString().Should().BeEquivalentTo("0=1;10=0;20=0");

            distribution.Add(9);
            distribution.Values.Should().BeEquivalentTo(2, 0, 0);
            distribution.ToString().Should().BeEquivalentTo("0=2;10=0;20=0");

            distribution.Add(12);
            distribution.Values.Should().BeEquivalentTo(2, 1, 0);
            distribution.ToString().Should().BeEquivalentTo("0=2;10=1;20=0");

            distribution.Add(3);
            distribution.Values.Should().BeEquivalentTo(3, 1, 0);
            distribution.ToString().Should().BeEquivalentTo("0=3;10=1;20=0");

            distribution.Add(10);
            distribution.Values.Should().BeEquivalentTo(3, 2, 0);
            distribution.ToString().Should().BeEquivalentTo("0=3;10=2;20=0");

            distribution.Add(99);
            distribution.Values.Should().BeEquivalentTo(3, 2, 1);
            distribution.ToString().Should().BeEquivalentTo("0=3;10=2;20=1");

            distribution = new Distribution(new[] { 7, 13 });
            distribution.Ranges.Should().BeEquivalentTo(7, 13);
            distribution.Values.Should().BeEquivalentTo(0, 0);
            distribution.ToString().Should().BeEquivalentTo("7=0;13=0");

            distribution.Add(5);
            distribution.Values.Should().BeEquivalentTo(1, 0);
            distribution.ToString().Should().BeEquivalentTo("7=1;13=0");
        }
    }
}
