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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public enum CompilationErrorBehavior
    {
        FailTest,
        Ignore,
        Default = FailTest
    }

    public static class OldVerifier
    {
        public static void VerifyNoExceptionThrown(string path, DiagnosticAnalyzer[] diagnosticAnalyzers, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default)
        {
            var compilation = SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddDocument(path)
                .GetCompilation();

            var diagnostics = DiagnosticVerifier.GetAnalyzerDiagnostics(compilation, diagnosticAnalyzers, checkMode);
            DiagnosticVerifier.VerifyNoExceptionThrown(diagnostics);
        }

        /// <summary>
        /// Verify analyzer from C# on a snippet in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyCSharpAnalyzer(string snippet,
                                        SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                        IEnumerable<MetadataReference> additionalReferences) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences)
                .AddSnippet(snippet)
                .WithConcurrentAnalysis(false)
                .Verify();

        /// <summary>
        /// Verify analyzer from C# on a snippet in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyCSharpAnalyzer(string snippet,
                                        SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                        CompilationErrorBehavior checkMode,
                                        IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddSnippet(snippet)
                .WithConcurrentAnalysis(false)
                .WithErrorBehavior(checkMode)
                .Verify();

        /// <summary>
        /// Verify analyzer from C# on a snippet in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyCSharpAnalyzer(string snippet,
                                                SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                ImmutableArray<ParseOptions> options = default,
                                                CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                IEnumerable<MetadataReference> additionalReferences = null,
                                                OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                DiagnosticDescriptor[] onlyDiagnostics = null,
                                                string sonarProjectConfigPath = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddSnippet(snippet)
                .WithConcurrentAnalysis(false)
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithErrorBehavior(checkMode)
                .WithOutputKind(outputKind)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .WithSonarProjectConfigPath(sonarProjectConfigPath)
                .Verify();

        /// <summary>
        /// Verify analyzer from VB.NET on a snippet in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyVisualBasicAnalyzer(string snippet,
                                                     DiagnosticAnalyzer diagnosticAnalyzer,
                                                     CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                     IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddSnippet(snippet)
                .WithConcurrentAnalysis(false)
                .WithErrorBehavior(checkMode)
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 9 with top level statements in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp9Console(string path,
                                                            DiagnosticAnalyzer diagnosticAnalyzer,
                                                            IEnumerable<MetadataReference> additionalReferences = null,
                                                            DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithTopLevelStatements()
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp10Console(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithTopLevelStatements()
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 9 with top level statements in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp9Console(string[] paths, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(paths))
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithTopLevelStatements()
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp10Console(string[] paths, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(paths))
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithTopLevelStatements()
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 9 with top level statements in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp9Console(string path,
                                                            DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                            IEnumerable<MetadataReference> additionalReferences = null,
                                                            DiagnosticDescriptor[] onlyDiagnostics = null) =>
            diagnosticAnalyzers.Aggregate(new VerifierBuilder(), (builder, analyzer) => builder.AddAnalyzer(() => analyzer))
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithTopLevelStatements()
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 9 on a test library project in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp9LibraryInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddTestReference()
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 10 on a test library project in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp10LibraryInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddTestReference()
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 9 on a test console project in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp9ConsoleInTest(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddTestReference()
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithTopLevelStatements()
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 9 without top level statements in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp9Library(string path,
                                                            DiagnosticAnalyzer diagnosticAnalyzer,
                                                            IEnumerable<MetadataReference> additionalReferences = null,
                                                            DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 9 without top level statements in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp9Library(string path,
                                                            DiagnosticAnalyzer diagnosticAnalyzer,
                                                            CompilationErrorBehavior behaviour,
                                                            IEnumerable<MetadataReference> additionalReferences = null,
                                                            DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithErrorBehavior(behaviour)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp10Library(string path,
                                                             DiagnosticAnalyzer diagnosticAnalyzer,
                                                             IEnumerable<MetadataReference> additionalReferences = null,
                                                             DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp10Library(string path,
                                                             DiagnosticAnalyzer[] diagnosticAnalyzers,
                                                             IEnumerable<MetadataReference> additionalReferences = null,
                                                             DiagnosticDescriptor[] onlyDiagnostics = null) =>
            diagnosticAnalyzers.Aggregate(new VerifierBuilder(), (builder, analyzer) => builder.AddAnalyzer(() => analyzer))
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp10Library(string path,
                                                             DiagnosticAnalyzer diagnosticAnalyzer,
                                                             CompilationErrorBehavior behavior,
                                                             IEnumerable<MetadataReference> additionalReferences = null,
                                                             DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithErrorBehavior(behavior)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerCSharpPreviewLibrary(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.CSharpPreview)
                .Verify();

        /// <summary>
        /// Verify analyzer from C# 9 without top level statements in non-concurrent execution mode.
        /// </summary>
        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp9Library(string[] paths, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(paths))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzerFromCSharp10Library(string[] paths, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(paths))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNonConcurrentAnalyzer(string path,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<MetadataReference> additionalReferences) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNonConcurrentAnalyzer(string path,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       ImmutableArray<ParseOptions> options = default,
                                                       CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                       OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                       IEnumerable<MetadataReference> additionalReferences = null,
                                                       DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithErrorBehavior(checkMode)
                .WithOutputKind(outputKind)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNonConcurrentAnalyzer(IEnumerable<string> paths,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<MetadataReference> additionalReferences) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(paths))
                .WithConcurrentAnalysis(false)
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNonConcurrentAnalyzer(IEnumerable<string> paths,
                                                       DiagnosticAnalyzer diagnosticAnalyzer,
                                                       ImmutableArray<ParseOptions> options = default,
                                                       IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(paths))
                .WithConcurrentAnalysis(false)
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzer(IEnumerable<string> paths,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(paths))
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer[] diagnosticAnalyzers,
                                          ImmutableArray<ParseOptions> options = default,
                                          CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                          OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                          IEnumerable<MetadataReference> additionalReferences = null,
                                          DiagnosticDescriptor[] onlyDiagnostics = null) =>
            diagnosticAnalyzers.Aggregate(new VerifierBuilder(), (builder, analyzer) => builder.AddAnalyzer(() => analyzer))
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithErrorBehavior(checkMode)
                .WithOutputKind(outputKind)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences,
                                          string sonarProjectConfigPath) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences)
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithSonarProjectConfigPath(sonarProjectConfigPath)
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          ImmutableArray<ParseOptions> options,
                                          IEnumerable<MetadataReference> additionalReferences,
                                          string sonarProjectConfigPath = null,
                                          DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences)
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options)
                .WithSonarProjectConfigPath(sonarProjectConfigPath)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          IEnumerable<MetadataReference> additionalReferences,
                                          DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences)
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          ImmutableArray<ParseOptions> options = default,
                                          CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                          OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                          IEnumerable<MetadataReference> additionalReferences = null,
                                          DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithErrorBehavior(checkMode)
                .WithOutputKind(outputKind)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzer(IEnumerable<string> paths,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          ImmutableArray<ParseOptions> options = default,
                                          IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(paths))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .Verify();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyAnalyzer(string path,
                                          DiagnosticAnalyzer diagnosticAnalyzer,
                                          CompilationErrorBehavior checkMode,
                                          IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithErrorBehavior(checkMode)
                .Verify();

        public static void VerifyNonConcurrentUtilityAnalyzer<TMessage>(IEnumerable<string> paths,
                                                                        UtilityAnalyzerBase diagnosticAnalyzer,
                                                                        string protobufPath,
                                                                        string sonarProjectConfigPath,
                                                                        Action<IReadOnlyList<TMessage>> verifyProtobuf,
                                                                        IEnumerable<ParseOptions> options = null)
            where TMessage : IMessage<TMessage>, new()
        {
            var solutionBuilder = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.FromPath(paths.First()))
                .AddDocuments(paths)
                .Solution;
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

        /// <summary>
        /// Verify utility analyzer is not run in non-concurrent execution mode.
        /// </summary>
        public static void VerifyUtilityAnalyzerIsNotRun(string path,
                                                         UtilityAnalyzerBase diagnosticAnalyzer,
                                                         string protobufPath,
                                                         IEnumerable<ParseOptions> options = null)
        {
            var solutionBuilder = SolutionBuilder.CreateSolutionFromPath(path);
            foreach (var compilation in solutionBuilder.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, CompilationErrorBehavior.Default);
                new FileInfo(protobufPath).Length.Should().Be(0);
            }
        }

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNoIssueReportedInTest(string path,
                                                       SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                       IEnumerable<MetadataReference> additionalReferences = null,
                                                       DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddTestReference()
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .VerifyNoIssueReported();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNoIssueReportedInTest(string path,
                                                       SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                       ImmutableArray<ParseOptions> options,
                                                       IEnumerable<MetadataReference> additionalReferences = null,
                                                       DiagnosticDescriptor[] onlyDiagnostics = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddTestReference()
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithOnlyDiagnostics(onlyDiagnostics ?? Array.Empty<DiagnosticDescriptor>())
                .VerifyNoIssueReported();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 IEnumerable<MetadataReference> additionalReferences) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences)
                .AddPaths(RemoveTestCasesPrefix(path))
                .VerifyNoIssueReported();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 ImmutableArray<ParseOptions> options,
                                                 IEnumerable<MetadataReference> additionalReferences,
                                                 string sonarProjectConfigPath = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences)
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithSonarProjectConfigPath(sonarProjectConfigPath)
                .VerifyNoIssueReported();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyNoIssueReported(string path,
                                                 SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                 ImmutableArray<ParseOptions> options = default,
                                                 CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
                                                 OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                 IEnumerable<MetadataReference> additionalReferences = null,
                                                 string sonarProjectConfigPath = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithErrorBehavior(checkMode)
                .WithOutputKind(outputKind)
                .WithSonarProjectConfigPath(sonarProjectConfigPath)
                .VerifyNoIssueReported();

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

        private static string[] RemoveTestCasesPrefix(IEnumerable<string> paths) =>
            paths.Select(RemoveTestCasesPrefix).ToArray();

        private static string RemoveTestCasesPrefix(string path)
        {
            const string prefix = @"TestCases\";
            return path.StartsWith(prefix)
                ? path.Substring(prefix.Length)
                : throw new ArgumentException($"TestCase path doesn't start with {prefix}: {path}");
        }
    }
}
