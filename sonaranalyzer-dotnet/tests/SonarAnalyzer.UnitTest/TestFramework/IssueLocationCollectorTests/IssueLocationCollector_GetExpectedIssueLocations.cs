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
using System;
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
        public void GetExpectedIssueLocations_OnlyCommentedNoncompliant()
        {
            var code = @"public class MyNoncompliantClass
{
    public void NoncompliantMethod(object o)
    {
        Console.WriteLine(o); // Noncompliant
    }
}";
            var locations = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(1);
            locations.Select(l => l.IsPrimary).Should().Equal(new[] { true });
            locations.Select(l => l.LineNumber).Should().Equal(new[] { 5 });
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
            locations.Select(l => l.Start).Should().Equal(new[] { 16, 27 });
            locations.Select(l => l.Length).Should().Equal(new[] { 3, 1 });
        }

        [TestMethod]
        public void GetExpectedIssueLocations_ExactColumns()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Noncompliant ^17#3
                              // Secondary@-1 ^28#1
    {
        Console.WriteLine(o);
    }
}";
            var locations = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(2);

            locations.Select(l => l.IsPrimary).Should().BeEquivalentTo(new[] { true, false });
            locations.Select(l => l.LineNumber).Should().Equal(new[] { 3, 3 });
            locations.Select(l => l.Start).Should().Equal(new[] { 16, 27 });
            locations.Select(l => l.Length).Should().Equal(new[] { 3, 1 });
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Redundant_Locations()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Noncompliant ^17#3
//              ^^^
    {
        Console.WriteLine(o);
    }
}";

            Action action = () => new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Unexpected redundant issue location on line 3. Issue location can be set either " +
                "with 'precise issue location' or 'exact column location' pattern but not both.");
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Multiple_PrimaryIds()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Noncompliant [myId1]
    {
        Console.WriteLine(o); // Noncompliant [myId1]
    }
}";

            Action action = () => new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Primary location with id [myId1] found on multiple lines: 3,5");
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Invalid_Type_Format()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Is Noncompliant
    {
        Console.WriteLine(o);
    }
}";

            Action action = () => new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage(@"Line 2 looks like it contains comment RegEx for noncompliant code, but it is not recognized as one of the expected RegEx.
Either remove the Noncompliant/Secondary word or precise pattern '^^' from the comment, or fix the pattern.");
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Invalid_Precise_Format()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Noncompliant
//  issue is here   ^^^^^^
    {
        Console.WriteLine(o);
    }
}";

            Action action = () => new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage(@"Line 3 looks like it contains comment RegEx for noncompliant code, but it is not recognized as one of the expected RegEx.
Either remove the Noncompliant/Secondary word or precise pattern '^^' from the comment, or fix the pattern.");
        }
    }
}
