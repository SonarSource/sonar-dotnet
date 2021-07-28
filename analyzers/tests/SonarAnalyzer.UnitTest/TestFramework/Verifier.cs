/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public enum CompilationErrorBehavior
    {
        FailTest,
        Ignore,
        Default = FailTest
    }

    public static class Verifier
    {
        public static void VerifyNoExceptionThrown(string path, IEnumerable<DiagnosticAnalyzer> diagnosticAnalyzers, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default)
        {
            var compilation = SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddDocument(path)
                .GetCompilation();

            DiagnosticVerifier.GetAllDiagnostics(compilation, diagnosticAnalyzers, checkMode);
        }

        /// <summary>
        /// Verify analyzer from C# on a snippet. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyCSharpAnalyzer(string snippet,
                                        SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                        IEnumerable<MetadataReference> additionalReferences) =>
            VerifyCSharpAnalyzer(snippet, diagnosticAnalyzer, null, CompilationErrorBehavior.Default, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# on a snippet. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyCSharpAnalyzer(string snippet,
                                        SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                        CompilationErrorBehavior checkMode,
                                        IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyCSharpAnalyzer(snippet, diagnosticAnalyzer, null, checkMode, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# on a snippet. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyCSharpAnalyzer(string snippet,
                                                SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                IEnumerable<ParseOptions> options = null,
                                                CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                IEnumerable<MetadataReference> additionalReferences = null)
        {
            var solution = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippet(snippet).AddReferences(additionalReferences).GetSolution();
            CompileAndVerifyAnalyzer(solution, new DiagnosticAnalyzer[] { diagnosticAnalyzer }, options, checkMode);
        }

        /// <summary>
        /// Verify analyzer from VB.NET on a snippet. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyVisualBasicAnalyzer(string snippet,
                                                     DiagnosticAnalyzer diagnosticAnalyzer,
                                                     CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                     IEnumerable<MetadataReference> additionalReferences = null)
        {
            var solution = SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).AddSnippet(snippet).AddReferences(additionalReferences).GetSolution();
            CompileAndVerifyAnalyzer(solution, new[] { diagnosticAnalyzer }, null, checkMode);
        }

        /// <summary>
        /// Verify analyzer from C# 9 with top level statements. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Console(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp9,
                                        CompilationErrorBehavior.Default,
                                        OutputKind.ConsoleApplication,
                                        additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 with top level statements. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Console(string path, DiagnosticAnalyzer[] diagnosticAnalyzers, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path }, diagnosticAnalyzers, ParseOptionsHelper.FromCSharp9, CompilationErrorBehavior.Default, OutputKind.ConsoleApplication, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 on a test library project. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9LibraryInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzerFromCSharp9InTest(path, diagnosticAnalyzer, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 on a test console project. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9ConsoleInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzerFromCSharp9InTest(path, diagnosticAnalyzer, OutputKind.ConsoleApplication, additionalReferences);

        /// <summary>
        /// Verify analyzer from C# 9 without top level statements. Note that analyzer is being run in non-concurrent execution mode,
        /// that means that issues regarding concurrent execution could not be detected with this method.
        /// </summary>
        public static void VerifyAnalyzerFromCSharp9Library(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp9,
                                        CompilationErrorBehavior.Default,
                                        OutputKind.DynamicallyLinkedLibrary,
                                        additionalReferences);

        public static void VerifyNonConcurrentAnalyzer(string path,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<MetadataReference> additionalReferences) =>
            VerifyNonConcurrentAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, null, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyNonConcurrentAnalyzer(string path,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<ParseOptions> options,
                                                       IEnumerable<MetadataReference> additionalReferences,
                                                       string sonarProjectConfigPath = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        options,
                                        CompilationErrorBehavior.Default,
                                        OutputKind.DynamicallyLinkedLibrary,
                                        additionalReferences,
                                        sonarProjectConfigPath);

        public static void VerifyNonConcurrentAnalyzer(string path,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<ParseOptions> options = null,
                                                       CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                       OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                       IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, options, checkMode, outputKind, additionalReferences);

        public static void VerifyNonConcurrentAnalyzer(IEnumerable<string> paths,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<MetadataReference> additionalReferences) =>
            VerifyNonConcurrentAnalyzer(paths, new[] { diagnosticAnalyzer }, null, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyNonConcurrentAnalyzer(IEnumerable<string> paths,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<ParseOptions> options = null,
                                                       IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(paths, new[] { diagnosticAnalyzer }, options, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyAnalyzer(IEnumerable<string> paths,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences) =>
            VerifyAnalyzer(paths, new[] { diagnosticAnalyzer }, null, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer[] diagnosticAnalyzers,
                                          IEnumerable<ParseOptions> options = null,
                                          CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                          OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                          IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyAnalyzer(new[] { path }, diagnosticAnalyzers, options, checkMode, outputKind, additionalReferences);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences,
                                          string sonarProjectConfigPath) =>
            VerifyAnalyzer(new[] { path },
                           new[] { diagnosticAnalyzer },
                           null, CompilationErrorBehavior.Default,
                           OutputKind.DynamicallyLinkedLibrary,
                           additionalReferences,
                           sonarProjectConfigPath);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<ParseOptions> options,
                                          IEnumerable<MetadataReference> additionalReferences,
                                          string sonarProjectConfigPath = null) =>
            VerifyAnalyzer(new[] { path },
                                     new[] { diagnosticAnalyzer },
                                     options,
                                     CompilationErrorBehavior.Default,
                                     OutputKind.DynamicallyLinkedLibrary,
                                     additionalReferences,
                                     sonarProjectConfigPath);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences) =>
            VerifyAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, null, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<ParseOptions> options = null,
                                          CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                          OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                          IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, options, checkMode, outputKind, additionalReferences);

        public static void VerifyAnalyzer(IEnumerable<string> paths,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<ParseOptions> options = null,
                                          IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyAnalyzer(paths, new[] { diagnosticAnalyzer }, options, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          CompilationErrorBehavior checkMode,
                                          IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, null, checkMode, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        public static void VerifyAnalyzerNoDuplication(IEnumerable<string> paths,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<ParseOptions> options = null,
                                                       IEnumerable<MetadataReference> additionalReferences = null)
        {
            using var scope = new EnvironmentVariableScope(true);
            scope.SetVariable(SonarDiagnosticAnalyzer.EnableConcurrentExecutionVariable, "true");
            VerifyNonConcurrentAnalyzer(paths, new[] { diagnosticAnalyzer }, options, CompilationErrorBehavior.Default, OutputKind.DynamicallyLinkedLibrary, additionalReferences);
        }

        public static void VerifyNonConcurrentUtilityAnalyzer<TMessage>(IEnumerable<string> paths,
                                                                        UtilityAnalyzerBase diagnosticAnalyzer,
                                                                        string protobufPath,
                                                                        string sonarProjectConfigPath,
                                                                        Action<IReadOnlyList<TMessage>> verifyProtobuf,
                                                                        IEnumerable<ParseOptions> options = null)
            where TMessage : IMessage<TMessage>, new()
        {
            var solutionBuilder = SolutionBuilder.CreateSolutionFromPaths(paths);
            foreach (var compilation in solutionBuilder.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, CompilationErrorBehavior.Default, sonarProjectConfigPath);
                verifyProtobuf(ReadProtobuf(protobufPath).ToList());
            }

            static IEnumerable<TMessage> ReadProtobuf(string path)
            {
                using var input = File.OpenRead(path);
                var parser = new MessageParser<TMessage>(() => new TMessage());
                while (input.Position < input.Length)
                {
                    yield return parser.ParseDelimitedFrom(input);
                }
            }
        }

        public static void VerifyNonConcurrentUtilityAnalyzerIsNotRun(IEnumerable<string> paths,
                                                                      UtilityAnalyzerBase diagnosticAnalyzer,
                                                                      string protobufPath,
                                                                      IEnumerable<ParseOptions> options = null)
        {
            var solutionBuilder = SolutionBuilder.CreateSolutionFromPaths(paths);
            foreach (var compilation in solutionBuilder.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, CompilationErrorBehavior.Default);
                new FileInfo(protobufPath).Length.Should().Be(0);
            }
        }

        public static void VerifyNoIssueReportedInTest(string path, SonarDiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNoIssueReportedInTest(path, diagnosticAnalyzer, null, additionalReferences);

        public static void VerifyNoIssueReportedInTest(string path,
                                                       SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<ParseOptions> options,
                                                       IEnumerable<MetadataReference> additionalReferences = null)
        {
            var builder = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddTestReferences()
                .AddReferences(additionalReferences)
                .AddDocument(path);

            VerifyNoIssueReported(builder, diagnosticAnalyzer, options, CompilationErrorBehavior.Default, null);
        }

        public static void VerifyNoIssueReportedFromCSharp9InTest(string path,
                                                                  SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                                  OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                                  IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNoIssueReported(path, diagnosticAnalyzer, options: ParseOptionsHelper.FromCSharp9, outputKind: outputKind, additionalReferences: AddTestReference(additionalReferences));

        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 IEnumerable<MetadataReference> additionalReferences) =>
            VerifyNoIssueReported(path, diagnosticAnalyzer, null, checkMode: CompilationErrorBehavior.Default, additionalReferences: additionalReferences);

        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 IEnumerable<ParseOptions> options,
                                                 IEnumerable<MetadataReference> additionalReferences,
                                                 string sonarProjectConfigPath = null) =>
            VerifyNoIssueReported(path, diagnosticAnalyzer, options, CompilationErrorBehavior.Default, additionalReferences: additionalReferences, sonarProjectConfigPath: sonarProjectConfigPath);

        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 IEnumerable<ParseOptions> options = null,
                                                 CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                 OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                 IEnumerable<MetadataReference> additionalReferences = null,
                                                 string sonarProjectConfigPath = null)
        {
            var builder = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.FromPath(path), outputKind: outputKind)
                .AddReferences(additionalReferences)
                .AddDocument(path);

            VerifyNoIssueReported(builder, diagnosticAnalyzer, options, checkMode, sonarProjectConfigPath);
        }

        public static void VerifyCodeFix(string path,
                                         string pathToExpected,
                                         SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                         SonarCodeFixProvider codeFixProvider,
                                         IEnumerable<ParseOptions> options = null,
                                         OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                         IEnumerable<MetadataReference> additionalReferences = null) =>
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToExpected, diagnosticAnalyzer, codeFixProvider, null, options, outputKind, additionalReferences);

        /// <summary>
        /// Verifies batch code fix.
        /// </summary>
        public static void VerifyCodeFix(string path,
                                         string pathToExpected,
                                         string pathToBatchExpected,
                                         SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                         SonarCodeFixProvider codeFixProvider,
                                         IEnumerable<ParseOptions> options = null,
                                         IEnumerable<MetadataReference> additionalReferences = null) =>
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToBatchExpected, diagnosticAnalyzer, codeFixProvider, null, options, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        /// <summary>
        /// Verifies code fix with title.
        /// </summary>
        public static void VerifyCodeFix(string path,
                                         string pathToExpected,
                                         SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                         SonarCodeFixProvider codeFixProvider,
                                         string codeFixTitle,
                                         IEnumerable<ParseOptions> options = null,
                                         IEnumerable<MetadataReference> additionalReferences = null) =>
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToExpected, diagnosticAnalyzer, codeFixProvider, codeFixTitle, options, OutputKind.DynamicallyLinkedLibrary, additionalReferences);

        private static void VerifyNonConcurrentAnalyzerFromCSharp9InTest(string path,
                                                                         DiagnosticAnalyzer diagnosticAnalyzer,
                                                                         OutputKind outputKind,
                                                                         IEnumerable<MetadataReference> additionalReferences = null) =>
            VerifyNonConcurrentAnalyzer(new[] { path },
                                        new[] { diagnosticAnalyzer },
                                        ParseOptionsHelper.FromCSharp9,
                                        CompilationErrorBehavior.Default,
                                        outputKind,
                                        AddTestReference(additionalReferences));

        private static void VerifyAnalyzer(IEnumerable<string> paths,
                                           DiagnosticAnalyzer[] diagnosticAnalyzers,
                                           IEnumerable<ParseOptions> options,
                                           CompilationErrorBehavior checkMode,
                                           OutputKind outputKind,
                                           IEnumerable<MetadataReference> additionalReferences,
                                           string sonarProjectConfigPath = null)
        {
            using var scope = new EnvironmentVariableScope(true);
            scope.SetVariable(SonarDiagnosticAnalyzer.EnableConcurrentExecutionVariable, "true");
            var analyzerLanguage = AnalyzerLanguage.FromPath(paths.First());
            var content = paths.Select(x => new ProjectFileAsPathAndContent(x, File.ReadAllText(new FileInfo(x).FullName, Encoding.UTF8))).ToList();
            var duplicatedFiles = content.Select(x => x.Duplicate(analyzerLanguage)).ToList();
            content.AddRange(duplicatedFiles);
            var solution = SolutionBuilder.CreateSolutionFromContent(content, analyzerLanguage, outputKind, additionalReferences);
            CompileAndVerifyAnalyzer(solution, diagnosticAnalyzers, options, checkMode, sonarProjectConfigPath);
        }

        private static void VerifyNonConcurrentAnalyzer(IEnumerable<string> paths,
                                                        DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                        IEnumerable<ParseOptions> options,
                                                        CompilationErrorBehavior checkMode,
                                                        OutputKind outputKind,
                                                        IEnumerable<MetadataReference> additionalReferences,
                                                        string sonarProjectConfigPath = null)
        {
            var solution = SolutionBuilder.CreateSolutionFromPaths(paths, outputKind, additionalReferences, IsSupportForCSharp9InitNeeded(options));
            CompileAndVerifyAnalyzer(solution, diagnosticAnalyzers, options, checkMode, sonarProjectConfigPath);
        }

        private static void CompileAndVerifyAnalyzer(SolutionBuilder solution,
                                                     DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                     IEnumerable<ParseOptions> options,
                                                     CompilationErrorBehavior checkMode,
                                                     string sonarProjectConfigPath = null)
        {
            foreach (var compilation in solution.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzers, checkMode, sonarProjectConfigPath);
            }
        }

        private static void VerifyNoIssueReported(ProjectBuilder builder,
                                                  SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                  IEnumerable<ParseOptions> options,
                                                  CompilationErrorBehavior checkMode,
                                                  string sonarProjectConfigPath)
        {
            foreach (var option in options ?? new ParseOptions[] { null })
            {
                DiagnosticVerifier.VerifyNoIssueReported(builder.GetCompilation(option), diagnosticAnalyzer, checkMode, sonarProjectConfigPath);
            }
        }

        private static bool IsSupportForCSharp9InitNeeded(IEnumerable<ParseOptions> options) =>
            options != null && options.OfType<CSharpParseOptions>().Select(option => option.LanguageVersion).Contains(LanguageVersion.CSharp9);

        private static IEnumerable<MetadataReference> AddTestReference(IEnumerable<MetadataReference> additionalReferences) =>
            NuGetMetadataReference.MSTestTestFrameworkV1.Concat(additionalReferences ?? Enumerable.Empty<MetadataReference>());
    }
}
