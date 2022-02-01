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
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    public partial class IssueLocationCollectorTest
    {
        [TestMethod]
        public void GetExpectedBuildErrors_No_Comments()
        {
            const string code = @"public class Foo
{
    public void Bar(object o)
    {
        Console.WriteLine(o);
    }
}";
            var expectedErrors = IssueLocationCollector.GetExpectedBuildErrors(SourceText.From(code).Lines);

            expectedErrors.Should().BeEmpty();
        }

        [TestMethod]
        public void GetExpectedBuildErrors_ExpectedErrors()
        {
            const string code = @"public class Foo
{
    public void Bar(object o) // Error [CS1234]
    {
        // Error@+1 [CS3456]
        Console.WriteLine(o);
    }
}";
            var expectedErrors = IssueLocationCollector.GetExpectedBuildErrors(SourceText.From(code).Lines).ToList();

            expectedErrors.Should().HaveCount(2);

            expectedErrors.Select(l => l.IsPrimary).Should().Equal(true, true);
            expectedErrors.Select(l => l.LineNumber).Should().Equal(3, 6);
        }

        [TestMethod]
        public void GetExpectedBuildErrors_Multiple_ExpectedErrors()
        {
            const string code = @"public class Foo
{
    public void Bar(object o) // Error [CS1234,CS2345,CS3456]
    {
    }
}";
            var expectedErrors = IssueLocationCollector.GetExpectedBuildErrors(SourceText.From(code).Lines).ToList();

            expectedErrors.Should().HaveCount(3);

            expectedErrors.Select(l => l.IsPrimary).Should().Equal(true, true, true);
            expectedErrors.Select(l => l.LineNumber).Should().Equal(3, 3, 3);
            expectedErrors.Select(l => l.IssueId).Should().Equal("CS1234", "CS2345", "CS3456");
        }
    }
}
