/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Test.Verification.IssueValidation;

[TestClass]
public class CompilationIssuesTest
{
    [TestMethod]
    public void UniqueKeys() =>
        CreateSut().UniqueKeys().Should().BeEquivalentTo(new IssueLocationKey[]
        {
            new("First.cs", 11, true),
            new("First.cs", 11, false),
            new("First.cs", 99, true),
            new("Second.cs", 11, true),
            new("Third.cs", 1, false)
        });

    [TestMethod]
    public void Remove_NoMatch()
    {
        var sut = CreateSut();
        sut.Should().HaveCount(13);
        sut.Remove(new("ThisKeyIsNotPresent.cs", 11, true)).Should().BeEmpty();
        sut.Should().HaveCount(13);
    }

    [TestMethod]
    public void Remove_WhenPresent()
    {
        var sut = CreateSut();
        sut.Should().HaveCount(13);
        sut.Remove(new("Second.cs", 11, true)).Should().HaveCount(5);
        sut.Should().HaveCount(8);
    }

    [TestMethod]
    public void Remove_WhenPresent_ReturnsAllOccurances()
    {
        var sut = CreateSut();
        sut.Should().HaveCount(13);
        sut.Remove(new("Third.cs", 1, false)).Should().HaveCount(3);
        sut.Should().HaveCount(10);
    }

    [TestMethod]
    public void Dump()
    {
        using var log = new LogTester();
        CreateSut().Dump();
        log.AssertContain("""
            Actual C# 99 diagnostics First.cs:
                S1111, Line: 11, [1, 10] Lorem ipsum
                S1111, Line: 11, [9, 10] Lorem ipsum
                S2222, Line: 11, [1, 10] Lorem 2222 ipsum
                S2222, Line: 11, [1, 10] Lorem 2222 ipsum
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
        new("C# 99", new IssueLocation[]
        {
            new() { RuleId = "S1111", FilePath = "First.cs", LineNumber = 11, IsPrimary = true, Start = 1, Length = 10, Message = "Lorem ipsum" },
            new() { RuleId = "S1111", FilePath = "First.cs", LineNumber = 11, IsPrimary = true, Start = 9, Length = 10, Message = "Lorem ipsum" },
            new() { RuleId = "S2222", FilePath = "First.cs", LineNumber = 99, IsPrimary = true, Start = 1, Length = 10, Message = "Lorem 2222 ipsum" },
            new() { RuleId = "S2222", FilePath = "First.cs", LineNumber = 11, IsPrimary = true, Start = 1, Length = 10, Message = "Lorem 2222 ipsum" },
            new() { RuleId = "S2222", FilePath = "First.cs", LineNumber = 11, IsPrimary = false, Start = 1, Length = 10, Message = "Lorem 2222 ipsum" },
            new() { RuleId = "S2222", FilePath = "Second.cs", LineNumber = 11, IsPrimary = true, Start = 1, Length = 11, Message = "Lorem 2222 ipsum" },
            new() { RuleId = "S2222", FilePath = "Second.cs", LineNumber = 11, IsPrimary = true, Start = 2, Length = 12, Message = "Lorem 2222 ipsum" },
            new() { RuleId = "S2222", FilePath = "Second.cs", LineNumber = 11, IsPrimary = true, Start = 3, Length = 13, Message = "Lorem 2222 ipsum" },
            new() { RuleId = "S2222", FilePath = "Second.cs", LineNumber = 11, IsPrimary = true, Start = 4, Length = 14, Message = "Lorem 2222 ipsum" },
            new() { RuleId = "S2222", FilePath = "Second.cs", LineNumber = 11, IsPrimary = true, Start = 5, Length = 15, Message = "Lorem 2222 ipsum" },
            new() { RuleId = "S3333", FilePath = "Third.cs", LineNumber = 1, IsPrimary = false, Start = 1, Length = 10, Message = "Multiple identical secondaries" },
            new() { RuleId = "S3333", FilePath = "Third.cs", LineNumber = 1, IsPrimary = false, Start = 1, Length = 10, Message = "Multiple identical secondaries" },
            new() { RuleId = "S3333", FilePath = "Third.cs", LineNumber = 1, IsPrimary = false, Start = 1, Length = 10, Message = "Multiple identical secondaries" }
        });
}
