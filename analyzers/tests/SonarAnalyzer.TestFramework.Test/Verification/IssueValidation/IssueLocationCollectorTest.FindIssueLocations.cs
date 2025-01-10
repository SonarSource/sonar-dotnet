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

using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification.IssueValidation;

public partial class IssueLocationCollectorTest
{
    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Two_Flows()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant [flow1,flow2]
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary, IssueType.Primary },
            expectedLineNumbers: new[] { 3, 3 },
            expectedMessages: new string[] { null, null },
            expectedIssueIds: new[] { "flow1", "flow2" });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Offset_Message_And_Flows()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant@-1 [flow1,flow2] {{Some message}}
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary, IssueType.Primary },
            expectedLineNumbers: new[] { 2, 2 },
            expectedMessages: new[] { "Some message", "Some message" },
            expectedIssueIds: new[] { "flow1", "flow2" });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Reversed_Message_And_Flows()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant {{Some message}} [flow1,flow2]
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 3 },
            expectedMessages: new[] { "Some message" },
            expectedIssueIds: new string[] { null });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Offset()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant@-1
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 2 },
            expectedMessages: new string[] { null },
            expectedIssueIds: new string[] { null });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Message_And_Flows()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant [flow1,flow2] {{Some message}}
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary, IssueType.Primary },
            expectedLineNumbers: new[] { 3, 3 },
            expectedMessages: new[] { "Some message", "Some message" },
            expectedIssueIds: new[] { "flow1", "flow2" });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Message()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant {{Some message}}
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 3 },
            expectedMessages: new[] { "Some message" },
            expectedIssueIds: new string[] { null });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Invalid_Offset()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant@=1
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 3 },
            expectedMessages: new string[] { null },
            expectedIssueIds: new string[] { null });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_With_Flow()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant [last,flow1,flow2]
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary, IssueType.Primary, IssueType.Primary },
            expectedLineNumbers: new[] { 3, 3, 3 },
            expectedMessages: new string[] { null, null, null },
            expectedIssueIds: new[] { "flow1", "flow2", "last" });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 3 },
            expectedMessages: new string[] { null },
            expectedIssueIds: new string[] { null });
    }

    [TestMethod]
    public void FindIssueLocations_Flow_With_Offset_Message_And_Flows()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Secondary@-1 [flow1,flow2] {{Some message}}
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Secondary, IssueType.Secondary },
            expectedLineNumbers: new[] { 2, 2 },
            expectedMessages: new[] { "Some message", "Some message" },
            expectedIssueIds: new[] { "flow1", "flow2" });
    }

    [TestMethod]
    public void FindIssueLocations_NoComment()
    {
        var line = GetLine(2, """
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
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant^5#7
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 3 },
            expectedMessages: new string[] { null },
            expectedIssueIds: new string[] { null });
    }

    [TestMethod]
    public void FindIssueLocations_Secondary_ExactColumn_Ids()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Secondary ^13#9 [myId]
            }
            """);
        VerifyIssueLocations(IssueLocationCollector.FindIssueLocations("File.cs", line),
            expectedTypes: new[] { IssueType.Secondary },
            expectedLineNumbers: new[] { 3 },
            expectedMessages: new string[] { null },
            expectedIssueIds: new[] { "myId" });
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_Offset_ExactColumn_Message_Whitespaces()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); // Noncompliant @-2 ^5#16 [myIssueId] {{MyMessage}}
            }
            """);
        var result = IssueLocationCollector.FindIssueLocations("File.cs", line).ToArray();
        VerifyIssueLocations(result,
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 1 },
            expectedMessages: new[] { "MyMessage" },
            expectedIssueIds: new[] { "myIssueId" });
        result.Select(x => x.Start).Should().Equal(4);
        result.Select(x => x.Length).Should().Equal(16);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_Offset_ExactColumn_Message_NoWhitespace()
    {
        var line = GetLine(2, """
            if (a > b)
            {
                Console.WriteLine(a); //Noncompliant@-2^5#16[myIssueId]{{MyMessage}}
            }
            """);
        var result = IssueLocationCollector.FindIssueLocations("File.cs", line).ToArray();
        VerifyIssueLocations(result,
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 1 },
            expectedMessages: new[] { "MyMessage" },
            expectedIssueIds: new[] { "myIssueId" });
        result.Select(x => x.Start).Should().Equal(4);
        result.Select(x => x.Length).Should().Equal(16);
    }

    [TestMethod]
    public void FindIssueLocations_Noncompliant_Offset_ExactColumn_Message_Whitespaces_Xml()
    {
        var line = GetLine(2, """
            <RootRootRootRootRootRoot />

            <!-- Noncompliant @-2 ^5#16 [myIssueId] {{MyMessage}} -->

            """);
        var result = IssueLocationCollector.FindIssueLocations("File.cs", line).ToArray();
        VerifyIssueLocations(result,
            expectedTypes: new[] { IssueType.Primary },
            expectedLineNumbers: new[] { 1 },
            expectedMessages: new[] { "MyMessage" },
            expectedIssueIds: new[] { "myIssueId" });
        result.Select(x => x.Start).Should().Equal(4);
        result.Select(x => x.Length).Should().Equal(16);
    }
}
