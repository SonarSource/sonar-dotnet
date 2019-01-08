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
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SonarAnalyzer.UnitTest.TestFramework.IssueLocationCollectorTests
{
    [TestClass]
    public class IssueLocationCollector_GetExpectedIssueLocations
    {
        [TestMethod]
        public void GetExpectedIssueLocations_No_Comments()
        {
            var code = @"public class Foo
{
    public void Bar(object o)
    {
        Console.WriteLine(o);
    }
}";
            var locations = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().BeEmpty();
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Locations()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Noncompliant
    {
        // Noncompliant@+1
        Console.WriteLine(o);
    }
}";
            var locations = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(2);

            locations.Select(l => l.IsPrimary).Should().Equal(new[] { true, true });
            locations.Select(l => l.LineNumber).Should().Equal(new[] { 3, 6 });
        }

        [TestMethod]
        public void GetExpectedIssueLocations_ExactLocations()
        {
            var code = @"public class Foo
{
    public void Bar(object o)
//              ^^^
//                         ^ Secondary@-1
    {
        Console.WriteLine(o);
    }
}";
            var locations = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(2);

            locations.Select(l => l.IsPrimary).Should().BeEquivalentTo(new[] { true, false });
            locations.Select(l => l.LineNumber).Should().Equal(new[] { 3, 3 });
        }
    }
}
