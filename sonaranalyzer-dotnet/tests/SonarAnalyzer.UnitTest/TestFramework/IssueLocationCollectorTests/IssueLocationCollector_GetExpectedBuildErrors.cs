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
    public class IssueLocationCollector_GetExpectedBuildErrors
    {
        [TestMethod]
        public void GetExpectedBuildErrors_No_Comments()
        {
            var code = @"public class Foo
{
    public void Bar(object o)
    {
        Console.WriteLine(o);
    }
}";
            var expectedErrors = new IssueLocationCollector().GetExpectedBuildErrors(SourceText.From(code).Lines);

            expectedErrors.Should().BeEmpty();
        }

        [TestMethod]
        public void GetExpectedBuildErrors_ExpectedErrors()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Error [CS1234]
    {
        // Error@+1 [CS3456]
        Console.WriteLine(o);
    }
}";
            var expectedErrors = new IssueLocationCollector().GetExpectedBuildErrors(SourceText.From(code).Lines);

            expectedErrors.Should().HaveCount(2);

            expectedErrors.Select(l => l.IsPrimary).Should().Equal(new[] { true, true });
            expectedErrors.Select(l => l.LineNumber).Should().Equal(new[] { 3, 6 });
        }

        [TestMethod]
        public void GetExpectedBuildErrors_Multiple_ExpectedErrors()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Error [CS1234,CS2345,CS3456]
    {
    }
}";
            var expectedErrors = new IssueLocationCollector().GetExpectedBuildErrors(SourceText.From(code).Lines);

            expectedErrors.Should().HaveCount(3);

            expectedErrors.Select(l => l.IsPrimary).Should().Equal(new[] { true, true, true });
            expectedErrors.Select(l => l.LineNumber).Should().Equal(new[] { 3, 3, 3 });
            expectedErrors.Select(l => l.IssueId).Should().Equal(new[] { "CS1234", "CS2345", "CS3456" });
        }
    }
}
