﻿/*
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

using System.IO;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Core.Configuration;
using SonarAnalyzer.CSharp.Rules;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.TestFramework.Verification;
using static SonarAnalyzer.TestFramework.Verification.Verifier;

namespace SonarAnalyzer.Test.TestFramework.Tests.Verification;

[TestClass]
public class VerifierTest
{
    private static readonly VerifierBuilder DummyCS = new VerifierBuilder<DummyAnalyzerCS>();
    private static readonly VerifierBuilder DummyVB = new VerifierBuilder<DummyAnalyzerVB>();
    private static readonly VerifierBuilder DummyCodeFixCS = new VerifierBuilder<DummyAnalyzerCS>()
        .AddPaths("Path.cs")
        .WithCodeFix<DummyCodeFixCS>()
        .WithCodeFixedPaths("Expected.cs");

    private static readonly VerifierBuilder DummyWithLocation = new VerifierBuilder<DummyAnalyzerWithLocation>();

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
        DummyCS.AddAnalyzer(() => new SonarAnalyzer.VisualBasic.Rules.OptionStrictOn())
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

#if NET

    [TestMethod]
    public void Verify_RazorWithAssociatedCS() =>
        DummyCS.AddPaths(WriteFile("File.razor", """<p @bind="pValue">Dynamic content</p>"""))
            .AddPaths(WriteFile("File.razor.cs", """public partial class File { string pValue = "The value bound"; int a = 42;  }"""))
            .Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>();

    [TestMethod]
    public void Verify_RazorWithUnrelatedCS() =>
        DummyCS.AddPaths(WriteFile("File.razor", """<p @bind="pValue">Dynamic content</p>"""))
            .AddPaths(WriteFile("SomeSource.cs", """class SomeSource { int a = 42; }"""))
            .Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>();

    [TestMethod]
    public void Verify_RazorWithUnrelatedIssues() =>
        DummyCS.AddPaths(WriteFile("File.razor", """<p @bind="pValue">Dynamic content</p>"""))
            .AddPaths(WriteFile("SomeSource.cs", """class SomeSource { int a = 42; }"""))
            .AddPaths(WriteFile("Sample.cs", """
                public class Sample
                {
                    private int a = 42;     // Noncompliant {{Message for SDummy}}
                    private int b = 42;     // Noncompliant
                    private bool c = true;
                }
                """))
            .Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>();

    [TestMethod]
    [DataRow("Dummy.SecondaryLocation.razor")]
    [DataRow("Dummy.SecondaryLocation.cshtml")]
    public void Verify_RazorWithAdditionalLocation(string path) =>
        DummyWithLocation.AddPaths(path)
            .WithOptions(LanguageOptions.BeforeCSharp10)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    [DataRow("Dummy.SecondaryLocation.CSharp10.razor")]
    [DataRow("Dummy.SecondaryLocation.cshtml")]
    public void Verify_RazorWithAdditionalLocation_CSharp10(string path) =>
        DummyWithLocation.AddPaths(path)
            .WithOptions(LanguageOptions.FromCSharp10)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    [DataRow("Dummy.razor")]
    [DataRow("Dummy.cshtml")]
    public void Verify_Razor(string path) =>
        DummyWithLocation.AddPaths(path)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    [DataRow("DummyExpressions.razor")]
    [DataRow("DummyExpressions.cshtml")]
    public void Verify_RazorExpressions_Locations(string path) =>
        DummyWithLocation
            .AddPaths(path)
            .WithOptions(LanguageOptions.BeforeCSharp10)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    [DataRow("DummyExpressions.CSharp10.razor")]
    [DataRow("DummyExpressions.cshtml")]
    public void Verify_RazorExpressions_Locations_CSharp10(string path) =>
        DummyWithLocation
            .AddPaths(path)
            .WithOptions(LanguageOptions.FromCSharp10)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();

    [TestMethod]
    [DataRow("Dummy.razor")]
    [DataRow("Dummy.cshtml")]
    public void Verify_RazorAnalysisIsDisabled_DoesNotRaise(string path) =>
        DummyWithLocation.AddPaths(path)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarLintXml(TestContext, analyzeRazorCode: false))
            .VerifyNoIssues();

    [TestMethod]
    [DataRow("Dummy.razor")]
    [DataRow("Dummy.cshtml")]
    public void Verify_RazorAnalysisInSLAndNugetContext(string path) =>
        DummyWithLocation.AddPaths(path)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Unknown))
            .Verify();

    [TestMethod]
    public void Compile_Razor_DefaultFramework()
    {
        var compilation = DummyWithLocation.AddPaths("Dummy.razor")
            .AddSnippet("class Sample(object forceTheTypeToBeUsed) { }")
            .WithLanguageVersion(LanguageVersion.CSharp12)
            .Build()
            .Compile(false)
            .Single();
        compilation.Compilation.GetSpecialType(SpecialType.System_Object).ContainingAssembly.Identity.Version.Major.Should().Be(9, "This version is the default framework for in-memory compilation");

        compilation.Compilation.ExternalReferences.Select(x => Path.GetFileName(x.Display)).Should().Contain([
            "Microsoft.AspNetCore.dll",
            "Microsoft.AspNetCore.Components.dll",
            "Microsoft.AspNetCore.Components.Web.dll",
            "System.Text.Encodings.Web.dll"
        ]);
    }

    [TestMethod]
    public void Compile_Razor_AddReferences()
    {
        var compilations = DummyWithLocation.AddPaths("Dummy.razor")
            .AddReferences(NuGetMetadataReference.MicrosoftAzureDocumentDB())
            .WithLanguageVersion(LanguageVersion.Latest)
            .Build()
            .Compile(false);
        var references = compilations.Single().Compilation.References;
        references.Should().Contain(x => x.Display.Contains("Microsoft.Azure.DocumentDB"));
    }

    [TestMethod]
    public void Compile_Razor_NoReferences()
    {
        var compilations = DummyWithLocation.AddPaths("Dummy.razor")
            .WithLanguageVersion(LanguageVersion.Latest)
            .Build()
            .Compile(false);
        var references = compilations.Single().Compilation.References;
        references.Should().NotContain(x => x.Display.Contains("Microsoft.Azure.DocumentDB"));
    }

    [TestMethod]
    public void Compile_Razor_Snippet()
    {
        var compilation = DummyWithLocation
            .AddSnippet("""
                <p>@Counter</p>

                @code {
                    int Counter = 0;
                }
                """, "snippet.razor")
            .WithLanguageVersion(LanguageVersion.Latest)
            .Build()
            .Compile(false)
            .Single();

        compilation.Compilation.SyntaxTrees.Should().ContainSingle();
        ContainsSyntaxTreeWithName(compilation, "snippet_razor.g.cs");
    }

    [TestMethod]
    public void Compile_Cshtml_Snippet()
    {
        var compilation = DummyWithLocation
            .AddSnippet("""
                @{ var total = 7; }
                <p>@total</p>
                """, "snippet.cshtml")
            .WithLanguageVersion(LanguageVersion.Latest)
            .Build()
            .Compile(false)
            .Single();

        compilation.Compilation.SyntaxTrees.Should().ContainSingle();
        ContainsSyntaxTreeWithName(compilation, "snippet_cshtml.g.cs");
    }

    [TestMethod]
    public void Compile_Razor_Snippet_NoName()
    {
        var compilation = DummyWithLocation
            .AddPaths("Dummy.cshtml")
            .AddSnippet("class Sample {}")
            .WithLanguageVersion(LanguageVersion.Latest)
            .Build()
            .Compile(false)
            .Single();

        ContainsSyntaxTreeWithName(compilation, "Dummy_cshtml.g.cs");
        ContainsSyntaxTreeWithName(compilation, "snippet0.cs");
    }

    [TestMethod]
    public void Compile_Razor_Snippet_Name_CSharp()
    {
        var compilation = DummyWithLocation
            .AddPaths("Dummy.cshtml")
            .AddSnippet("class Sample {}", "snippet.cs")
            .WithLanguageVersion(LanguageVersion.Latest)
            .Build()
            .Compile(false)
            .Single();

        ContainsSyntaxTreeWithName(compilation, "Dummy_cshtml.g.cs");
        ContainsSyntaxTreeWithName(compilation, "snippet.cs");
    }

    [TestMethod]
    public void Compile_Razor_Snippet_Name_Cshtml()
    {
        var compilation = DummyWithLocation
            .AddPaths("Dummy.cshtml")
            .AddSnippet("class Sample {}", "snippet.cshtml")
            .WithLanguageVersion(LanguageVersion.Latest)
            .Build()
            .Compile(false)
            .Single();

        ContainsSyntaxTreeWithName(compilation, "Dummy_cshtml.g.cs");
        ContainsSyntaxTreeWithName(compilation, "snippet_cshtml.g.cs");
    }

    [TestMethod]
    public void Compile_Razor_Snippet_MixedNames()
    {
        var compilation = DummyWithLocation
            .AddPaths("Dummy.cshtml")
            .AddSnippet("class Sample {}")                     // No name, .0.cs
            .AddSnippet("class Sample {}")                     // No name, .1.cs
            .AddSnippet("class Sample {}", "snippet.cs")       // Named, c#
            .AddSnippet("class Sample {}", "snippet.cshtml")   // Named, c#
            .WithLanguageVersion(LanguageVersion.Latest)
            .Build()
            .Compile(false)
            .Single();

        ContainsSyntaxTreeWithName(compilation, "Dummy_cshtml.g.cs");
        ContainsSyntaxTreeWithName(compilation, "snippet_cshtml.g.cs");
        ContainsSyntaxTreeWithName(compilation, "snippet0.cs");
        ContainsSyntaxTreeWithName(compilation, "snippet1.cs");
        ContainsSyntaxTreeWithName(compilation, "snippet.cs");
    }

#endif

    [TestMethod]
    public void Verify_ThrowsWithCodeFixSet()
    {
        var originalPath = WriteFile("File.cs", null);
        var fixedPath = WriteFile("File.Fixed.cs", null);
        DummyCS.AddPaths(originalPath).WithCodeFix<DummyCodeFixCS>().WithCodeFixedPaths(fixedPath).Invoking(x => x.Verify())
            .Should().Throw<InvalidOperationException>().WithMessage("Cannot use Verify with CodeFix set.");
    }

    [TestMethod]
    public void Verify_RaiseExpectedIssues_CS() =>
        WithSnippetCS("""
            public class Sample
            {
                private int a = 42;     // Noncompliant {{Message for SDummy}}
                private int b = 42;     // Noncompliant
                private bool c = true;
            }
            """).Invoking(x => x.Verify()).Should().NotThrow();

    [TestMethod]
    public void Verify_RaiseExpectedIssues_VB() =>
        WithSnippetVB("""
            Public Class Sample
                Private A As Integer = 42   ' Noncompliant {{Message for SDummy}}
                Private B As Integer = 42   ' Noncompliant
                Private C As Boolean = True
            End Class
            """).Invoking(x => x.Verify()).Should().NotThrow();

    [TestMethod]
    public void Verify_RaiseUnexpectedIssues_CS() =>
        WithSnippetCS("""
            public class Sample
            {
                private int a = 42;     // FP
                private int b = 42;     // FP
                private bool c = true;
            }
            """).Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 3: Unexpected issue 'Message for SDummy' Rule SDummy
              Line 4: Unexpected issue 'Message for SDummy' Rule SDummy

            There are 2 more differences in File.Concurrent.cs

            """);

    [TestMethod]
    public void Verify_RaiseUnexpectedIssues_VB() =>
        WithSnippetVB("""
            Public Class Sample
                Private A As Integer = 42   ' FP
                Private B As Integer = 42   ' FP
                Private C As Boolean = True
            End Class
            """).Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for VisualBasic12 File.vb:
              Line 2: Unexpected issue 'Message for SDummy' Rule SDummy
              Line 3: Unexpected issue 'Message for SDummy' Rule SDummy

            There are 2 more differences in File.Concurrent.vb

            """);

    [TestMethod]
    public void Verify_MissingExpectedIssues() =>
        WithSnippetCS("""
            public class Sample
            {
                private bool a = true;   // Noncompliant - FN
                private bool b = true;   // Noncompliant - FN
                private bool c = true;
            }
            """).Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 3: Missing expected issue
              Line 4: Missing expected issue

            There are 2 more differences in File.Concurrent.cs

            """);

    [TestMethod]
    public void Verify_TwoAnalyzers() =>
        WithSnippetCS("""
            public class Sample
            {
                private int a = 42;     // Noncompliant {{Message for SDummy}}
                                        // Noncompliant@-1
                private int b = 42;     // Noncompliant
                                        // Noncompliant@-1
                private bool c = true;
            }
            """)
        .AddAnalyzer(() => new DummyAnalyzerCS()) // Duplicate
        .Invoking(x => x.Verify()).Should().NotThrow();

    [TestMethod]
    public void Verify_TwoPaths() =>
        WithSnippetCS("""
            public class First
            {
                private bool a = true;     // Noncompliant - FN in File.cs
            }
            """)
        .AddPaths(WriteFile("Second.cs", """
            public class Second
            {
                private bool a = true;     // Noncompliant - FN in Second.cs
            }
            """))
        .WithConcurrentAnalysis(false)
        .Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 3: Missing expected issue

            There are differences for CSharp7 Second.cs:
              Line 3: Missing expected issue
            """);

    [TestMethod]
    public void Verify_AutogenerateConcurrentFiles()
    {
        var builder = WithSnippetCS("// Noncompliant - FN");
        // Concurrent analysis by-default automatically generates concurrent files - File.Concurrent.cs
        builder.Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 1: Missing expected issue

            There is 1 more difference in File.Concurrent.cs
            """);
        // When AutogenerateConcurrentFiles is turned off, only the provided snippet is analyzed
        builder.WithAutogenerateConcurrentFiles(false).Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 1: Missing expected issue

            """);
    }

    [TestMethod]
    public void Verify_TestProject()
    {
        var builder = new VerifierBuilder<DoNotWriteToStandardOutput>() // Rule with scope Main
            .AddSnippet("class Sample { void Main() { System.Console.WriteLine(); } }  // Noncompliant");
        builder.Invoking(x => x.Verify()).Should().NotThrow();
        builder.AddTestReference().Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>("project references should be recognized as Test code").WithMessage("""
            There are differences for CSharp7 snippet0.cs:
              Line 1: Missing expected issue
            """);
    }

    [TestMethod]
    public void Verify_ParseOptions()
    {
        var builder = WithSnippetCS("""
            class Sample
            {
                int i = 42;                     // Noncompliant
                System.Exception ex = new();    // C# 9 target-typed new
            }
            """);
        builder.WithOptions(LanguageOptions.FromCSharp9).Invoking(x => x.Verify()).Should().NotThrow();
        builder.WithOptions(LanguageOptions.BeforeCSharp9).Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp5 File.cs:
              Line 4: Unexpected error, use // Error [CS8026] Feature 'target-typed object creation' is not available in C# 5. Please use language version 9.0 or greater.

            There is 1 more difference in File.Concurrent.cs
            """);
    }

    [TestMethod]
    public void Verify_BasePath()
    {
        DummyCS.AddPaths("Nonexistent.cs").Invoking(x => x.Verify()).Should().Throw<FileNotFoundException>("This file should not exist in TestCases directory.");
        DummyCS.AddPaths("Verifier.BasePathAssertFails.cs").Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>("File should be found in TestCases directory.");
        DummyCS.WithBasePath("Verifier").AddPaths("Verifier.BasePath.cs").Invoking(x => x.Verify()).Should().NotThrow();
    }

    [TestMethod]
    public void Verify_ErrorBehavior()
    {
        var builder = WithSnippetCS("""
            class Sample
            {
                int a = 42; // Noncompliant
            }
            undefined
            """);
        builder.Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>()
            .WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 5: Unexpected error, use // Error [CS0246] The type or namespace name 'undefined' could not be found (are you missing a using directive or an assembly reference?)
              Line 5: Unexpected error, use // Error [CS8107] Feature 'top-level statements' is not available in C# 7.0. Please use language version 9.0 or greater.
              Line 5: Unexpected error, use // Error [CS8803] Top-level statements must precede namespace and type declarations.
              Line 5: Unexpected error, use // Error [CS8805] Program using top-level statements must be an executable.
              Line 5: Unexpected error, use // Error [CS1001] Identifier expected
              Line 5: Unexpected error, use // Error [CS1002] ; expected

            There is 1 more difference in File.Concurrent.cs

            """);
        builder.WithErrorBehavior(CompilationErrorBehavior.FailTest).Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 5: Unexpected error, use // Error [CS0246] The type or namespace name 'undefined' could not be found (are you missing a using directive or an assembly reference?)
              Line 5: Unexpected error, use // Error [CS8107] Feature 'top-level statements' is not available in C# 7.0. Please use language version 9.0 or greater.
              Line 5: Unexpected error, use // Error [CS8803] Top-level statements must precede namespace and type declarations.
              Line 5: Unexpected error, use // Error [CS8805] Program using top-level statements must be an executable.
              Line 5: Unexpected error, use // Error [CS1001] Identifier expected
              Line 5: Unexpected error, use // Error [CS1002] ; expected

            There is 1 more difference in File.Concurrent.cs

            """);
        builder.WithErrorBehavior(CompilationErrorBehavior.Ignore).Invoking(x => x.Verify()).Should().NotThrow();
    }

    [TestMethod]
    public void Verify_OnlyDiagnostics()
    {
        var builder = new VerifierBuilder<ObsoleteAttributes>().AddPaths(WriteFile("File.cs", "[System.Obsolete]public class Sample { }"));
        builder.Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 1: Unexpected issue 'Add an explanation.' Rule S1123
              Line 1: Unexpected issue 'Do not forget to remove this deprecated code someday.' Rule S1133

            There are 2 more differences in File.Concurrent.cs

            """);
        builder.WithOnlyDiagnostics(AnalysisScaffolding.CreateDescriptor("S1123")).Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp7 File.cs:
              Line 1: Unexpected issue 'Add an explanation.' Rule S1123

            There is 1 more difference in File.Concurrent.cs

            """);
        builder.WithOnlyDiagnostics(AnalysisScaffolding.CreateDescriptor("S0000")).Invoking(x => x.VerifyNoIssues()).Should().NotThrow();
    }

    [TestMethod]
    public void Verify_NonConcurrentAnalysis()
    {
        var builder = WithSnippetCS("var topLevelStatement = 42;  // Noncompliant").WithOptions(LanguageOptions.FromCSharp9).WithOutputKind(OutputKind.ConsoleApplication);
        builder.Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>("Default Verifier behavior duplicates the source file.").WithMessage("""
            There are differences for CSharp9 File.Concurrent.cs:
              Line 1: Unexpected error, use // Error [CS0825] The contextual keyword 'var' may only appear within a local variable declaration or in script code
              Line 1: Unexpected error, use // Error [CS0116] A namespace cannot directly contain members such as fields, methods or statements
            """);
        builder.WithConcurrentAnalysis(false).Invoking(x => x.Verify()).Should().NotThrow();
    }

    [TestMethod]
    public void Verify_OutputKind()
    {
        var builder = WithSnippetCS("var topLevelStatement = 42;  // Noncompliant").WithOptions(LanguageOptions.FromCSharp9);
        builder.WithTopLevelStatements().Invoking(x => x.Verify()).Should().NotThrow();
        builder.WithOutputKind(OutputKind.ConsoleApplication).WithConcurrentAnalysis(false).Invoking(x => x.Verify()).Should().NotThrow();
        builder.Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
            There are differences for CSharp9 File.cs:
              Line 1: Unexpected error, use // Error [CS8805] Program using top-level statements must be an executable.

            There are 2 more differences in File.Concurrent.cs

            """);
    }

    [TestMethod]
    public void Verify_Snippets() =>
        DummyCS.AddSnippet("public class First { } // Noncompliant [first]  - not raised")
            .AddSnippet("public class Second { } // Noncompliant [second] - not raised")
            .Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
                There are differences for CSharp7 snippet0.cs:
                  Line 1: Missing expected issue ID first

                There are differences for CSharp7 snippet1.cs:
                  Line 1: Missing expected issue ID second
                """);

    [TestMethod]
    public void VerifyCodeFix_NoCodeFix() =>
        DummyCS.AddSnippet("// Nothing to see here").Invoking(x => x.VerifyCodeFix()).Should().Throw<InvalidOperationException>().WithMessage("CodeFix was not set.");

    [TestMethod]
    public void VerifyCodeFix_FixExpected_CS()
    {
        var originalPath = WriteFile("File.cs", """
            public class Sample
            {
                private int a = 0;     // Noncompliant
                private int b = 0;     // Noncompliant
                private bool c = true;
            }
            """);
        var fixedPath = WriteFile("File.Fixed.cs", """
            public class Sample
            {
                private int a = default;     // Fixed
                private int b = default;     // Fixed
                private bool c = true;
            }
            """);
        DummyCS.AddPaths(originalPath).WithCodeFix<DummyCodeFixCS>().WithCodeFixedPaths(fixedPath).Invoking(x => x.VerifyCodeFix()).Should().NotThrow();
    }

    [TestMethod]
    public void VerifyCodeFix_FixExpected_VB()
    {
        var originalPath = WriteFile("File.vb", """
            Public Class Sample
                Private A As Integer = 42   ' Noncompliant
                Private B As Integer = 42   ' Noncompliant
                Private C As Boolean = True
            End Class
            """);
        var fixedPath = WriteFile("File.Fixed.vb", """
            Public Class Sample
                Private A As Integer = Nothing   ' Fixed
                Private B As Integer = Nothing   ' Fixed
                Private C As Boolean = True
            End Class
            """);
        DummyVB.AddPaths(originalPath).WithCodeFix<DummyCodeFixVB>().WithCodeFixedPaths(fixedPath).Invoking(x => x.VerifyCodeFix()).Should().NotThrow();
    }

    [TestMethod]
    public void VerifyCodeFix_NotFixed_CS()
    {
        var originalPath = WriteFile("File.cs", """
            public class Sample
            {
                private int a = 0;     // Noncompliant
                private int b = 0;     // Noncompliant
                private bool c = true;
            }
            """);
        DummyCS.AddPaths(originalPath).WithCodeFix<DummyCodeFixCS>().WithCodeFixedPaths(originalPath).Invoking(x => x.VerifyCodeFix()).Should().Throw<AssertionFailedException>().WithMessage("""
            Expected actual to be the same string because VerifyWhileDocumentChanges updates the document until all issues are fixed, even if the fix itself creates a new issue again. Language: CSharp7, but they differ on line 3 and column 21 (index 42):
                        ↓ (actual)
              "…int a = default;     //…"
              "…int a = 0;     //…"
                        ↑ (expected).
            """);
    }

    [TestMethod]
    public void VerifyCodeFix_NotFixed_VB()
    {
        var originalPath = WriteFile("File.vb", """
            Public Class Sample
                Private A As Integer = 42   ' Noncompliant
                Private B As Integer = 42   ' Noncompliant
                Private C As Boolean = True
            End Class
            """);
        DummyVB.AddPaths(originalPath).WithCodeFix<DummyCodeFixVB>().WithCodeFixedPaths(originalPath).Invoking(x => x.VerifyCodeFix()).Should().Throw<AssertionFailedException>().WithMessage("""
            Expected actual to be the same string because VerifyWhileDocumentChanges updates the document until all issues are fixed, even if the fix itself creates a new issue again. Language: VisualBasic12, but they differ on line 2 and column 28 (index 47):
                               ↓ (actual)
              "…A As Integer = Nothing  …"
              "…A As Integer = 42   '…"
                               ↑ (expected).
            """);
    }

    [TestMethod]
    public void VerifyCodeFix_NoIssueRaised()
    {
        var path = WriteFile("File.cs", "// Nothing to see here");
        DummyCS.AddPaths(path).WithCodeFix<DummyCodeFixCS>().WithCodeFixedPaths(path).Invoking(x => x.VerifyCodeFix()).Should().Throw<AssertionFailedException>()
            .WithMessage("Expected state.Diagnostics not to be empty.");
    }

    [TestMethod]
    public void VerifyNoIssues_NoIssues_Succeeds() =>
        WithSnippetCS("// Noncompliant - this comment is ignored").Invoking(x => x.VerifyNoIssues()).Should().NotThrow();

    [TestMethod]
    public void VerifyNoIssues_WithIssues_Throws() =>
        WithSnippetCS("""
            public class Sample
            {
                private int a = 42;     // Noncompliant
            }
            """).Invoking(x => x.VerifyNoIssues()).Should().Throw<AssertionFailedException>();

    [TestMethod]
    public void VerifyNoIssues_InvalidCode_Throws() =>
        WithSnippetCS("Nonsense").Invoking(x => x.VerifyNoIssues()).Should().Throw<AssertionFailedException>();

    [TestMethod]
    public void VerifyNoAD0001_AnalyzerException_Fail() =>
        new VerifierBuilder<DummyAnalyzerThatThrowsCS>()
            .AddSnippet("""
                        public class Class7
                        {
                            public int X = 7;
                        }
                        """)
            .Invoking(x => x.VerifyNoAD0001())
            .Should().Throw<AssertionFailedException>();

    [TestMethod]
    public void VerifyNoAD0001_NoAD0001_DoesNotFail() =>
        new VerifierBuilder<DummyAnalyzerCS>()
            .AddSnippet("""
                        public class Class7
                        {
                            public int X = 7;
                        }
                        """)
            .VerifyNoAD0001();

    [TestMethod]
    public void VerifyNoIssuesIgnoreErrors_NoIssues_Succeeds() =>
        WithSnippetCS("// Noncompliant - this comment is ignored").Invoking(x => x.VerifyNoIssuesIgnoreErrors()).Should().NotThrow();

    [TestMethod]
    public void VerifyNoIssuesIgnoreErrors_WithIssues_Throws() =>
        WithSnippetCS("""
            public class Sample
            {
                private int a = 42;     // Noncompliant
            }
            """).Invoking(x => x.VerifyNoIssuesIgnoreErrors()).Should().Throw<AssertionFailedException>();

    [TestMethod]
    public void VerifyNoIssuesIgnoreErrors_InvalidCode_Throws() =>
        WithSnippetCS("Nonsense").Invoking(x => x.VerifyNoIssuesIgnoreErrors()).Should().NotThrow();

    [TestMethod]
    public void Verify_ConcurrentAnalysis_FileEndingWithComment_CS() =>
        WithSnippetCS("// Nothing to see here, file ends with a comment").Invoking(x => x.VerifyNoIssues()).Should().NotThrow();

    [TestMethod]
    public void Verify_ConcurrentAnalysis_FileEndingWithComment_VB() =>
        WithSnippetVB("' Nothing to see here, file ends with a comment").Invoking(x => x.VerifyNoIssues()).Should().NotThrow();

    [TestMethod]
    public void VerifyUtilityAnalyzerProducesEmptyProtobuf_EmptyFile()
    {
        var protobufPath = TestFiles.TestPath(TestContext, "Empty.pb");
        new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerCS(protobufPath, null)).AddSnippet("// Nothing to see here").WithProtobufPath(protobufPath)
            .Invoking(x => x.VerifyUtilityAnalyzerProducesEmptyProtobuf())
            .Should().NotThrow();
    }

    [TestMethod]
    public void VerifyUtilityAnalyzerProducesEmptyProtobuf_WithContent()
    {
        var protobufPath = TestFiles.TestPath(TestContext, "Empty.pb");
        var message = new LogInfo { Text = "Lorem Ipsum" };
        new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerCS(protobufPath, message)).AddSnippet("// Nothing to see here").WithProtobufPath(protobufPath)
            .Invoking(x => x.VerifyUtilityAnalyzerProducesEmptyProtobuf())
            .Should().Throw<AssertionFailedException>()
            .WithMessage("Expected value to be 0L because protobuf file should be empty, but found *");
    }

    [TestMethod]
    public void VerifyUtilityAnalyzer_CorrectProtobuf_CS()
    {
        var protobufPath = TestFiles.TestPath(TestContext, "Log.pb");
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
        var protobufPath = TestFiles.TestPath(TestContext, "Log.pb");
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
        var protobufPath = TestFiles.TestPath(TestContext, "Empty.pb");
        new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerCS(protobufPath, null)).AddSnippet("// Nothing to see here").WithProtobufPath(protobufPath)
            .Invoking(x => x.VerifyUtilityAnalyzer<LogInfo>(x => throw new AssertionFailedException("Some failed assertion about Protobuf")))
            .Should().Throw<AssertionFailedException>()
            .WithMessage("Some failed assertion about Protobuf");
    }

    [TestMethod]
    public void VerifyUtilityAnalyzer_VerifyProtobuf_PropagateFailedAssertion_VB()
    {
        var protobufPath = TestFiles.TestPath(TestContext, "Empty.pb");
        new VerifierBuilder().AddAnalyzer(() => new DummyUtilityAnalyzerVB(protobufPath, null)).AddSnippet("' Nothing to see here").WithProtobufPath(protobufPath)
            .Invoking(x => x.VerifyUtilityAnalyzer<LogInfo>(x => throw new AssertionFailedException("Some failed assertion about Protobuf")))
            .Should().Throw<AssertionFailedException>()
            .WithMessage("Some failed assertion about Protobuf");
    }

    private VerifierBuilder WithSnippetCS(string code) =>
        DummyCS.AddPaths(WriteFile("File.cs", code));

    private VerifierBuilder WithSnippetVB(string code) =>
        DummyVB.AddPaths(WriteFile("File.vb", code));

    private string WriteFile(string name, string content) =>
        TestFiles.WriteFile(TestContext, $@"TestCases\{name}", content);

    private static void ContainsSyntaxTreeWithName(CompilationData compilation, string suffix) =>
        compilation.Compilation.SyntaxTrees.Select(x => x.FilePath).Should().Contain(x => x.EndsWith(suffix));
}
