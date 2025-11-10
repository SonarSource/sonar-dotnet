/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation.Test;

[TestClass]
public class CompilationIssuesTest
{
    [TestMethod]
    public void UniqueKeys() =>
        CreateSut().UniqueKeys().Should().BeEquivalentTo(new IssueLocationKey[]
        {
            new(IssueType.Primary, "First.cs", 11),
            new(IssueType.Secondary, "First.cs", 11),
            new(IssueType.Error, "First.cs", 11),
            new(IssueType.Primary, "First.cs", 99),
            new(IssueType.Primary, "Second.cs", 11),
            new(IssueType.Secondary, "Third.cs", 1)
        });

    [TestMethod]
    public void UniqueKeys_IteratingIsResilientToRemoval()
    {
        var sut = CreateSut();
        var enumerator = sut.UniqueKeys().GetEnumerator();
        enumerator.MoveNext();
        sut.Remove((IssueLocationKey)enumerator.Current);
        enumerator.Invoking(x => x.MoveNext()).Should().NotThrow();
    }

    [TestMethod]
    public void Remove_NoMatch()
    {
        var sut = CreateSut();
        sut.Should().HaveCount(14);
        sut.Remove(new(IssueType.Primary, "ThisKeyIsNotPresent.cs", 11)).Should().BeEmpty();
        sut.Should().HaveCount(14);
    }

    [TestMethod]
    public void Remove_WhenPresent()
    {
        var sut = CreateSut();
        sut.Should().HaveCount(14);
        sut.Remove(new(IssueType.Primary, "Second.cs", 11)).Should().HaveCount(5);
        sut.Should().HaveCount(9);
    }

    [TestMethod]
    public void Remove_WhenPresent_ReturnsAllOccurances()
    {
        var sut = CreateSut();
        sut.Should().HaveCount(14);
        sut.Remove(new(IssueType.Secondary, "Third.cs", 1)).Should().HaveCount(3);
        sut.Should().HaveCount(11);
    }

    [TestMethod]
    public void Dump()
    {
        using var log = new LogTester();
        CreateSut().Dump("C# 99");
        log.AssertContain("""
            Actual C# 99 diagnostics First.cs:
                S1111, Line: 11, [1, 10] Lorem ipsum
                S1111, Line: 11, [9, 10] Lorem ipsum
                S2222, Line: 11, [1, 10] Lorem 2222 ipsum
                S2222, Line: 11, [1, 10] Lorem 2222 ipsum
                CS000, Line: 11, [1, 10] Compilation error
                S2222, Line: 99, [1, 10] Lorem 2222 ipsum
            Actual C# 99 diagnostics Second.cs:
                S2222, Line: 11, [1, 11] Lorem 2222 ipsum
                S2222, Line: 11, [2, 12] Lorem 2222 ipsum
                S2222, Line: 11, [3, 13] Lorem 2222 ipsum
                S2222, Line: 11, [4, 14] Lorem 2222 ipsum
                S2222, Line: 11, [5, 15] Lorem 2222 ipsum
            """);
    }

    private static CompilationIssues CreateSut() =>
        new(new IssueLocation[]
        {
            new(IssueType.Primary, "First.cs", 11, "Lorem ipsum", null, 1, 10, "S1111"),
            new(IssueType.Primary, "First.cs", 11, "Lorem ipsum", null, 9, 10, "S1111"),
            new(IssueType.Primary, "First.cs", 99, "Lorem 2222 ipsum", null, 1, 10, "S2222"),
            new(IssueType.Primary, "First.cs", 11, "Lorem 2222 ipsum", null, 1, 10, "S2222"),
            new(IssueType.Secondary, "First.cs", 11, "Lorem 2222 ipsum", null, 1, 10, "S2222"),
            new(IssueType.Error, "First.cs", 11, "Compilation error", null, 1, 10, "CS000"),
            new(IssueType.Primary, "Second.cs", 11, "Lorem 2222 ipsum", null, 1, 11, "S2222"),
            new(IssueType.Primary, "Second.cs", 11, "Lorem 2222 ipsum", null, 2, 12,  "S2222"),
            new(IssueType.Primary, "Second.cs", 11, "Lorem 2222 ipsum", null, 3, 13,  "S2222"),
            new(IssueType.Primary, "Second.cs", 11, "Lorem 2222 ipsum", null, 4, 14,  "S2222"),
            new(IssueType.Primary, "Second.cs", 11, "Lorem 2222 ipsum", null, 5, 15,  "S2222"),
            new(IssueType.Secondary, "Third.cs", 1, "Identical secondaries", null, 1, 10,  "S3333"),
            new(IssueType.Secondary, "Third.cs", 1, "Identical secondaries", null, 1, 10,  "S3333"),
            new(IssueType.Secondary, "Third.cs", 1, "Identical secondaries", null, 1, 10,  "S3333")
        });
}
