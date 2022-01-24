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

using System;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    [TestClass]
    public class VerifierTest
    {
        private static readonly VerifierBuilder DummyCS = new VerifierBuilder<DummyAnalyzerCS>();
        private static readonly VerifierBuilder DummyVB = new VerifierBuilder<DummyAnalyzerVB>();
        private static readonly VerifierBuilder DummyCodeFixCS = new VerifierBuilder<DummyAnalyzerCS>()
            .AddPaths("Path.cs")
            .WithCodeFix<DummyCodeFixCS>()
            .WithCodeFixedPaths("Expected.cs");

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor_Null_Throws() =>
            ((Func<Verifier>)(() => new(null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("builder");

        [TestMethod]
        public void Constructor_NoAnalyzers_Throws() =>
            new VerifierBuilder()
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>()
                .WithMessage("Analyzers cannot be empty. Use VerifierBuilder<TAnalyzer> instead or add at least one analyzer using builder.AddAnalyzer().");

        [TestMethod]
        public void Constructor_NullAnalyzers_Throws() =>
            new VerifierBuilder().AddAnalyzer(() => null)
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Analyzer instance cannot be null.");

        [TestMethod]
        public void Constructor_NoPaths_Throws() =>
            DummyCS.Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Paths cannot be empty. Add at least one file using builder.AddPaths() or AddSnippet().");

        [TestMethod]
        public void Constructor_MixedLanguageAnalyzers_Throws() =>
            DummyCS.AddAnalyzer(() => new SonarAnalyzer.Rules.VisualBasic.OptionStrictOn())
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("All Analyzers must declare the same language in their DiagnosticAnalyzerAttribute.");

        [TestMethod]
        public void Constructor_MixedLanguagePaths_Throws() =>
            DummyCS.AddPaths("File.txt")
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Path 'File.txt' doesn't match C# file extension '.cs'.");

        [TestMethod]
        public void Constructor_CodeFix_MissingCodeFixedPath_Throws() =>
            DummyCodeFixCS.WithCodeFixedPaths(null)
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("CodeFixedPath was not set.");

        [TestMethod]
        public void Constructor_CodeFix_WrongCodeFixedPath_Throws() =>
            DummyCodeFixCS.WithCodeFixedPaths("File.vb")
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Path 'File.vb' doesn't match C# file extension '.cs'.");

        [TestMethod]
        public void Constructor_CodeFix_WrongCodeFixedPathBatch_Throws() =>
            DummyCodeFixCS.WithCodeFixedPaths("File.cs", "Batch.vb")
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Path 'Batch.vb' doesn't match C# file extension '.cs'.");

        [TestMethod]
        public void Constructor_CodeFix_MultipleAnalyzers_Throws() =>
            DummyCodeFixCS.AddAnalyzer(() => new DummyAnalyzerCS())
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("When CodeFix is set, Analyzers must contain only 1 analyzer, but 2 were found.");

        [TestMethod]
        public void Constructor_CodeFix_MultiplePaths_Throws() =>
            DummyCodeFixCS.AddPaths("Second.cs", "Third.cs")
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Paths must contain only 1 file, but 3 were found.");

        [TestMethod]
        public void Constructor_CodeFix_WithSnippets_Throws() =>
            DummyCodeFixCS.AddSnippet("Wrong")
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("Snippets must be empty when CodeFix is set.");

        [TestMethod]
        public void Constructor_CodeFix_WrongLanguage_Throws() =>
            DummyCodeFixCS.WithCodeFix<DummyCodeFixVB>()
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("DummyAnalyzerCS language C# does not match DummyCodeFixVB language.");

        [TestMethod]
        public void Constructor_CodeFix_FixableDiagnosticsNotSupported_Throws() =>
            DummyCodeFixCS.WithCodeFix<EmptyMethodCodeFix>()
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("DummyAnalyzerCS does not support diagnostics fixable by the EmptyMethodCodeFix.");

        [TestMethod]
        public void Constructor_CodeFix_MissingAttribute_Throws() =>
            DummyCodeFixCS.WithCodeFix<DummyCodeFixNoAttribute>()
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("DummyCodeFixNoAttribute does not have ExportCodeFixProviderAttribute.");

        [TestMethod]
        public void Constructor_ProtobufPath_MultipleAnalyzers_Throws() =>
            DummyCS.AddSnippet("//Empty").WithProtobufPath("Proto.pb").AddAnalyzer(() => new DummyAnalyzerCS())
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("When ProtobufPath is set, Analyzers must contain only 1 analyzer, but 2 were found.");

        [TestMethod]
        public void Constructor_ProtobufPath_WrongAnalyzerType_Throws() =>
            DummyCS.AddSnippet("//Empty").WithProtobufPath("Proto.pb")
                .Invoking(x => x.Build()).Should().Throw<ArgumentException>().WithMessage("DummyAnalyzerCS does not inherit from UtilityAnalyzerBase.");

        [TestMethod]
        public void Verify_RaiseExpectedIssues_CS() =>
            WithSnippetCS(
@"public class Sample
{
    private int a = 42;     // Noncompliant {{Dummy message}}
    private int b = 42;     // Noncompliant
    private bool c = true;
}").Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void Verify_RaiseExpectedIssues_VB() =>
            WithSnippetVB(
@"Public Class Sample
    Private A As Integer = 42   ' Noncompliant {{Dummy message}}
    Private B As Integer = 42   ' Noncompliant
    Private C As Boolean = True
End Class").Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void Verify_RaiseUnexpectedIssues_CS() =>
            WithSnippetCS(
@"public class Sample
{
    private int a = 42;     // FP
    private int b = 42;     // FP
    private bool c = true;
}").Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>().WithMessage("CSharp7: Unexpected primary issue on line 3, span (2,20)-(2,22) with message 'Dummy message'*");

        [TestMethod]
        public void Verify_RaiseUnexpectedIssues_VB() =>
            WithSnippetVB(
@"Public Class Sample
    Private A As Integer = 42   ' FP
    Private B As Integer = 42   ' FP
    Private C As Boolean = True
End Class").Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>().WithMessage("VisualBasic12: Unexpected primary issue on line 2, span (1,27)-(1,29) with message 'Dummy message'*");

        [TestMethod]
        public void Verify_MissingExpectedIssues() =>
            WithSnippetCS(
@"public class Sample
{
    private bool a = true;   // Noncompliant - FN
    private bool b = true;   // Noncompliant - FN
    private bool c = true;
}").Invoking(x => x.Verify()).Should().Throw<AssertFailedException>().WithMessage(
@"CSharp7: Issue(s) expected but not raised in file(s):
File: File.cs
Line: 3, Type: primary, Id: ''
Line: 4, Type: primary, Id: ''

File: File.Concurrent.cs
Line: 3, Type: primary, Id: ''
Line: 4, Type: primary, Id: ''
");

        [TestMethod]
        public void Verify_TwoAnalyzers() =>
            WithSnippetCS(
@"public class Sample
{
    private int a = 42;     // Noncompliant {{Dummy message}}
                            // Noncompliant@-1
    private int b = 42;     // Noncompliant
                            // Noncompliant@-1
    private bool c = true;
}")
            .AddAnalyzer(() => new DummyAnalyzerCS()) // Duplicate
            .Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void Verify_TwoPaths() =>
            WithSnippetCS(
@"public class First
{
    private bool a = true;     // Noncompliant - FN in File.cs
}")
            .AddPaths(WriteFile("Second.cs",
@"public class Second
{
    private bool a = true;     // Noncompliant - FN in Second.cs
}"))
            .WithConcurrentAnalysis(false)
            .Invoking(x => x.Verify()).Should().Throw<AssertFailedException>().WithMessage(
@"CSharp7: Issue(s) expected but not raised in file(s):
File: File.cs
Line: 3, Type: primary, Id: ''

File: Second.cs
Line: 3, Type: primary, Id: ''
");

        [TestMethod]
        public void Verify_AutogenerateConcurrentFiles()
        {
            var builder = WithSnippetCS("// Noncompliant - FN");
            // Concurrent analysis by-default automatically generates concurrent files - File.Concurrent.cs
            builder.Invoking(x => x.Verify()).Should().Throw<AssertFailedException>().WithMessage(
    @"CSharp7: Issue(s) expected but not raised in file(s):
File: File.cs
Line: 1, Type: primary, Id: ''

File: File.Concurrent.cs
Line: 1, Type: primary, Id: ''
");
            // When AutogenerateConcurrentFiles is turned off, only the provided snippet is analyzed
            builder.WithAutogenerateConcurrentFiles(false).Invoking(x => x.Verify()).Should().Throw<AssertFailedException>().WithMessage(
    @"CSharp7: Issue(s) expected but not raised in file(s):
File: File.cs
Line: 1, Type: primary, Id: ''
");
        }

        [TestMethod]
        public void Verify_TestProject()
        {
            var builder = new VerifierBuilder<DoNotWriteToStandardOutput>() // Rule with scope Main
                .AddSnippet("public class Sample { public void Main() { System.Console.WriteLine(); } }");
            builder.Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>();
            builder.AddTestReference().Invoking(x => x.Verify()).Should().NotThrow("Project references should be recognized as Test code.");
        }

        [TestMethod]
        public void Verify_ParseOptions()
        {
            var builder = WithSnippetCS(
@"public class Sample
{
    private System.Exception ex = new(); // C# 9 target-typed new
}");
            builder.WithOptions(ParseOptionsHelper.FromCSharp9).Invoking(x => x.Verify()).Should().NotThrow();
            builder.WithOptions(ParseOptionsHelper.BeforeCSharp9).Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>()
                .WithMessage("CSharp5: Unexpected build error [CS8026]: Feature 'target-typed object creation' is not available in C# 5. Please use language version 9.0 or greater. on line 3");
        }

        [TestMethod]
        public void Verify_BasePath()
        {
            DummyCS.AddPaths("Nonexistent.cs").Invoking(x => x.Verify()).Should().Throw<FileNotFoundException>("This file should not exist in TestCases directory.");
            DummyCS.AddPaths("ArrayCovariance.cs").Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>("File should be found in TestCases directory.");
            DummyCS.WithBasePath("TestFramework").AddPaths("Verifier.BasePath.cs").Invoking(x => x.Verify()).Should().NotThrow();
        }

        [TestMethod]
        public void Verify_ErrorBehavior()
        {
            var builder = WithSnippetCS("undefined");
            builder.Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>()
                .WithMessage("CSharp7: Unexpected build error [CS0116]: A namespace cannot directly contain members such as fields, methods or statements on line 1");
            builder.WithErrorBehavior(CompilationErrorBehavior.FailTest).Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>()
                .WithMessage("CSharp7: Unexpected build error [CS0116]: A namespace cannot directly contain members such as fields, methods or statements on line 1");
            builder.WithErrorBehavior(CompilationErrorBehavior.Ignore).Invoking(x => x.Verify()).Should().NotThrow();
        }

        [TestMethod]
        public void Verify_OnlyDiagnostics()
        {
            var builder = new VerifierBuilder<SymbolicExecutionRunner>().AddPaths(WriteFile("File.cs",
@"public class Sample
{
    public void Method()
    {
        var t = true;
        if (t)          // S2583
            t = true;
        else
            t = true;
        if (t)          // S2589
            t = true;
    }
}"));
            builder.Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>().WithMessage(
@"CSharp7: Unexpected primary issue on line 6, span (5,12)-(5,13) with message 'Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.'.*");
            builder.WithOnlyDiagnostics(ConditionEvaluatesToConstant.S2589).Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>().WithMessage(
@"CSharp7: Unexpected primary issue on line 10, span (9,12)-(9,13) with message 'Change this condition so that it does not always evaluate to 'true'.'*");
            builder.WithOnlyDiagnostics(NullPointerDereference.S2259).Invoking(x => x.Verify()).Should().NotThrow();
        }

        [TestMethod]
        public void Verify_NonConcurrentAnalysis()
        {
            var builder = WithSnippetCS("var topLevelStatement = true;").WithOptions(ParseOptionsHelper.FromCSharp9).WithOutputKind(OutputKind.ConsoleApplication);
            builder.Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>("Default Verifier behavior duplicates the source file.")
                .WithMessage("CSharp9: Unexpected build error [CS0825]: The contextual keyword 'var' may only appear within a local variable declaration or in script code on line 1");
            builder.WithConcurrentAnalysis(false).Invoking(x => x.Verify()).Should().NotThrow();
        }

        [TestMethod]
        public void Verify_OutputKind()
        {
            var builder = WithSnippetCS("var topLevelStatement = true;").WithOptions(ParseOptionsHelper.FromCSharp9);
            builder.WithTopLevelStatements().Invoking(x => x.Verify()).Should().NotThrow();
            builder.WithOutputKind(OutputKind.ConsoleApplication).WithConcurrentAnalysis(false).Invoking(x => x.Verify()).Should().NotThrow();
            builder.Invoking(x => x.Verify()).Should().Throw<UnexpectedDiagnosticException>()
                .WithMessage("CSharp9: Unexpected build error [CS8805]: Program using top-level statements must be an executable. on line 1");
        }

        [TestMethod]
        public void Verify_Snippets() =>
            DummyCS.AddSnippet("public class First { } // Noncompliant [first]  - not raised")
                .AddSnippet("public class Second { } // Noncompliant [second] - not raised")
                .Invoking(x => x.Verify()).Should().Throw<AssertFailedException>().WithMessage(
@"CSharp7: Issue(s) expected but not raised in file(s):
File: snippet1.cs
Line: 1, Type: primary, Id: 'first'

File: snippet2.cs
Line: 1, Type: primary, Id: 'second'
");

        [TestMethod]
        public void VerifyCodeFix_FixExpected_CS()
        {
            var originalPath = WriteFile("File.cs",
@"public class Sample
{
    private int a = 0;     // Noncompliant
    private int b = 0;     // Noncompliant
    private bool c = true;
}");
            var fixedPath = WriteFile("File.Fixed.cs",
@"public class Sample
{
    private int a = default;     // Fixed
    private int b = default;     // Fixed
    private bool c = true;
}");
            DummyCS.AddPaths(originalPath).WithCodeFix<DummyCodeFixCS>().WithCodeFixedPaths(fixedPath).Invoking(x => x.VerifyCodeFix()).Should().NotThrow();
        }

        [TestMethod]
        public void VerifyCodeFix_FixExpected_VB()
        {
            var originalPath = WriteFile("File.vb",
@"Public Class Sample
    Private A As Integer = 42   ' Noncompliant
    Private B As Integer = 42   ' Noncompliant
    Private C As Boolean = True
End Class");
            var fixedPath = WriteFile("File.Fixed.vb",
@"Public Class Sample
    Private A As Integer = Nothing   ' Fixed
    Private B As Integer = Nothing   ' Fixed
    Private C As Boolean = True
End Class");
            DummyVB.AddPaths(originalPath).WithCodeFix<DummyCodeFixVB>().WithCodeFixedPaths(fixedPath).Invoking(x => x.VerifyCodeFix()).Should().NotThrow();
        }

        [TestMethod]
        public void VerifyCodeFix_NotFixed_CS()
        {
            var originalPath = WriteFile("File.cs",
@"public class Sample
{
    private int a = 0;     // Noncompliant
    private int b = 0;     // Noncompliant
    private bool c = true;
}");
            DummyCS.AddPaths(originalPath).WithCodeFix<DummyCodeFixCS>().WithCodeFixedPaths(originalPath).Invoking(x => x.VerifyCodeFix()).Should().Throw<AssertFailedException>().WithMessage(
@"Expected * to be*
""public class Sample
{
    private int a = 0;     // Noncompliant
    private int b = 0;     // Noncompliant
    private bool c = true;
}"" with a length of 136 because VerifyWhileDocumentChanges updates the document until all issues are fixed, even if the fix itself creates a new issue again. Language: CSharp7, but*
""public class Sample
{
    private int a = default;     // Fixed
    private int b = default;     // Fixed
    private bool c = true;
}"" has a length of 134, differs near ""def"" (index 42).");
        }

        [TestMethod]
        public void VerifyCodeFix_NotFixed_VB()
        {
            var originalPath = WriteFile("File.vb",
@"Public Class Sample
    Private A As Integer = 42   ' Noncompliant
    Private B As Integer = 42   ' Noncompliant
    Private C As Boolean = True
End Class");
            DummyVB.AddPaths(originalPath).WithCodeFix<DummyCodeFixVB>().WithCodeFixedPaths(originalPath).Invoking(x => x.VerifyCodeFix()).Should().Throw<AssertFailedException>().WithMessage(
@"Expected * to be*
""Public Class Sample
    Private A As Integer = 42   ' Noncompliant
    Private B As Integer = 42   ' Noncompliant
    Private C As Boolean = True
End Class"" with a length of 155 because VerifyWhileDocumentChanges updates the document until all issues are fixed, even if the fix itself creates a new issue again. Language: VisualBasic12, but*
""Public Class Sample
    Private A As Integer = Nothing   ' Fixed
    Private B As Integer = Nothing   ' Fixed
    Private C As Boolean = True
End Class"" has a length of 151, differs near ""Not"" (index 47).");
        }

        [TestMethod]
        public void VerifyNoIssueReported_NoIssues_Succeeds() =>
            WithSnippetCS("// Noncompliant - this comment is ignored").Invoking(x => x.VerifyNoIssueReported()).Should().NotThrow();

        [TestMethod]
        public void VerifyNoIssueReported_WithIssues_Throws() =>
            WithSnippetCS(
@"public class Sample
{
    private int a = 42;     // This will raise an issue
}").Invoking(x => x.VerifyNoIssueReported()).Should().Throw<AssertFailedException>();

        [TestMethod]
        public void Verify_ConcurrentAnalysis_FileEndingWithComment_CS() =>
            WithSnippetCS("// Nothing to see here, file ends with a comment").Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void Verify_ConcurrentAnalysis_FileEndingWithComment_VB() =>
            WithSnippetVB("' Nothing to see here, file ends with a comment").Invoking(x => x.Verify()).Should().NotThrow();

        [TestMethod]
        public void VerifyUtilityAnalyzerProducesEmptyProtobuf_EmptyFile()
        {
            var protobufPath = TestHelper.TestPath(TestContext, "Empty.pb");
            new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerCS(protobufPath, null)).AddSnippet("// Nothing to see here").WithProtobufPath(protobufPath)
                .Invoking(x => x.VerifyUtilityAnalyzerProducesEmptyProtobuf())
                .Should().NotThrow();
        }

        [TestMethod]
        public void VerifyUtilityAnalyzerProducesEmptyProtobuf_WithContent()
        {
            var protobufPath = TestHelper.TestPath(TestContext, "Empty.pb");
            var message = new LogInfo { Text = "Lorem Ipsum" };
            new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerCS(protobufPath, message)).AddSnippet("// Nothing to see here").WithProtobufPath(protobufPath)
                .Invoking(x => x.VerifyUtilityAnalyzerProducesEmptyProtobuf())
                .Should().Throw<AssertFailedException>().WithMessage("Expected value to be 0L because protobuf file should be empty, but found *");
        }

        [TestMethod]
        public void VerifyUtilityAnalyzer_CorrectProtobuf_CS()
        {
            var protobufPath = TestHelper.TestPath(TestContext, "Log.pb");
            var message = new LogInfo { Text = "Lorem Ipsum" };
            var wasInvoked = false;
            new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerCS(protobufPath, message)).AddSnippet("// Nothing to see here").WithProtobufPath(protobufPath)
                .VerifyUtilityAnalyzer<LogInfo>(x =>
                {
                    x.Should().ContainSingle().Which.Text.Should().Be("Lorem Ipsum");
                    wasInvoked = true;
                });
            wasInvoked.Should().BeTrue();
        }

        [TestMethod]
        public void VerifyUtilityAnalyzer_CorrectProtobuf_VB()
        {
            var protobufPath = TestHelper.TestPath(TestContext, "Log.pb");
            var message = new LogInfo { Text = "Lorem Ipsum" };
            var wasInvoked = false;
            new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerVB(protobufPath, message)).AddSnippet("' Nothing to see here").WithProtobufPath(protobufPath)
                .VerifyUtilityAnalyzer<LogInfo>(x =>
                {
                    x.Should().ContainSingle().Which.Text.Should().Be("Lorem Ipsum");
                    wasInvoked = true;
                });
            wasInvoked.Should().BeTrue();
        }

        [TestMethod]
        public void VerifyUtilityAnalyzer_VerifyProtobuf_PropagateFailedAssertion_CS()
        {
            var protobufPath = TestHelper.TestPath(TestContext, "Empty.pb");
            new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerCS(protobufPath, null)).AddSnippet("// Nothing to see here").WithProtobufPath(protobufPath)
                .Invoking(x => x.VerifyUtilityAnalyzer<LogInfo>(x => throw new AssertFailedException("Some failed assertion about Protobuf")))
                .Should().Throw<AssertFailedException>().WithMessage("Some failed assertion about Protobuf");
        }

        [TestMethod]
        public void VerifyUtilityAnalyzer_VerifyProtobuf_PropagateFailedAssertion_VB()
        {
            var protobufPath = TestHelper.TestPath(TestContext, "Empty.pb");
            new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerVB(protobufPath, null)).AddSnippet("' Nothing to see here").WithProtobufPath(protobufPath)
                .Invoking(x => x.VerifyUtilityAnalyzer<LogInfo>(x => throw new AssertFailedException("Some failed assertion about Protobuf")))
                .Should().Throw<AssertFailedException>().WithMessage("Some failed assertion about Protobuf");
        }

        private VerifierBuilder WithSnippetCS(string code) =>
            DummyCS.AddPaths(WriteFile("File.cs", code));

        private VerifierBuilder WithSnippetVB(string code) =>
            DummyVB.AddPaths(WriteFile("File.vb", code));

        private string WriteFile(string name, string content) =>
            TestHelper.WriteFile(TestContext, name, content);
    }
}
