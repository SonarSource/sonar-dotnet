/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    public partial class IssueLocationCollectorTest
    {
        [TestMethod]
        public void GetExpectedIssueLocations_No_Comments()
        {
            const string code = @"public class Foo
{
    public void Bar(object o)
    {
        Console.WriteLine(o);
    }
}";
            var locations = IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().BeEmpty();
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Locations_CS()
        {
            const string code = @"
public class Foo
{
    public void Bar(object o) // Noncompliant
    {
        // Noncompliant@+1
        Console.WriteLine(o);
    }
}";
            var locations = IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(2);

            locations.Select(l => l.IsPrimary).Should().Equal(true, true);
            locations.Select(l => l.LineNumber).Should().Equal(4, 7);
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Locations_VB()
        {
            const string code = @"
Public Class Foo

    Public Sub Bar(o As Object) ' Noncompliant
        ' Noncompliant@+1
        Console.WriteLine(o)
    End Sub

End Class";
            var locations = IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(2);

            locations.Select(l => l.IsPrimary).Should().Equal(true, true);
            locations.Select(l => l.LineNumber).Should().Equal(4, 6);
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Locations_Xml()
        {
            const string code = @"<Root>
<SelfClosing /><!-- Noncompliant -->
<SelfClosing /><!-- Noncompliant with additional comment and new line
-->
<InsideWithSpace><!-- Noncompliant--></InsideWithSpace>
<InsideNoSpace><!--Secondary--></InsideNoSpace>
<Innocent><!--Noncompliant@+1--></Innocent>
<Guilty />
<!--
Noncompliant - this should not be detected as expected issue
-->
</Root>";
            var locations = IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(5);

            locations.Select(l => l.IsPrimary).Should().Equal(true, true, true, false, true);
            locations.Select(l => l.LineNumber).Should().Equal(2, 3, 5, 6, 8);
        }

        [TestMethod]
        public void GetExpectedIssueLocations_OnlyCommentedNoncompliant()
        {
            const string code = @"public class MyNoncompliantClass
{
    public void NoncompliantMethod(object o)
    {
        Console.WriteLine(o); // Noncompliant
    }
}";
            var locations = IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().ContainSingle();
            locations.Select(l => l.IsPrimary).Should().Equal(true);
            locations.Select(l => l.LineNumber).Should().Equal(5);
        }

        [TestMethod]
        public void GetExpectedIssueLocations_ExactLocations()
        {
            const string code = @"public class Foo
{
    public void Bar(object o)
//              ^^^
//                         ^ Secondary@-1
//                  ^^^^^^ Secondary@-2 [flow]
//                   ^^^^ Secondary@-3 [flow] {{Some message}}
    {
        Console.WriteLine(o);
    }
}";
            var locations = IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(4);

            locations.Select(l => l.IsPrimary).Should().BeEquivalentTo(new[] { true, false, false, false });
            locations.Select(l => l.LineNumber).Should().Equal(3, 3, 3, 3);
            locations.Select(l => l.Start).Should().Equal(new[] { 16, 27, 20, 21 });
            locations.Select(l => l.Length).Should().Equal(new[] { 3, 1, 6, 4 });
        }

        [TestMethod]
        public void GetExpectedIssueLocations_ExactColumns()
        {
            const string code = @"public class Foo
{
    public void Bar(object o) // Noncompliant ^17#3
                              // Secondary@-1 ^28#1
    {
        Console.WriteLine(o);
    }
}";
            var locations = IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(2);

            locations.Select(l => l.IsPrimary).Should().BeEquivalentTo(new[] { true, false });
            locations.Select(l => l.LineNumber).Should().Equal(3, 3);
            locations.Select(l => l.Start).Should().Equal(new[] { 16, 27 });
            locations.Select(l => l.Length).Should().Equal(new[] { 3, 1 });
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Redundant_Locations()
        {
            const string code = @"public class Foo
{
    public void Bar(object o) // Noncompliant ^17#3
//              ^^^
    {
        Console.WriteLine(o);
    }
}";

            Action action = () => IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            action.Should()
                  .Throw<InvalidOperationException>()
                  .WithMessage("Unexpected redundant issue location on line 3. Issue location can be set either with 'precise issue location' or 'exact column location' pattern but not both.");
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Multiple_PrimaryIds()
        {
            const string code = @"public class Foo
{
    public void Bar(object o) // Noncompliant [myId1]
    {
        Console.WriteLine(o); // Noncompliant [myId1]
    }
}";

            Action action = () => IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            action.Should().Throw<InvalidOperationException>().WithMessage("Primary location with id [myId1] found on multiple lines: 3, 5");
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Invalid_Type_Format()
        {
            const string code = @"public class Foo
{
    public void Bar(object o) // Is Noncompliant
    {
        Console.WriteLine(o);
    }
}";

            Action action = () => IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            action.Should()
                  .Throw<InvalidOperationException>()
                  .WithMessage(@"Line 2 looks like it contains comment for noncompliant code, but it is not recognized as one of the expected pattern.
Either remove the Noncompliant/Secondary word or precise pattern '^^' from the comment, or fix the pattern.");
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Invalid_Precise_Format()
        {
            const string code = @"public class Foo
{
    public void Bar(object o) // Noncompliant
//  issue is here   ^^^^^^
    {
        Console.WriteLine(o);
    }
}";

            Action action = () => IssueLocationCollector.GetExpectedIssueLocations(SourceText.From(code).Lines);

            action.Should()
                  .Throw<InvalidOperationException>()
                  .WithMessage(@"Line 3 looks like it contains comment for noncompliant code, but it is not recognized as one of the expected pattern.
Either remove the Noncompliant/Secondary word or precise pattern '^^' from the comment, or fix the pattern.");
        }
    }
}
