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

using Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.TestFramework.Verification;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification;

[TestClass]
public class DiagnosticVerifierTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<CS.BinaryOperationWithIdenticalExpressions>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void PrimaryIssueNotExpected() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    // Secondary@+1
                    if (a == a)
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 6: Unexpected issue 'Correct one of the identical expressions on both sides of operator '=='.' Rule S1764
            """);

    [TestMethod]
    public void SecondaryIssueNotExpected() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    if (a == a) // Noncompliant
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 5 Secondary location: Unexpected issue ''
            """);

    [TestMethod]
    public void UnexpectedSecondaryIssue_WrongId() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    // Secondary@+1 [myWrongId]
                    if (a == a) // Noncompliant [myId]
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 6 Secondary location: The expected issueId 'myWrongId' does not match the actual issueId 'myId' Rule S1764 ID myId
            """);

    [TestMethod]
    public void UnexpectedSecondaryIssue_WrongIdWithWrongPrimaryMessageAndLocation() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    // Secondary@+1 [myWrongId]
                    if (a == a) // Noncompliant ^1#1 {{This has wrong message and location and still needs to match secondary ID}} [myId]
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 6: The expected message 'This has wrong message and location and still needs to match secondary ID' does not match the actual message 'Correct one of the identical expressions on both sides of operator '=='.' Rule S1764
              Line 6 Secondary location: The expected issueId 'myWrongId' does not match the actual issueId '' Rule S1764 ID myWrongId
            """);

    [TestMethod]
    public void UnexpectedSecondaryIssue_MissingExpectedId() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    // Secondary@+1
                    if (a == a) // Noncompliant [myId]
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 6 Secondary location: The expected issueId '' does not match the actual issueId 'myId' Rule S1764 ID myId
            """);

    [TestMethod]
    public void UnexpectedSecondaryIssue_MissingActualId() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    // Secondary@+1 [myWrongId]
                    if (a == a) // Noncompliant
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 6 Secondary location: The expected issueId 'myWrongId' does not match the actual issueId '' Rule S1764 ID myWrongId
            """);

    [TestMethod]
    public void UnexpectedSecondaryIssue_WrongIdWithMultipleIssues() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    if (a == a) { if ( b == b) { } } // Noncompliant [idForAA, idForBB]
                    // Secondary@-1 [wrongId]
                    // Secondary@-2 [idForAA]
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 5 Secondary location: The expected issueId 'wrongId' does not match the actual issueId 'idForBB' Rule S1764 ID idForBB
            """);

    [TestMethod]
    public void UnexpectedSecondaryIssue_WrongIdsWithWrongLocations() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b, bool c)
                {
                    if (a == a) { if ( b == b) { if ( c == c) { } } } // Noncompliant [idForAA, idForBB, idForCC]
                    // Secondary@-1 ^0#0 [idForAA] All are on wrong location
                    // Secondary@-2 ^0#0 [wrongId] They should prefer to match on ID first, then on the closest location
                    // Secondary@-3 ^0#0 [idForCC]
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 5 Secondary location: Should start on column -1 but got column 12 Rule S1764 ID idForAA
              Line 5 Secondary location: The expected issueId 'wrongId' does not match the actual issueId 'idForBB' Rule S1764 ID idForBB
              Line 5 Secondary location: Should start on column -1 but got column 42 Rule S1764 ID idForCC
            """);

    [TestMethod]
    public void SecondaryIssueUnexpectedMessage() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    // Secondary@+1 {{Wrong message}}
                    if (a == a) // Noncompliant
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 6 Secondary location: The expected message 'Wrong message' does not match the actual message ''
            """);

    [TestMethod]
    public void SecondaryIssueUnexpectedStartPosition() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    if (a == a)
            //               ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
            //        ^ Secondary@-1
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 5 Secondary location: Should start on column 10 but got column 12
            """);

    [TestMethod]
    public void SecondaryIssueUnexpectedLength() =>
        VerifyThrows("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    if (a == a)
            //               ^ {{Correct one of the identical expressions on both sides of operator '=='.}}
            //          ^^^^ Secondary@-1
                    { }
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 5 Secondary location: Should have a length of 4 but got a length of 1
            """);

    [TestMethod]
    public void ValidVerification() =>
        builder.AddSnippet("""
            public class UnexpectedSecondary
            {
                public void Test(bool a, bool b)
                {
                    // Secondary@+1
                    if (a == a) // Noncompliant
                    { }
                }
            }
            """).Invoking(x => x.Verify()).Should().NotThrow();

    [TestMethod]
    public void BuildError_CS() =>
        VerifyThrows("""
            public class UnexpectedBuildError
            {
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 2: Unexpected error, use // Error [CS1513] } expected
            """);

    [TestMethod]
    public void BuildError_VB() =>
        new VerifierBuilder<DummyAnalyzerVB>().AddSnippet("Public Class UnexpectedBuildError")
            .WithConcurrentAnalysis(false)
            .Invoking(x => x.Verify())
            .Should().Throw<DiagnosticVerifierException>()
            .Which.Message.Should().ContainIgnoringLineEndings("""
                There are differences for VisualBasic12 snippet0.vb:
                  Line 1: Unexpected error, use ' Error [BC30481] 'Class' statement must end with a matching 'End Class'.
                """);

    [TestMethod]
    public void UnexpectedRemainingOpeningCurlyBrace() =>
        VerifyThrows("""
            public class UnexpectedRemainingCurlyBrace
            {
                public void Test(bool a, bool b)
                {
                    if (a == a) // Noncompliant {Wrong format message}
                    { }
                }
            }
            """,
            """
            Unexpected '{' is used after the recognized issue pattern. Remove it, or fix the pattern to the valid format:
            // ^^^^ (Noncompliant|Secondary|Error) ^1#10 [issue-id1, issue-id2] {{Expected message.}} Final note without significant special characters
            """);

    [TestMethod]
    public void UnexpectedRemainingClosingCurlyBrace() =>
        VerifyThrows("""
            public class UnexpectedRemainingCurlyBrace
            {
                public void Test(bool a, bool b)
                {
                    if (a == a) // Noncompliant (Another Wrong format message}
                    { }
                }
            }
            """,
            """
            Unexpected '}' is used after the recognized issue pattern. Remove it, or fix the pattern to the valid format:
            // ^^^^ (Noncompliant|Secondary|Error) ^1#10 [issue-id1, issue-id2] {{Expected message.}} Final note without significant special characters
            """);

    [TestMethod]
    public void ExpectedIssuesNotRaised() =>
        VerifyThrows("""
            public class ExpectedIssuesNotRaised
            {
                public void Test(bool a, bool b) // Noncompliant [MyId0]
                {
                    if (a == b) // Noncompliant
                    { } // Secondary [MyId1]
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 3: Missing expected issue ID MyId0
              Line 5: Missing expected issue
              Line 6 Secondary location: Missing expected issue ID MyId1
            """);

    [TestMethod]
    public void ExpectedIssuesNotRaised_WhileBeingRaisedOnceOnTheSameLine() =>
        VerifyThrows("""
            public class Sample
            {
                public void Test(bool a, bool b)
                {
                    if (a == a) { if (a == b) {  }  }   // Noncompliant
                                                        // Noncompliant@-1
                                                        // Secondary@-2
                }
            }
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 5: Missing expected issue
            """);

    [TestMethod]
    public void ExpectedIssuesNotRaised_MultipleFiles() =>
        builder.WithBasePath("DiagnosticsVerifier")
            .AddPaths("ExpectedIssuesNotRaised.cs", "ExpectedIssuesNotRaised2.cs")
            .WithConcurrentAnalysis(false)
            .Invoking(x => x.Verify())
            .Should().Throw<DiagnosticVerifierException>()
            .WithMessage("""
                There are differences for CSharp7 DiagnosticsVerifier\ExpectedIssuesNotRaised.cs:
                  Line 3: Missing expected issue ID MyId0
                  Line 5: Missing expected issue
                  Line 6 Secondary location: Missing expected issue ID MyId1

                There are differences for CSharp7 DiagnosticsVerifier\ExpectedIssuesNotRaised2.cs:
                  Line 3: Missing expected issue ID MyId0
                  Line 5: Missing expected issue
                  Line 6 Secondary location: Missing expected issue ID MyId1
                """);

    [TestMethod]
    public void ProjectLevelIssues_CorrectLocation_WrongMessage()
    {
        var project = SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.VisualBasic)
            .AddSnippet("' Noncompliant ^1#0 {{This is not the correct message.}}");
        var compilation = project.GetCompilation(null, new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionExplicit: false));
        ((Action)(() => DiagnosticVerifier.Verify(compilation, [new VB.OptionExplicitOn()], CompilationErrorBehavior.Default, null, [], [])))
            .Should().Throw<DiagnosticVerifierException>()
            .WithMessage("""
                There are differences for VisualBasic17_13 <project-level-issue>:
                  Line 1: The expected message 'This is not the correct message.' does not match the actual message 'Configure 'Option Explicit On' for assembly 'project0'.' Rule S6146
                """);
    }

    [TestMethod]
    public void ProjectLevelIssues_WrongLocation()
    {
        var project = SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.VisualBasic)
            .AddSnippet("' Noncompliant@+1 {{This is expected on a wrong line.}}");
        var compilation = project.GetCompilation(null, new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionExplicit: false));
        ((Action)(() => DiagnosticVerifier.Verify(compilation, [new VB.OptionExplicitOn()], CompilationErrorBehavior.Default, null, [], [])))
            .Should().Throw<DiagnosticVerifierException>()
            .WithMessage("""
                There are differences for VisualBasic17_13 <project-level-issue>:
                  Line 1: Unexpected issue 'Configure 'Option Explicit On' for assembly 'project0'.' Rule S6146

                There are differences for VisualBasic17_13 snippet0.vb:
                  Line 2: Missing expected issue 'This is expected on a wrong line.'
                """);
    }

    [TestMethod]
    [DataRow("First.vb", "Second.vb")]  // File ordering should not confuse the matching
    [DataRow("Second.vb", "First.vb")]
    public void ProjectLevelIssues_HaveExpectedAnnotationWithPath(string projectLevelIssueFile, string fileLevelIssueFile)
    {
        var project = SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.VisualBasic)
            .AddSnippet("' Noncompliant ^1#0 {{Configure 'Option Explicit On' for assembly 'project0'.}}", projectLevelIssueFile)
            .AddSnippet("Option Explicit Off ' Noncompliant ^1#19 {{Change this to 'Option Explicit On'.}}", fileLevelIssueFile);
        var compilation = project.GetCompilation(null, new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionExplicit: false));
        ((Action)(() => DiagnosticVerifier.Verify(compilation, [new VB.OptionExplicitOn()], CompilationErrorBehavior.Default, null, [], []))).Should().NotThrow();
    }

    [TestMethod]
    public void ProjectLevelIssues_MultipleIssuesRaised()
    {
        var project = SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.VisualBasic)
            .AddSnippet("""
                ' Noncompliant ^1#0 {{Configure 'Option Explicit On' for assembly 'project0'.}}
                ' Noncompliant@-1 ^1#0 {{Configure 'Option Strict On' for assembly 'project0'.}}
                """);
        var compilation = project.GetCompilation(null, new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionExplicit: false, optionStrict: OptionStrict.Off));
        var analyzers = new DiagnosticAnalyzer[] { new VB.OptionExplicitOn(), new VB.OptionStrictOn() };
        ((Action)(() => DiagnosticVerifier.Verify(compilation, analyzers, CompilationErrorBehavior.Default, null, [], [], true))).Should().NotThrow();
    }

    [TestMethod]
    public void DiagnosticsAndErrors_IgnoresLineContinuation_VB()
    {
        const string code = """
            <AttributeUsage(AttributeTargets.All)>  ' This linecontinuation comment is not valid in VB 12
            Public Class Sample
            End Class
            """;
        var compilation = new SnippetCompiler(code, true, AnalyzerLanguage.VisualBasic, parseOptions: new VisualBasicParseOptions(LanguageVersion.VisualBasic12)).Compilation;
        // We need to place many "' Noncompliant" annotations all over the place, so we ignore this error to avoid doing Noncompliant@+1 on too many places
        DiagnosticVerifier.AnalyzerDiagnostics(compilation, new DummyAnalyzerVB(), CompilationErrorBehavior.FailTest).Should().BeEmpty();
    }

    [TestMethod]
    public void Verify_BuildErrors_AreSortedById() =>
        VerifyThrows("""
            var almostTopLevel =
            """, """
            There are differences for CSharp7 snippet0.cs:
              Line 1: Unexpected error, use // Error [CS8107] Feature 'top-level statements' is not available in C# 7.0. Please use language version 9.0 or greater.
              Line 1: Unexpected error, use // Error [CS8805] Program using top-level statements must be an executable.
              Line 1: Unexpected error, use // Error [CS1002] ; expected
              Line 1: Unexpected error, use // Error [CS1733] Expected expression
            """);

    [TestMethod]
    public void Verify_ConcurrentFile_DuplicateIssues_Muted() =>
        CreateConcurrentBuilder("var topLevelStatement = 42;")
            .Invoking(x => x.Verify())
            .Should().Throw<DiagnosticVerifierException>()
            .WithMessage("""
                There are differences for CSharp9 File.cs:
                  Line 1: Unexpected issue 'Message for SDummy' Rule SDummy

                There are 3 more differences in File.Concurrent.cs

                """);

    [TestMethod]
    public void Verify_ConcurrentFile_IssuesOnlyInConcurrent_Reported() =>
        CreateConcurrentBuilder("var topLevelStatement = 42; // Noncompliant")
            .Invoking(x => x.Verify())
            .Should().Throw<DiagnosticVerifierException>()
            .WithMessage("""
                There are differences for CSharp9 File.Concurrent.cs:
                  Line 1: Unexpected error, use // Error [CS0825] The contextual keyword 'var' may only appear within a local variable declaration or in script code
                  Line 1: Unexpected error, use // Error [CS0116] A namespace cannot directly contain members such as fields, methods or statements

                """);

    private void VerifyThrows(string snippet, string expectedMessage) =>
        builder.AddSnippet(snippet)
            .WithConcurrentAnalysis(false)
            .Invoking(x => x.Verify())
            .Should().Throw<DiagnosticVerifierException>()
            .Which.Message.Should().ContainIgnoringLineEndings(expectedMessage);

    private VerifierBuilder CreateConcurrentBuilder(string code) =>
        new VerifierBuilder<DummyAnalyzerCS>()
            .AddPaths(TestFiles.WriteFile(TestContext, @"TestCases\File.cs", code))
            .WithTopLevelStatements()
            .WithConcurrentAnalysis(true);   // Force concurrent analysis over the top level statements
}
