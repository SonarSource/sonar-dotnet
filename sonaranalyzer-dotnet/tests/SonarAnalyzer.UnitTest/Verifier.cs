/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Google.Protobuf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest
{
    internal static class Verifier
    {
        #region Well known metadata references
        private const string PackagesFolderRelativePath = @"..\..\..\..\packages\";

        private static readonly MetadataReference systemAssembly =
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference systemLinqAssembly =
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference systemNetAssembly =
            MetadataReference.CreateFromFile(typeof(WebClient).Assembly.Location);
        internal static readonly MetadataReference SystemImmutableAssembly =
            MetadataReference.CreateFromFile(typeof(ImmutableArray).Assembly.Location);
        internal static readonly MetadataReference SystemDataAssembly =
            MetadataReference.CreateFromFile(typeof(DataTable).Assembly.Location);
        internal static readonly MetadataReference SystemXmlAssembly =
            MetadataReference.CreateFromFile(typeof(System.Xml.XmlDocument).Assembly.Location);
        internal static readonly MetadataReference MicrosoftVisualStudioTestToolsUnitTestingAssembly =
            MetadataReference.CreateFromFile(typeof(TestMethodAttribute).Assembly.Location);
        internal static readonly MetadataReference SystemComponentModelComposition =
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.Composition.PartCreationPolicyAttribute).Assembly.Location);
        internal static readonly MetadataReference XunitCoreAssembly =
            MetadataReference.CreateFromFile(PackagesFolderRelativePath + @"xunit.extensibility.core.2.2.0\lib\netstandard1.1\xunit.core.dll");
        internal static readonly MetadataReference XunitAssertAssembly =
            MetadataReference.CreateFromFile(PackagesFolderRelativePath + @"xunit.assert.2.2.0\lib\netstandard1.1\xunit.assert.dll");
        internal static readonly MetadataReference NUnitFrameworkAssembly =
            MetadataReference.CreateFromFile(PackagesFolderRelativePath + @"NUnit.2.6.4\lib\nunit.framework.dll");
        internal static readonly MetadataReference SystemThreadingTasksExtensionsAssembly =
           MetadataReference.CreateFromFile(PackagesFolderRelativePath + @"System.Threading.Tasks.Extensions.4.3.0\lib\netstandard1.0\System.Threading.Tasks.Extensions.dll");
        internal static readonly MetadataReference FluentAssertionsAssembly =
            MetadataReference.CreateFromFile(typeof(AssertionExtensions).Assembly.Location);
        internal static readonly MetadataReference FluentAssertionsCoreAssembly =
            MetadataReference.CreateFromFile(typeof(AssertionOptions).Assembly.Location);
        internal static readonly MetadataReference MicrosoftVisualBasicAssembly =
            MetadataReference.CreateFromFile(typeof(Microsoft.VisualBasic.Interaction).Assembly.Location);
        internal static readonly MetadataReference SystemXamlAssembly =
            MetadataReference.CreateFromFile(typeof(System.Windows.Markup.ConstructorArgumentAttribute).Assembly.Location);
        internal static readonly MetadataReference SystemWindowsFormsAssembly =
            MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Form).Assembly.Location);
        internal static readonly MetadataReference RuntimeSerializationAssembly =
            MetadataReference.CreateFromFile(typeof(System.Runtime.Serialization.DataMemberAttribute).Assembly.Location);
        #endregion

        private const string FIXED_MESSAGE = "Fixed";

        private const string GeneratedAssemblyName = "foo";
        private const string TestAssemblyName = "fooTest";
        private const string AnalyzerFailedDiagnosticId = "AD0001";
        private const string CSharpFileExtension = ".cs";
        private const string VisualBasicFileExtension = ".vb";

        private const string WindowsLineEnding = "\r\n";
        private const string UnixLineEnding = "\n";

        #region Verify

        public static void VerifyNoExceptionThrown(string path,
            IEnumerable<DiagnosticAnalyzer> diagnosticAnalyzers)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var file = new FileInfo(path);
                var project = CreateProject(file.Extension, GeneratedAssemblyName, workspace).AddDocument(file);
                var compilation = project.GetCompilationAsync().Result;
                var diagnostics = GetAllDiagnostics(compilation, diagnosticAnalyzers);
                VerifyNoExceptionThrown(diagnostics);
            }
        }

        public static void VerifyCSharpAnalyzer(string snippet, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            ParseOptions options = null, params MetadataReference[] additionalReferences)
        {
            VerifyAnalyzer(new[] { new DocumentInfo($"file1{CSharpFileExtension}", snippet) },
                CSharpFileExtension, new[] { diagnosticAnalyzer }, options, null, additionalReferences);
        }

        public static void VerifyVisualBasicAnalyzer(string snippet, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            ParseOptions options = null, params MetadataReference[] additionalReferences)
        {
            VerifyAnalyzer(new[] { new DocumentInfo($"file1{VisualBasicFileExtension}", snippet) },
                VisualBasicFileExtension, new[] { diagnosticAnalyzer }, options, null, additionalReferences);
        }

        public static void VerifyAnalyzer(string path, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            ParseOptions options = null, params MetadataReference[] additionalReferences)
        {
            VerifyAnalyzer(new[] { path }, new[] { diagnosticAnalyzer }, null, options, additionalReferences);
        }

        public static void VerifyAnalyzer(string path, IEnumerable<SonarDiagnosticAnalyzer> diagnosticAnalyzers,
            ParseOptions options = null, params MetadataReference[] additionalReferences)
        {
            VerifyAnalyzer(new[] { path }, diagnosticAnalyzers, null, options, additionalReferences);
        }

        public static void VerifyUtilityAnalyzer<TMessage>(IEnumerable<string> paths, UtilityAnalyzerBase diagnosticAnalyzer,
            string protobufPath, Action<IList<TMessage>> verifyProtobuf)
            where TMessage : IMessage<TMessage>, new()
        {
            VerifyAnalyzer(paths, new[] { diagnosticAnalyzer }, (analyzer, compilation) =>
            {
                verifyProtobuf(ReadProtobuf<TMessage>(protobufPath).ToList());
            });
        }

        public static void VerifyAnalyzer(IEnumerable<string> paths, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            Action<DiagnosticAnalyzer, Compilation> additionalVerify = null, ParseOptions options = null,
            params MetadataReference[] additionalReferences)
        {
            VerifyAnalyzer(paths, new[] { diagnosticAnalyzer }, additionalVerify, options, additionalReferences);
        }

        public static void VerifyAnalyzer(IEnumerable<string> paths, IEnumerable<SonarDiagnosticAnalyzer> diagnosticAnalyzers,
            Action<DiagnosticAnalyzer, Compilation> additionalVerify = null, ParseOptions options = null,
            params MetadataReference[] additionalReferences)
        {
            if (paths == null || !paths.Any())
            {
                throw new ArgumentException("Please specify at least one file path to analyze.", nameof(paths));
            }

            var files = paths.Select(path => new FileInfo(path)).ToList();
            if (files.Select(file => file.Extension).Distinct().Count() != 1)
            {
                throw new ArgumentException("Please use a collection of paths with the same extension", nameof(paths));
            }

            var fileExtension = files[0].Extension;

            var fileContents = files.Select(file => new DocumentInfo(file));

            VerifyAnalyzer(fileContents, fileExtension, diagnosticAnalyzers, options, additionalVerify, additionalReferences);
        }

        private static IEnumerable<TMessage> ReadProtobuf<TMessage>(string path)
            where TMessage : IMessage<TMessage>, new()
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

        private static void VerifyAnalyzer(IEnumerable<DocumentInfo> documents, string fileExtension, IEnumerable<SonarDiagnosticAnalyzer> diagnosticAnalyzers,
            ParseOptions options = null, Action<DiagnosticAnalyzer, Compilation> additionalVerify = null, params MetadataReference[] additionalReferences)
        {
            try
            {
                HookSuppression();

                var parseOptions = GetParseOptionsAlternatives(options, fileExtension);

                using (var workspace = new AdhocWorkspace())
                {
                    var project = CreateProject(fileExtension, GeneratedAssemblyName, workspace, additionalReferences);
                    project = documents.Aggregate(project, (p, doc) => p.AddDocument(doc.Name, doc.Content).Project); // side effect on purpose (project is immutable)

                    var issueLocationCollector = new IssueLocationCollector();

                    foreach (var parseOption in parseOptions)
                    {
                        if (parseOption != null)
                        {
                            project = project.WithParseOptions(parseOption);
                        }

                        var compilation = project.GetCompilationAsync().Result;

                        var diagnostics = GetDiagnostics(compilation, diagnosticAnalyzers);

                        var expectedIssues = issueLocationCollector
                            .GetExpectedIssueLocations(compilation.SyntaxTrees.Skip(1).First().GetText().Lines)
                            .ToList();

                        foreach (var diagnostic in diagnostics)
                        {
                            VerifyIssue(expectedIssues, issue => issue.IsPrimary, diagnostic.Location, diagnostic.GetMessage(), out var issueId);

                            diagnostic.AdditionalLocations
                                .Select((location, i) => diagnostic.GetSecondaryLocation(i))
                                .OrderBy(x => x.Location.GetLineNumberToReport())
                                .ThenBy(x => x.Location.GetLineSpan().StartLinePosition.Character)
                                .ToList()
                                .ForEach(secondaryLocation =>
                                {
                                    VerifyIssue(expectedIssues, issue => issue.IssueId == issueId && !issue.IsPrimary,
                                        secondaryLocation.Location, secondaryLocation.Message, out issueId);
                                });
                        }

                        if (expectedIssues.Count != 0)
                        {
                            Execute.Assertion.FailWith($"Issue expected but not raised on line(s) {string.Join(",", expectedIssues.Select(i => i.LineNumber))}.");
                        }

                        // When there are no diagnostics reported from the test (for example the FileLines analyzer
                        // does not report in each call to Verifier.VerifyAnalyzer) we skip the check for the extension
                        // method.
                        if (diagnostics.Any())
                        {
                            ExtensionMethodsCalledForAllDiagnostics(diagnosticAnalyzers).Should().BeTrue("The ReportDiagnosticWhenActive should be used instead of ReportDiagnostic");
                        }

                        foreach (var diagnosticAnalyzer in diagnosticAnalyzers)
                        {
                            additionalVerify?.Invoke(diagnosticAnalyzer, compilation);
                        }
                    }
                }
            }
            finally
            {
                UnHookSuppression();
            }
        }

        private static void VerifyIssue(IList<IIssueLocation> expectedIssues, Func<IIssueLocation, bool> issueFilter, Location location, string message, out string issueId)
        {
            var lineNumber = location.GetLineNumberToReport();

            var expectedIssue = expectedIssues
                .Where(issueFilter)
                .FirstOrDefault(issue => issue.LineNumber == lineNumber);

            if (expectedIssue == null)
            {
                Execute.Assertion.FailWith($"Issue with message '{message}' not expected on line {lineNumber}");
            }

            if (expectedIssue.Message != null && expectedIssue.Message != message)
            {
                Execute.Assertion.FailWith($"Expected message on line {lineNumber} to be '{expectedIssue.Message}', but got '{message}'.");
            }

            var diagnosticStart = location.GetLineSpan().StartLinePosition.Character;

            if (expectedIssue.Start.HasValue && expectedIssue.Start != diagnosticStart)
            {
                Execute.Assertion.FailWith(
                    $"Expected issue on line {lineNumber} to start on column {expectedIssue.Start} but got column {diagnosticStart}.");
            }

            if (expectedIssue.Length.HasValue && expectedIssue.Length != location.SourceSpan.Length)
            {
                Execute.Assertion.FailWith(
                    $"Expected issue on line {lineNumber} to have a length of {expectedIssue.Length} but got a length of {location.SourceSpan.Length}).");
            }

            expectedIssues.Remove(expectedIssue);

            issueId = expectedIssue.IssueId;
        }

        public static void VerifyNoIssueReportedInTest(string path, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            params MetadataReference[] additionalReferences)
        {
            VerifyNoIssueReported(path, TestAssemblyName, diagnosticAnalyzer, additionalReferences);
        }

        public static void VerifyNoIssueReported(string path, SonarDiagnosticAnalyzer diagnosticAnalyzer, ParseOptions options = null,
            params MetadataReference[] additionalReferences)
        {
            VerifyNoIssueReported(path, GeneratedAssemblyName, diagnosticAnalyzer, additionalReferences, options);
        }

        public static void VerifyCodeFix(string path, string pathToExpected,
            SonarDiagnosticAnalyzer diagnosticAnalyzer, SonarCodeFixProvider codeFixProvider,
            params MetadataReference[] additionalReferences)
        {
            VerifyCodeFix(path, pathToExpected, pathToExpected, diagnosticAnalyzer, codeFixProvider,
                null, additionalReferences);
        }

        public static void VerifyCodeFix(string path, string pathToExpected, string pathToBatchExpected,
            SonarDiagnosticAnalyzer diagnosticAnalyzer, SonarCodeFixProvider codeFixProvider,
            params MetadataReference[] additionalReferences)
        {
            VerifyCodeFix(path, pathToExpected, pathToBatchExpected, diagnosticAnalyzer, codeFixProvider,
                null, additionalReferences);
        }

        public static void VerifyCodeFix(string path, string pathToExpected,
            SonarDiagnosticAnalyzer diagnosticAnalyzer, SonarCodeFixProvider codeFixProvider, string codeFixTitle,
            params MetadataReference[] additionalReferences)
        {
            VerifyCodeFix(path, pathToExpected, pathToExpected, diagnosticAnalyzer, codeFixProvider,
                codeFixTitle, additionalReferences);
        }

        public static void VerifyCodeFix(string path, string pathToExpected, string pathToBatchExpected,
            SonarDiagnosticAnalyzer diagnosticAnalyzer, SonarCodeFixProvider codeFixProvider, string codeFixTitle,
            params MetadataReference[] additionalReferences)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var file = new FileInfo(path);
                var parseOptions = GetParseOptionsWithDifferentLanguageVersions(null, file.Extension);

                foreach (var parseOption in parseOptions)
                {
                    var document = CreateProject(file.Extension, GeneratedAssemblyName, workspace, additionalReferences)
                        .AddDocument(file, true)
                        .Documents
                        .Single(d => d.Name == file.Name);
                    RunCodeFixWhileDocumentChanges(diagnosticAnalyzer, codeFixProvider, codeFixTitle, document, parseOption, pathToExpected);
                }
            }

            VerifyFixAllCodeFix(path, pathToBatchExpected, diagnosticAnalyzer, codeFixProvider, codeFixTitle, additionalReferences);
        }

        #endregion

        #region Generic helper

        private static void VerifyNoIssueReported(string path, string assemblyName, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            MetadataReference[] additionalReferences, ParseOptions parseOptions = null)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var file = new FileInfo(path);
                var project = CreateProject(file.Extension, assemblyName, workspace, additionalReferences)
                    .AddDocument(file);

                project = parseOptions != null
                    ? project.WithParseOptions(parseOptions)
                    : project;

                var compilation = project.GetCompilationAsync().Result;
                var diagnostics = GetDiagnostics(compilation, new[] { diagnosticAnalyzer });

                diagnostics.Should().BeEmpty();
            }
        }

        private static void VerifyFixAllCodeFix(string path, string pathToExpected, SonarDiagnosticAnalyzer diagnosticAnalyzer,
            CodeFixProvider codeFixProvider, string codeFixTitle, params MetadataReference[] additionalReferences)
        {
            var fixAllProvider = codeFixProvider.GetFixAllProvider();
            if (fixAllProvider == null)
            {
                return;
            }

            using (var workspace = new AdhocWorkspace())
            {
                var file = new FileInfo(path);
                var parseOptions = GetParseOptionsWithDifferentLanguageVersions(null, file.Extension);

                foreach (var parseOption in parseOptions)
                {
                    var document = CreateProject(file.Extension, GeneratedAssemblyName, workspace, additionalReferences)
                        .AddDocument(file, true)
                        .Documents
                        .Single(d => d.Name == file.Name);
                    RunFixAllProvider(diagnosticAnalyzer, codeFixProvider, codeFixTitle, fixAllProvider, document, parseOption, pathToExpected);
                }
            }
        }

        private static Project CreateProject(string fileExtension, string assemblyName,
            AdhocWorkspace workspace, params MetadataReference[] additionalReferences)
        {
            var language = fileExtension == CSharpFileExtension
                ? LanguageNames.CSharp
                : LanguageNames.VisualBasic;

            var project = workspace.CurrentSolution.AddProject(assemblyName, $"{assemblyName}.dll", language)
                .AddMetadataReference(systemAssembly)
                .AddMetadataReference(systemLinqAssembly)
                .AddMetadataReference(systemNetAssembly)
                .AddMetadataReferences(additionalReferences);

            // adding an extra file to the project
            // this won't trigger any issues, but it keeps a reference to the original ParseOption, so
            // if an analyzer/codefix changes the language version, Roslyn throws an ArgumentException
            project = project.AddDocument("ExtraEmptyFile.g" + fileExtension, string.Empty).Project;

            return project;
        }

        private static Project AddDocument(this Project project, FileInfo file,
            bool removeAnalysisComments = false)
        {
            var text = ReadDocument(file.FullName, removeAnalysisComments);

            return project.AddDocument(file.Name, text).Project;
        }

        private static string ReadDocument(string filePath, bool removeAnalysisComments)
        {
            var lines = File.ReadAllText(filePath, Encoding.UTF8)
                .Replace(WindowsLineEnding, UnixLineEnding) // This allows to deal with multiple line endings
                .Split(new[] { UnixLineEnding }, StringSplitOptions.None);

            if (removeAnalysisComments)
            {
                lines = lines.Where(IssueLocationCollector.IsNotIssueLocationLine)
                    .Select(ReplaceNonCompliantComment)
                    .ToArray();
            }

            return string.Join(UnixLineEnding, lines);
        }

        private static string ReplaceNonCompliantComment(string line)
        {
            var match = Regex.Match(line, IssueLocationCollector.ISSUE_LOCATION_PATTERN);
            if (!match.Success)
            {
                return line;
            }

            if (match.Groups["issueType"].Value == "Noncompliant")
            {
                var startIndex = line.IndexOf(match.Groups["issueType"].Value);
                return string.Concat(line.Remove(startIndex), FIXED_MESSAGE);
            }

            return line.Replace(match.Value, string.Empty).TrimEnd();
        }

        #endregion

        #region Analyzer helpers

        private static IEnumerable<ParseOptions> GetParseOptionsAlternatives(ParseOptions options, string fileExtension)
        {
            return GetParseOptionsWithDifferentLanguageVersions(options, fileExtension).Concat(new[] { options });
        }

        private static IEnumerable<ParseOptions> GetParseOptionsWithDifferentLanguageVersions(ParseOptions options, string fileExtension)
        {
            if (fileExtension == CSharpFileExtension)
            {
                if (options == null)
                {
                    var csOptions = new CS.CSharpParseOptions();
                    yield return csOptions.WithLanguageVersion(CS.LanguageVersion.CSharp7);
                    yield return csOptions.WithLanguageVersion(CS.LanguageVersion.CSharp6);
                    yield return csOptions.WithLanguageVersion(CS.LanguageVersion.CSharp5);
                }
                yield break;
            }

            var vbOptions = options as VB.VisualBasicParseOptions ?? new VB.VisualBasicParseOptions();
            yield return vbOptions.WithLanguageVersion(VB.LanguageVersion.VisualBasic15);
            yield return vbOptions.WithLanguageVersion(VB.LanguageVersion.VisualBasic14);
            yield return vbOptions.WithLanguageVersion(VB.LanguageVersion.VisualBasic12);
        }

        internal static IEnumerable<Diagnostic> GetDiagnostics(Compilation compilation,
            IEnumerable<SonarDiagnosticAnalyzer> diagnosticAnalyzers)
        {
            var ids = new HashSet<string>(diagnosticAnalyzers
                .SelectMany(d => d.SupportedDiagnostics.Select(diagnostic => diagnostic.Id)));

            var diagnostics = GetAllDiagnostics(compilation, diagnosticAnalyzers).ToList();
            VerifyNoExceptionThrown(diagnostics);

            return diagnostics.Where(d => ids.Contains(d.Id));
        }

        private static void VerifyNoExceptionThrown(IEnumerable<Diagnostic> diagnostics)
        {
            diagnostics.Where(d => d.Id == AnalyzerFailedDiagnosticId).Should().BeEmpty();
        }

        private static IEnumerable<Diagnostic> GetAllDiagnostics(Compilation compilation,
            IEnumerable<DiagnosticAnalyzer> diagnosticAnalyzers)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                var compilationOptions = compilation.Language == LanguageNames.CSharp
                    ? (CompilationOptions)new CS.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
                    : new VB.VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
                var supportedDiagnostics = diagnosticAnalyzers
                        .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                        .ToList();
                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                    supportedDiagnostics
                        .Select(diagnostic =>
                            new KeyValuePair<string, ReportDiagnostic>(diagnostic.Id, ReportDiagnostic.Warn))
                        .Union(
                            new[]
                            {
                                new KeyValuePair<string, ReportDiagnostic>(AnalyzerFailedDiagnosticId, ReportDiagnostic.Error)
                            }));

                var compilationWithOptions = compilation.WithOptions(compilationOptions);
                var compilationWithAnalyzer = compilationWithOptions
                    .WithAnalyzers(
                        diagnosticAnalyzers.ToImmutableArray(),
                        cancellationToken: tokenSource.Token);

                return compilationWithAnalyzer.GetAllDiagnosticsAsync().Result;
            }
        }

        #endregion

        #region Codefix helper

        private static void RunCodeFixWhileDocumentChanges(SonarDiagnosticAnalyzer diagnosticAnalyzer, CodeFixProvider codeFixProvider,
            string codeFixTitle, Document document, ParseOptions parseOption, string pathToExpected)
        {
            var currentDocument = document;
            CalculateDiagnosticsAndCode(diagnosticAnalyzer, currentDocument, parseOption, out var diagnostics, out var actualCode);

            diagnostics.Should().NotBeEmpty();

            string codeBeforeFix;
            var codeFixExecutedAtLeastOnce = false;

            do
            {
                codeBeforeFix = actualCode;

                var codeFixExecuted = false;
                for (var diagnosticIndexToFix = 0; !codeFixExecuted && diagnosticIndexToFix < diagnostics.Count; diagnosticIndexToFix++)
                {
                    var codeActionsForDiagnostic = GetCodeActionsForDiagnostic(codeFixProvider, currentDocument, diagnostics[diagnosticIndexToFix]);

                    if (TryGetCodeActionToApply(codeFixTitle, codeActionsForDiagnostic, out var codeActionToExecute))
                    {
                        currentDocument = ApplyCodeFix(currentDocument, codeActionToExecute);
                        CalculateDiagnosticsAndCode(diagnosticAnalyzer, currentDocument, parseOption, out diagnostics, out actualCode);

                        codeFixExecutedAtLeastOnce = true;
                        codeFixExecuted = true;
                    }
                }
            } while (codeBeforeFix != actualCode);

            codeFixExecutedAtLeastOnce.Should().BeTrue();

            var actualWithUnixLineEnding = actualCode.Replace(WindowsLineEnding, UnixLineEnding);
            var expectedWithUnixLineEnding = File.ReadAllText(pathToExpected).Replace(WindowsLineEnding, UnixLineEnding);
            actualWithUnixLineEnding.Should().Be(expectedWithUnixLineEnding);
        }

        private static void RunFixAllProvider(SonarDiagnosticAnalyzer diagnosticAnalyzer, CodeFixProvider codeFixProvider,
            string codeFixTitle, FixAllProvider fixAllProvider, Document document, ParseOptions parseOption, string pathToExpected)
        {
            var currentDocument = document;
            CalculateDiagnosticsAndCode(diagnosticAnalyzer, currentDocument, parseOption, out var diagnostics, out var actualCode);

            diagnostics.Should().NotBeEmpty();

            var fixAllDiagnosticProvider = new FixAllDiagnosticProvider(
                codeFixProvider.FixableDiagnosticIds.ToHashSet(),
                (doc, ids, ct) => Task.FromResult(
                    GetDiagnostics(currentDocument.Project.GetCompilationAsync(ct).Result, new[] { diagnosticAnalyzer })),
                null);
            var fixAllContext = new FixAllContext(currentDocument, codeFixProvider, FixAllScope.Document,
                codeFixTitle,
                codeFixProvider.FixableDiagnosticIds,
                fixAllDiagnosticProvider,
                CancellationToken.None);
            var codeActionToExecute = fixAllProvider.GetFixAsync(fixAllContext).Result;

            codeActionToExecute.Should().NotBeNull();

            currentDocument = ApplyCodeFix(currentDocument, codeActionToExecute);

            CalculateDiagnosticsAndCode(diagnosticAnalyzer, currentDocument, parseOption, out diagnostics, out actualCode);

            var actualWithUnixLineEnding = actualCode.Replace(WindowsLineEnding, UnixLineEnding);
            var expectedWithUnixLineEnding = File.ReadAllText(pathToExpected).Replace(WindowsLineEnding, UnixLineEnding);
            actualWithUnixLineEnding.Should().Be(expectedWithUnixLineEnding);
        }

        private static void CalculateDiagnosticsAndCode(SonarDiagnosticAnalyzer diagnosticAnalyzer, Document document, ParseOptions parseOption,
            out List<Diagnostic> diagnostics,
            out string actualCode)
        {
            var project = document.Project;
            if (parseOption != null)
            {
                project = project.WithParseOptions(parseOption);
            }

            diagnostics = GetDiagnostics(project.GetCompilationAsync().Result, new[] { diagnosticAnalyzer }).ToList();
            actualCode = document.GetSyntaxRootAsync().Result.GetText().ToString();
        }

        private static Document ApplyCodeFix(Document document, CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        private static bool TryGetCodeActionToApply(string codeFixTitle, IEnumerable<CodeAction> codeActions,
            out CodeAction codeAction)
        {
            codeAction = codeFixTitle != null
                ? codeActions.SingleOrDefault(action => action.Title == codeFixTitle)
                : codeActions.FirstOrDefault();

            return codeAction != null;
        }

        private static IEnumerable<CodeAction> GetCodeActionsForDiagnostic(CodeFixProvider codeFixProvider, Document document,
            Diagnostic diagnostic)
        {
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);

            codeFixProvider.RegisterCodeFixesAsync(context).Wait();
            return actions;
        }

        #endregion

        #region Suppression

        private static bool isHooked = false;

        private static ConcurrentDictionary<string, int> counters = new ConcurrentDictionary<string, int>();

        private static void HookSuppression()
        {
            if (isHooked)
            {
                return;
            }


            SonarAnalysisContext.ShouldDiagnosticBeReported = (s, d) => { IncrementReportCount(d.Id); return true; };
        }

        private static void UnHookSuppression()
        {
            if (!isHooked)
            {
                return;
            }


            SonarAnalysisContext.ShouldDiagnosticBeReported = null;
        }

        internal static void IncrementReportCount(string ruleId)
        {
            counters.AddOrUpdate(ruleId, addValueFactory: key => 1, updateValueFactory: (key, count) => count + 1);
        }

        private static bool ExtensionMethodsCalledForAllDiagnostics(IEnumerable<DiagnosticAnalyzer> analyzers)
        {
            // In general this check is not very precise, because when the tests are run in parallel
            // we cannot determine which diagnostic was reported from which analyzer instance. In other
            // words, we cannot distinguish between diagnostics reported from different tests. That's
            // why we require each diagnostic to be reported through the extension methods at least once.
            return analyzers
                .SelectMany(a => a.SupportedDiagnostics)
                .Select(d => counters.GetValueOrDefault(d.Id))
                .Any(count => count > 0);
        }
        #endregion

        private class DocumentInfo
        {
            public DocumentInfo(FileInfo file) :
                this(file.Name, File.ReadAllText(file.FullName))
            {
            }

            public DocumentInfo(string name, string content)
            {
                Name = name;
                Content = content;
            }

            public string Name { get; }
            public string Content { get; }
        }
    }
}
