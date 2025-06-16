/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation.Tests;

public partial class IssueLocationCollectorTest
{
    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Two_Flows()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant [flow1,flow2]
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary, IssueType.Primary],
            expectedLineNumbers: [3, 3],
            expectedMessages: [null, null],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Offset_Message_And_Flows()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant@-1 [flow1,flow2] {{Some message}}
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary, IssueType.Primary],
            expectedLineNumbers: [2, 2],
            expectedMessages: ["Some message", "Some message"],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Reversed_Message_And_Flows()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant {{Some message}} [flow1,flow2]
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [3],
            expectedMessages: ["Some message"],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Offset()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant@-1
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [2],
            expectedMessages: [null],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Message_And_Flows()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant [flow1,flow2] {{Some message}}
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary, IssueType.Primary],
            expectedLineNumbers: [3, 3],
            expectedMessages: ["Some message", "Some message"],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Message()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant {{Some message}}
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [3],
            expectedMessages: ["Some message"],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Invalid_Offset()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant@=1
            }
            """);
        ((Func<IEnumerable<IssueLocation>>)(() => IssueLocationCollector.FindIssueLocations("File.cs", line))).Should().Throw<DiagnosticVerifierException>()
            .WithMessage("""
                Unexpected '@' is used after the recognized issue pattern. Remove it, or fix the pattern to the valid format:
                // ^^^^ (Noncompliant|Secondary|Error) ^1#10 [issue-id1, issue-id2] {{Expected message.}} Final note without significant special characters
                """);
    }

    [DataTestMethod]
    [DataRow("Opening { collides with message assertion")]
    [DataRow("Closing } collides with message assertion")]
    [DataRow("The @ collides with line offset @-1 or @+1")]
    [DataRow("The ^ collides numeric location ^1#10 or precise location ^^^^")]
    [DataRow("The # collides numeric location ^1#10")]
    [DataRow("Noncompliant collides with itself, on a wrong place")]
    [DataRow("Secondary collides with itself, on a wrong place")]
    public void FindIssueLocations_Noncompliant_InvalidNote(string note)
    {
        var line = Line(2, $$$"""
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant {{Valid message}} {{{note}}}
            }
            """);
        ((Func<IEnumerable<IssueLocation>>)(() => IssueLocationCollector.FindIssueLocations("File.cs", line))).Should().Throw<DiagnosticVerifierException>()
            .WithMessage("""
                Unexpected '*' is used after the recognized issue pattern. Remove it, or fix the pattern to the valid format:
                // ^^^^ (Noncompliant|Secondary|Error) ^1#10 [issue-id1, issue-id2] {{Expected message.}} Final note without significant special characters
                """);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Flow()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant [last,flow1,flow2]
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary, IssueType.Primary, IssueType.Primary],
            expectedLineNumbers: [3, 3, 3],
            expectedMessages: [null, null, null],
            expectedIssueIds: ["flow1", "flow2", "last"]);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [3],
            expectedMessages: [null],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindIssueLocations_Flow_With_Offset_Message_And_Flows()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Secondary@-1 [flow1,flow2] {{Some message}}
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary, IssueType.Secondary],
            expectedLineNumbers: [2, 2],
            expectedMessages: ["Some message", "Some message"],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindIssueLocations_NoComment()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a);
            }
            """);
        IssueLocationCollector.FindIssueLocations("File.cs", line).Should().BeEmpty();
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_ExactColumn()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant^5#7
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [3],
            expectedMessages: [null],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindIssueLocations_Secondary_ExactColumn_Ids()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Secondary ^13#9 [myId]
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary],
            expectedLineNumbers: [3],
            expectedMessages: [null],
            expectedIssueIds: ["myId"]);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_Offset_ExactColumn_Message_Whitespaces()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); // Noncompliant @-2 ^5#16 [myIssueId] {{MyMessage}}
            }
            """);
        var result = IssueLocationCollector.FindIssueLocations("File.cs", line).ToArray();
        VerifyIssueLocations(
            result,
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [1],
            expectedMessages: ["MyMessage"],
            expectedIssueIds: ["myIssueId"]);
        result.Select(x => x.Start).Should().Equal(4);
        result.Select(x => x.Length).Should().Equal(16);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_Offset_ExactColumn_Message_NoWhitespace()
    {
        var line = Line(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant@-2^5#16[myIssueId]{{MyMessage}}
            }
            """);
        var result = IssueLocationCollector.FindIssueLocations("File.cs", line).ToArray();
        VerifyIssueLocations(
            result,
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [1],
            expectedMessages: ["MyMessage"],
            expectedIssueIds: ["myIssueId"]);
        result.Select(x => x.Start).Should().Equal(4);
        result.Select(x => x.Length).Should().Equal(16);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_Offset_ExactColumn_Message_Whitespaces_Xml()
    {
        var line = Line(2, """
            <RootRootRootRootRootRoot />

            <!-- Noncompliant @-2 ^5#16 [myIssueId] {{MyMessage}} -->

            """);
        var result = IssueLocationCollector.FindIssueLocations("File.cs", line).ToArray();
        VerifyIssueLocations(
            result,
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [1],
            expectedMessages: ["MyMessage"],
            expectedIssueIds: ["myIssueId"]);
        result.Select(x => x.Start).Should().Equal(4);
        result.Select(x => x.Length).Should().Equal(16);
    }
}
