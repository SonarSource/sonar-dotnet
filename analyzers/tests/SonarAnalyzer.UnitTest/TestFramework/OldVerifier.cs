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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Google.Protobuf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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

    [Obsolete("Use VerifierBuilder instead.")]
    public static class OldVerifier
    {
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
        public static void VerifyAnalyzerCSharpPreviewLibrary(string path, DiagnosticAnalyzer diagnosticAnalyzer, IEnumerable<MetadataReference> additionalReferences = null) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.CSharpPreview)
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
                                                                        ImmutableArray<ParseOptions> options = default)
            where TMessage : IMessage<TMessage>, new() =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddPaths(RemoveTestCasesPrefix(paths))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithSonarProjectConfigPath(sonarProjectConfigPath)
                .WithProtobufPath(protobufPath)
                .VerifyUtilityAnalyzer(verifyProtobuf);

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyUtilityAnalyzerIsNotRun(string path,
                                                         UtilityAnalyzerBase diagnosticAnalyzer,
                                                         string protobufPath,
                                                         ImmutableArray<ParseOptions> options = default) =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithProtobufPath(protobufPath)
                .VerifyUtilityAnalyzerProducesEmptyProtobuf();

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

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyCodeFix<TCodeFix>(string path,
                                                   string pathToExpected,
                                                   SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                   ImmutableArray<ParseOptions> options = default,
                                                   OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
                                                   IEnumerable<MetadataReference> additionalReferences = null) where TCodeFix : SonarCodeFix, new() =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithOutputKind(outputKind)
                .WithCodeFix<TCodeFix>()
                .WithCodeFixedPath(RemoveTestCasesPrefix(pathToExpected))
                .VerifyCodeFix();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyCodeFix<TCodeFix>(string path,
                                                   string pathToExpected,
                                                   string pathToBatchExpected,
                                                   SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                   ImmutableArray<ParseOptions> options = default,
                                                   IEnumerable<MetadataReference> additionalReferences = null)
            where TCodeFix : SonarCodeFix, new() =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithCodeFix<TCodeFix>()
                .WithCodeFixedPath(RemoveTestCasesPrefix(pathToExpected))
                .WithCodeFixedPathBatch(RemoveTestCasesPrefix(pathToBatchExpected))
                .VerifyCodeFix();

        [Obsolete("Use VerifierBuilder instead.")]
        public static void VerifyCodeFix<TCodeFix>(string path,
                                                   string pathToExpected,
                                                   SonarDiagnosticAnalyzer diagnosticAnalyzer,
                                                   string codeFixTitle,
                                                   ImmutableArray<ParseOptions> options = default,
                                                   IEnumerable<MetadataReference> additionalReferences = null)
            where TCodeFix : SonarCodeFix, new() =>
            new VerifierBuilder()
                .AddAnalyzer(() => diagnosticAnalyzer)
                .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
                .AddPaths(RemoveTestCasesPrefix(path))
                .WithOptions(options.IsDefault ? ImmutableArray<ParseOptions>.Empty : options)
                .WithCodeFix<TCodeFix>()
                .WithCodeFixTitle(codeFixTitle)
                .WithCodeFixedPath(RemoveTestCasesPrefix(pathToExpected))
                .VerifyCodeFix();

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
