/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification.IssueValidation;

[TestClass]
public partial class IssueLocationCollectorTest
{
    private static TextLine GetLine(int lineNumber, string code) =>
        SourceText.From(code).Lines[lineNumber];

    private static void VerifyIssueLocations(IEnumerable<IssueLocation> result,
                                             IEnumerable<IssueType> expectedTypes,
                                             IEnumerable<int> expectedLineNumbers,
                                             IEnumerable<string> expectedMessages,
                                             IEnumerable<string> expectedIssueIds)
    {
        var values = result.ToArray();
        values.Should().HaveSameCount(expectedTypes);
        result.Select(x => x.Type).Should().Equal(expectedTypes);
        result.Select(x => x.LineNumber).Should().Equal(expectedLineNumbers);
        result.Select(x => x.Message).Should().Equal(expectedMessages);
        result.Select(x => x.IssueId).Should().Equal(expectedIssueIds);
    }
}
