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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation.Tests;

public partial class IssueLocationCollectorTest
{
    [TestMethod]
    public void ExpectedIssueLocations_No_Comments()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o)
                {
                    Console.WriteLine(o);
                }
            }
            """;
        IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines).Should().BeEmpty();
    }

    [TestMethod]
    public void ExpectedIssueLocations_Locations_CS()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) // Noncompliant
                {
                    // Noncompliant@+1
                    Console.WriteLine(o);
                }
            }
            """;
        var locations = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        locations.Should().HaveCount(2);
        locations.Select(x => x.Type).Should().Equal(IssueType.Primary, IssueType.Primary);
        locations.Select(x => x.LineNumber).Should().Equal(3, 6);
    }

    [TestMethod]
    public void ExpectedIssueLocations_Locations_VB()
    {
        const string code = """
            Public class Sample

                Public Sub Bar(o As Object) ' Noncompliant
                    ' Noncompliant@+1
                    Console.WriteLine(o)
                End Sub

            End Class
            """;
        var locations = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        locations.Should().HaveCount(2);
        locations.Select(x => x.Type).Should().Equal(IssueType.Primary, IssueType.Primary);
        locations.Select(x => x.LineNumber).Should().Equal(3, 5);
    }

    [TestMethod]
    public void ExpectedIssueLocations_Locations_Xml()
    {
        const string code = """
            <Root>
            <SelfClosing /><!-- Noncompliant -->
            <SelfClosing /><!-- Noncompliant with additional comment and new line
            -->
            <InsideWithSpace><!-- Noncompliant--></InsideWithSpace>
            <InsideNoSpace><!--Secondary--></InsideNoSpace>
            <Innocent><!--Noncompliant@+1--></Innocent>
            <Guilty />
            <!--
            Noncompliant - this should not be detected as expected issue
            -->
            </Root>
            """;
        var locations = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        locations.Should().HaveCount(5);
        locations.Select(x => x.Type).Should().Equal(IssueType.Primary, IssueType.Primary, IssueType.Primary, IssueType.Secondary, IssueType.Primary);
        locations.Select(x => x.LineNumber).Should().Equal(2, 3, 5, 6, 8);
    }

    [TestMethod]
    public void ExpectedIssueLocations_Locations_Razor()
    {
        const string code = """
            <p>The solution to all problems is: 42</p>@* Noncompliant *@
            <p>The solution to all problems is: 42</p>@* Noncompliant with additional comment and new line
            *@
            <p>The solution to all problems is: 42</p>@* Secondary *@

            """;
        var locations = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        locations.Should().HaveCount(3);
        locations.Select(x => x.Type).Should().Equal(IssueType.Primary, IssueType.Primary, IssueType.Secondary);
        locations.Select(x => x.LineNumber).Should().Equal(1, 2, 4);
    }

    [TestMethod]
    public void ExpectedIssueLocations_OnlyCommentedNoncompliant()
    {
        const string code = """
            public class MyNoncompliantClass
            {
                public void NoncompliantMethod(object o)
                {
                    Console.WriteLine(o); // Noncompliant
                }
            }
            """;
        var locations = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        locations.Should().ContainSingle();
        locations.Select(x => x.Type).Should().Equal(IssueType.Primary);
        locations.Select(x => x.LineNumber).Should().Equal(5);
    }

    [TestMethod]
    public void ExpectedIssueLocations_ExactLocations()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o)
            //              ^^^
            //                         ^ Secondary@-1
            //                  ^^^^^^ Secondary@-2 [flow]
            //                   ^^^^ Secondary@-3 [flow] {{Some message}}
                {
                    Console.WriteLine(o);
                }
            }
            """;
        var locations = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        locations.Should().HaveCount(4);
        locations.Select(x => x.Type).Should().BeEquivalentTo([IssueType.Primary, IssueType.Secondary, IssueType.Secondary, IssueType.Secondary]);
        locations.Select(x => x.LineNumber).Should().Equal(3, 3, 3, 3);
        locations.Select(x => x.Start).Should().Equal(16, 27, 20, 21);
        locations.Select(x => x.Length).Should().Equal(3, 1, 6, 4);
    }

    [TestMethod]
    public void ExpectedIssueLocations_ExactColumns()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) // Noncompliant ^17#3
                                          // Secondary@-1 ^28#1
                {
                    Console.WriteLine(o);
                }
            }
            """;
        var locations = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);
        locations.Should().HaveCount(2);
        locations.Select(x => x.Type).Should().BeEquivalentTo([IssueType.Primary, IssueType.Secondary]);
        locations.Select(x => x.LineNumber).Should().Equal(3, 3);
        locations.Select(x => x.Start).Should().Equal(16, 27);
        locations.Select(x => x.Length).Should().Equal(3, 1);
    }

    [TestMethod]
    public void ExpectedIssueLocations_Redundant_Locations()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) // Noncompliant ^17#3
            //              ^^^
                {
                    Console.WriteLine(o);
                }
            }
            """;
        Action action = () => IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Unexpected redundant issue location on line 3. Issue location can be set either with 'precise issue location' or 'exact column location' pattern but not both.");
    }

    [TestMethod]
    public void ExpectedIssueLocations_PrimaryIdWithAndBracketInMessage()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) // Noncompliant [myId1] {{A message with brackets [].}}
                {
                    Console.WriteLine(o);
                }
            }
            """;
        var location = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines).Should().ContainSingle().Subject;
        location.Type.Should().Be(IssueType.Primary);
        location.Message.Should().Be("A message with brackets [].");
        location.IssueId.Should().Be("myId1");
    }

    [TestMethod]
    public void ExpectedIssueLocations_Multiple_PrimaryIds()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) // Noncompliant [myId1]
                {
                    Console.WriteLine(o); // Noncompliant [myId1]
                }
            }
            """;
        Action action = () => IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        action.Should().Throw<InvalidOperationException>().WithMessage("Primary location with id [myId1] found on multiple lines: 3, 5");
    }

    [TestMethod]
    public void ExpectedIssueLocations_Invalid_Type_Format()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) // Is Noncompliant
                {
                    Console.WriteLine(o);
                }
            }
            """;
        Action action = () => IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        action.Should().Throw<InvalidOperationException>().WithMessage("""
            File.cs line 2 contains '// ... Noncompliant' comment, but it is not recognized as one of the expected patterns.
            Either remove the 'Noncompliant' word or fix the pattern.
            """);
    }

    [TestMethod]
    public void ExpectedIssueLocations_Invalid_Precise_Format()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) // Noncompliant
            //  issue is here   ^^^^^^
                {
                    Console.WriteLine(o);
                }
            }
            """;
        Action action = () => IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines);

        action.Should().Throw<InvalidOperationException>().WithMessage("""
            File.cs line 3 looks like it contains comment for precise location '^^'.
            Either remove the precise pattern '^^' from the comment, or fix the pattern.
            """);
    }

    [TestMethod]
    public void ExpectedBuildErrors_ExpectedErrors()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) // Error [CS1234]
                {
                    // Error@+1 [CS3456]
                    Console.WriteLine(o);
                }
            }
            """;
        var expectedErrors = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines).ToList();

        expectedErrors.Should().HaveCount(2);
        expectedErrors.Select(x => x.Type).Should().Equal(IssueType.Error, IssueType.Error);
        expectedErrors.Select(x => x.LineNumber).Should().Equal(3, 6);
    }

    [TestMethod]
    public void ExpectedBuildErrors_Multiple_ExpectedErrors()
    {
        const string code = """
            public class Sample
            {
                public void Method(object o) { } // Error [CS1234,CS2345,CS3456]
            }
            """;
        var expectedErrors = IssueLocationCollector.ExpectedIssueLocations("File.cs", SourceText.From(code).Lines).ToList();

        expectedErrors.Should().HaveCount(3);
        expectedErrors.Select(x => x.Type).Should().Equal(IssueType.Error, IssueType.Error, IssueType.Error);
        expectedErrors.Select(x => x.LineNumber).Should().Equal(3, 3, 3);
        expectedErrors.Select(x => x.IssueId).Should().Equal("CS1234", "CS2345", "CS3456");
    }
}
