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
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.TestFramework.IssueLocationCollectorTests
{
    [TestClass]
    public class IssueLocationCollector_GetPreciseIssueLocations
    {
        private TextLine GetLine(int lineNumber, string code)
        {
            var sourceText = SourceText.From(code);
            return sourceText.Lines[lineNumber];
        }

        private void VerifyIssueLocations(IEnumerable<IIssueLocation> result,
            IEnumerable<bool> expectedIsPrimary,
            IEnumerable<int> expectedLineNumbers,
            IEnumerable<string> expectedMessages,
            IEnumerable<string> expectedIssueIds)
        {
            result.Select(l => l.IsPrimary).Should().Equal(expectedIsPrimary);
            result.Select(l => l.LineNumber).Should().Equal(expectedLineNumbers);
            result.Select(l => l.Message).Should().Equal(expectedMessages);
            result.Select(l => l.IssueId).Should().Equal(expectedIssueIds);
        }

        [TestMethod]
        public void GetPreciseIssueLocations_NoMessage_NoIds()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().ContainSingle();

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { true },
                expectedLineNumbers: new[] { 3 },
                expectedMessages: new string[] { null },
                expectedIssueIds: new string[] { null });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_With_Offset()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ @-1
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().ContainSingle();

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { true },
                expectedLineNumbers: new[] { 2 },
                expectedMessages: new string[] { null },
                expectedIssueIds: new string[] { null });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_NoMessage_NoIds_Secondary()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ Secondary
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().ContainSingle();

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { false },
                expectedLineNumbers: new[] { 3 },
                expectedMessages: new string[] { null },
                expectedIssueIds: new string[] { null });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_Secondary_With_Offset()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ Secondary@-1
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().ContainSingle();

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { false },
                expectedLineNumbers: new[] { 2 },
                expectedMessages: new string[] { null },
                expectedIssueIds: new string[] { null });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_IssueIds()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ [flow1,flow2]
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().HaveCount(2);

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { true, true },
                expectedLineNumbers: new[] { 3, 3 },
                expectedMessages: new string[] { null, null },
                expectedIssueIds: new string[] { "flow1", "flow2" });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_IssueIds_Secondary()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ Secondary [last1,flow1,flow2]
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().HaveCount(3);

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { false, false, false },
                expectedLineNumbers: new[] { 3, 3, 3 },
                expectedMessages: new string[] { null, null, null },
                expectedIssueIds: new string[] { "flow1", "flow2", "last1" });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_Message_And_IssueIds()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ [flow1,flow2] {{Some message}}
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().HaveCount(2);

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { true, true },
                expectedLineNumbers: new[] { 3, 3 },
                expectedMessages: new string[] { "Some message", "Some message" },
                expectedIssueIds: new string[] { "flow1", "flow2" });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_Message_And_IssueIds_Secondary()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ Secondary [flow1,flow2] {{Some message}}
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().HaveCount(2);

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { false, false },
                expectedLineNumbers: new[] { 3, 3 },
                expectedMessages: new string[] { "Some message", "Some message" },
                expectedIssueIds: new string[] { "flow1", "flow2" });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_Message()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ {{Some message}}
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().ContainSingle();

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { true },
                expectedLineNumbers: new[] { 3 },
                expectedMessages: new string[] { "Some message" },
                expectedIssueIds: new string[] { null });
        }

        [TestMethod]
        public void GetPreciseIssueLocations_Message_Secondary()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
//          ^^^^^^^^^ Secondary {{Some message}}
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().ContainSingle();

            VerifyIssueLocations(result,
                expectedIsPrimary: new[] { false },
                expectedLineNumbers: new[] { 3 },
                expectedMessages: new string[] { "Some message" },
                expectedIssueIds: new string[] { null });
        }

        [TestMethod]
        public void GetIssueLocations_NoComment()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
}");
            var result = new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetIssueLocations_NotStartOfLine()
        {
            var line = GetLine(3, @"if (a > b)
{
    Console.WriteLine(a);
    //      ^^^^^^^^^
}");
            Action action = () => new IssueLocationCollector().GetPreciseIssueLocations(line).ToList();
            action.Should().Throw<AssertFailedException>();
        }
    }
}
