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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    [TestClass]
    public partial class IssueLocationCollectorTest
    {
        private static TextLine GetLine(int lineNumber, string code) =>
            SourceText.From(code).Lines[lineNumber];

        private static void VerifyIssueLocations(IReadOnlyCollection<IIssueLocation> result,
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
    }
}
