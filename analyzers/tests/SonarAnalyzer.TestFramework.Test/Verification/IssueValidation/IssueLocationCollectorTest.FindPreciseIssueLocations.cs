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

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation.Tests;

public partial class IssueLocationCollectorTest
{
    [TestMethod]
    public void FindPreciseIssueLocations_NoMessage_NoIds()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [3],
            expectedMessages: [null],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_With_Offset()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ @-1
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [2],
            expectedMessages: [null],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_NoMessage_NoIds_Secondary()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ Secondary
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary],
            expectedLineNumbers: [3],
            expectedMessages: [null],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Comment_Secondary()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ Secondary Comment
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary],
            expectedLineNumbers: [3],
            expectedMessages: [null],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Secondary_With_Offset()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ Secondary@-1
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary],
            expectedLineNumbers: [2],
            expectedMessages: [null],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_IssueIds()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ [flow1,flow2]
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary, IssueType.Primary],
            expectedLineNumbers: [3, 3],
            expectedMessages: [null, null],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_IssueIds_Secondary()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ Secondary [last1,flow1,flow2]
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary, IssueType.Secondary, IssueType.Secondary],
            expectedLineNumbers: [3, 3, 3],
            expectedMessages: [null, null, null],
            expectedIssueIds: ["flow1", "flow2", "last1"]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Message_And_IssueIds()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ [flow1,flow2] {{Some message}}
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary, IssueType.Primary],
            expectedLineNumbers: [3, 3],
            expectedMessages: ["Some message", "Some message"],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Message_And_IssueIds_Secondary_CS()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ Secondary [flow1,flow2] {{Some message}}
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary, IssueType.Secondary],
            expectedLineNumbers: [3, 3],
            expectedMessages: ["Some message", "Some message"],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Message_And_IssueIds_Secondary_XML()
    {
        var line = Line(2, """
            <Root>
                        <Baaad />
            <!--        ^^^^^^^^^ Secondary [flow1,flow2] {{Some message}}         -->
            </Root>
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary, IssueType.Secondary],
            expectedLineNumbers: [2, 2],
            expectedMessages: ["Some message", "Some message"],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Message()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ {{Some message}}
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Primary],
            expectedLineNumbers: [3],
            expectedMessages: ["Some message"],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Message_Secondary()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //          ^^^^^^^^^ Secondary {{Some message}}
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary],
            expectedLineNumbers: [3],
            expectedMessages: ["Some message"],
            expectedIssueIds: [null]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Full_Secondary()
    {
        var line = Line(4, """
            if (a > b)
            {
                Console.WriteLine(a);
            // Extra line
            //          ^^^^^^^^^ Secondary@-1 [flow1, flow2] {{Some message}} Comment
            }
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary, IssueType.Secondary],
            expectedLineNumbers: [3, 3],
            expectedMessages: ["Some message", "Some message"],
            expectedIssueIds: ["flow1", "flow2"]);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_NoComment()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            }
            """);
        IssueLocationCollector.FindPreciseIssueLocations("File.cs", line).Should().BeEmpty();
    }

    [TestMethod]
    public void FindPreciseIssueLocations_NotStartOfLineIsOk()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
                //      ^^^^^^^^^
            }
            """);
        var issueLocation = IssueLocationCollector.FindPreciseIssueLocations("File.cs", line).Should().ContainSingle().Subject;
        issueLocation.Type.Should().Be(IssueType.Primary);
        issueLocation.LineNumber.Should().Be(3);
        issueLocation.Start.Should().Be(12);
        issueLocation.Length.Should().Be(9);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_MultiplePatternsOnSameLine()
    {
        var line = Line(3, """
            if (a > b)
            {
                Console.WriteLine(a);
            //  ^^^^^^^ ^^^^^^^^^ ^
            }
            """);
        Action action = () => IssueLocationCollector.FindPreciseIssueLocations("File.cs", line);
        action.Should().Throw<InvalidOperationException>().WithMessage("""
            Expecting only one precise location per line, found 3 on line 3. If you want to specify more than one precise location per line you need to omit the Noncompliant comment:
            internal class MyClass : IInterface1 // there should be no Noncompliant comment
            ^^^^^^^^ {{Do not create internal classes.}}
                                     ^^^^^^^^^^^ @-1 {{IInterface1 is bad for your health.}}
            """);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Xml()
    {
        const string code = """
            <Root>
            <Space><SelfClosing /></Space>
            <!--   ^^^^^^^^^^^^^^^ -->
            <NoSpace><SelfClosing /></NoSpace>
                 <!--^^^^^^^^^^^^^^^-->
            <Multiline><SelfClosing /></Multiline>
            <!--       ^^^^^^^^^^^^^^^
            -->
            </Root>
            """;
        var line = Line(2, code);
        var issueLocation = IssueLocationCollector.FindPreciseIssueLocations("File.cs", line).Should().ContainSingle().Subject;
        issueLocation.Start.Should().Be(7);
        issueLocation.Length.Should().Be(15);

        line = Line(4, code);
        issueLocation = IssueLocationCollector.FindPreciseIssueLocations("File.cs", line).Should().ContainSingle().Subject;
        issueLocation.Start.Should().Be(9);
        issueLocation.Length.Should().Be(15);

        line = Line(6, code);
        issueLocation = IssueLocationCollector.FindPreciseIssueLocations("File.cs", line).Should().ContainSingle().Subject;
        issueLocation.Start.Should().Be(11);
        issueLocation.Length.Should().Be(15);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_RazorWithSpaces()
    {
        var line = Line(1, """
            <p>With spaces: 42</p>
            @*              ^^ *@
            """);
        var issueLocation = IssueLocationCollector.FindPreciseIssueLocations("File.cs", line).Should().ContainSingle().Subject;
        issueLocation.Start.Should().Be(16);
        issueLocation.Length.Should().Be(2);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_RazorWithoutSpaces()
    {
        var line = Line(1, """
            <p>Without spaces: 42</p>
                             @*^^*@
            """);
        var issueLocation = IssueLocationCollector.FindPreciseIssueLocations("File.cs", line).Should().ContainSingle().Subject;
        issueLocation.Start.Should().Be(19);
        issueLocation.Length.Should().Be(2);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_RazorWithMultiline()
    {
        var line = Line(1, """
            <p>Multiline: 42</p>
            @*            ^^
            *@
            """);
        var issueLocation = IssueLocationCollector.FindPreciseIssueLocations("File.cs", line).Should().ContainSingle().Subject;
        issueLocation.Start.Should().Be(14);
        issueLocation.Length.Should().Be(2);
    }

    [TestMethod]
    public void FindPreciseIssueLocations_Message_And_IssueIds_Secondary_Razor()
    {
        var line = Line(1, """
                        <p>The solution is: 42</p>
            @*                              ^^ Secondary [flow1,flow2] {{Some message}}         *@
            """);
        VerifyIssueLocations(
            IssueLocationCollector.FindPreciseIssueLocations("File.cs", line),
            expectedTypes: [IssueType.Secondary, IssueType.Secondary],
            expectedLineNumbers: [1, 1],
            expectedMessages: ["Some message", "Some message"],
            expectedIssueIds: ["flow1", "flow2"]);
    }
}
