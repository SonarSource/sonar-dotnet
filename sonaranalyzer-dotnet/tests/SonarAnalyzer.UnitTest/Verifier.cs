/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Google.Protobuf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest
{
    public enum CompilationErrorBehavior
    {
        FailTest,
        Ignore,
        Default = FailTest
    }

    internal static class Verifier
    {
        public static void VerifyNoExceptionThrown(string path,
            IEnumerable<DiagnosticAnalyzer> diagnosticAnalyzers, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default)
        {
            var compilation = SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddDocument(path)
                .GetCompilation();

            var diagnostics = DiagnosticVerifier.GetAllDiagnostics(compilation, diagnosticAnalyzers, checkMode);
        }

        public static void VerifyCSharpAnalyzer(string snippet, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            IEnumerable<CSharpParseOptions> options = null, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
            IEnumerable<MetadataReference> additionalReferences = null)
        {
            var solution = SolutionBuilder
               .Create()
               .AddProject(AnalyzerLanguage.CSharp)
               .AddSnippet(snippet)
               .AddReferences(additionalReferences)
               .GetSolution();

            // TODO: add [CallerLineNumber]int lineNumber = 0
            // then add ability to shift result reports with this line number
            foreach (var compilation in solution.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, checkMode);
            }
        }

        public static void VerifyVisualBasicAnalyzer(string snippet, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default, IEnumerable<MetadataReference> additionalReferences = null)
        {
            var solution = SolutionBuilder
               .Create()
               .AddProject(AnalyzerLanguage.VisualBasic)
               .AddSnippet(snippet)
               .AddReferences(additionalReferences)
               .GetSolution();

            // TODO: add [CallerLineNumber]int lineNumber = 0
            // then add ability to shift result reports with this line number
            foreach (var compilation in solution.Compile())
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, checkMode);
            }
        }

        public static void VerifyAnalyzer(string path, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            IEnumerable<ParseOptions> options = null, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
            IEnumerable<MetadataReference> additionalReferences = null)
        {
            VerifyAnalyzer(new[] { path }, diagnosticAnalyzer, options, checkMode, additionalReferences);
        }

        public static void VerifyUtilityAnalyzer<TMessage>(IEnumerable<string> paths, UtilityAnalyzerBase diagnosticAnalyzer,
            string protobufPath, Action<IList<TMessage>> verifyProtobuf, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default)
            where TMessage : IMessage<TMessage>, new()
        {
            var solutionBuilder = SolutionBuilder.CreateSolutionFromPaths(paths);

            foreach (var compilation in solutionBuilder.Compile())
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, checkMode);

                verifyProtobuf(ReadProtobuf(protobufPath).ToList());
            }

            IEnumerable<TMessage> ReadProtobuf(string path)
            {
                using (var input = File.OpenRead(path))
                {
                    var parser = new MessageParser<TMessage>(() => new TMessage());
                    while (input.Position < input.Length)
                    {
                        yield return parser.ParseDelimitedFrom(input);
                    }
                }
            }
        }

        public static void VerifyAnalyzer(IEnumerable<string> paths, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            IEnumerable<ParseOptions> options = null, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
            IEnumerable<MetadataReference> additionalReferences = null)
        {
            var solutionBuilder = SolutionBuilder.CreateSolutionFromPaths(paths, additionalReferences);

            foreach (var compilation in solutionBuilder.Compile(options?.ToArray()))
            {
                DiagnosticVerifier.Verify(compilation, diagnosticAnalyzer, checkMode);
            }
        }

        public static void VerifyNoIssueReportedInTest(string path, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            IEnumerable<MetadataReference> additionalReferences = null)
        {
            var compilation = SolutionBuilder.Create()
                .AddTestProject(AnalyzerLanguage.FromPath(path))
                .AddReferences(additionalReferences)
                .AddDocument(path)
                .GetCompilation();

            DiagnosticVerifier.VerifyNoIssueReported(compilation, diagnosticAnalyzer);
        }

        public static void VerifyNoIssueReported(string path, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            IEnumerable<ParseOptions> options = null, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default,
            IEnumerable<MetadataReference> additionalReferences = null)
        {
            var projectBuilder = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddReferences(additionalReferences)
                .AddDocument(path);


            if (options == null)
            {
                var compilation = projectBuilder.GetCompilation(null);
                DiagnosticVerifier.VerifyNoIssueReported(compilation, diagnosticAnalyzer, checkMode);
            }
            else
            {
                foreach (var option in options)
                {
                    var compilation = projectBuilder.GetCompilation(option);
                    DiagnosticVerifier.VerifyNoIssueReported(compilation, diagnosticAnalyzer, checkMode);
                }
            }
        }

        public static void VerifyCodeFix(string path, string pathToExpected,
            SonarDiagnosticAnalyzer diagnosticAnalyzer, SonarCodeFixProvider codeFixProvider,
            IEnumerable<ParseOptions> options = null, IEnumerable<MetadataReference> additionalReferences = null)
        {
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToExpected, diagnosticAnalyzer, codeFixProvider,
                null, options, additionalReferences);
        }

        public static void VerifyCodeFix(string path, string pathToExpected, string pathToBatchExpected,
            SonarDiagnosticAnalyzer diagnosticAnalyzer, SonarCodeFixProvider codeFixProvider,
            IEnumerable<ParseOptions> options = null, IEnumerable<MetadataReference> additionalReferences = null)
        {
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToBatchExpected, diagnosticAnalyzer, codeFixProvider,
                null, options, additionalReferences);
        }

        public static void VerifyCodeFix(string path, string pathToExpected,
            SonarDiagnosticAnalyzer diagnosticAnalyzer, SonarCodeFixProvider codeFixProvider, string codeFixTitle,
            IEnumerable<ParseOptions> options = null, IEnumerable<MetadataReference> additionalReferences = null)
        {
            CodeFixVerifier.VerifyCodeFix(path, pathToExpected, pathToExpected, diagnosticAnalyzer, codeFixProvider,
                codeFixTitle, options, additionalReferences);
        }
    }
}
