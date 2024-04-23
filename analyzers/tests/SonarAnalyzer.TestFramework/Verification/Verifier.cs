/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Google.Protobuf;
using Microsoft.CodeAnalysis.CodeFixes;
using SonarAnalyzer.Rules;
using SonarAnalyzer.TestFramework.Build;

namespace SonarAnalyzer.TestFramework.Verification;

internal class Verifier
{
    private const string TestCases = "TestCases";

    private static readonly Regex ImportsRegexVB = new(@"^\s*Imports\s+.+$", RegexOptions.Multiline | RegexOptions.RightToLeft, RegexConstants.DefaultTimeout);
    private readonly VerifierBuilder builder;
    private readonly DiagnosticAnalyzer[] analyzers;
    private readonly SonarCodeFix codeFix;
    private readonly AnalyzerLanguage language;
    private readonly string[] onlyDiagnosticIds;

    public Verifier(VerifierBuilder builder)
    {
        this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
        onlyDiagnosticIds = builder.OnlyDiagnostics.Select(x => x.Id).ToArray();
        analyzers = builder.Analyzers.Select(x => x()).ToArray();
        if (!analyzers.Any())
        {
            throw new ArgumentException($"{nameof(builder.Analyzers)} cannot be empty. Use {nameof(VerifierBuilder)}<TAnalyzer> instead or add at least one analyzer using {nameof(builder)}.{nameof(builder.AddAnalyzer)}().");
        }
        if (Array.Exists(analyzers, x => x == null))
        {
            throw new ArgumentException("Analyzer instance cannot be null.");
        }
        var allLanguages = analyzers.SelectMany(x => x.GetType().GetCustomAttributes<DiagnosticAnalyzerAttribute>()).SelectMany(x => x.Languages).Distinct().ToArray();
        if (allLanguages.Length > 1)
        {
            throw new ArgumentException($"All {nameof(builder.Analyzers)} must declare the same language in their DiagnosticAnalyzerAttribute.");
        }
        language = AnalyzerLanguage.FromName(allLanguages.Single());
        if (!builder.Paths.Any() && !builder.Snippets.Any())
        {
            throw new ArgumentException($"{nameof(builder.Paths)} cannot be empty. Add at least one file using {nameof(builder)}.{nameof(builder.AddPaths)}() or {nameof(builder.AddSnippet)}().");
        }
        foreach (var path in builder.Paths)
        {
            ValidateExtension(path);
        }
        if (builder.ProtobufPath is not null)
        {
            ValidateSingleAnalyzer(nameof(builder.ProtobufPath));
            if (analyzers.Single() is not UtilityAnalyzerBase)
            {
                throw new ArgumentException($"{analyzers.Single().GetType().Name} does not inherit from {nameof(UtilityAnalyzerBase)}.");
            }
        }
        if (builder.CodeFix is not null)
        {
            codeFix = builder.CodeFix();
            ValidateCodeFix();
        }
    }

    public void Verify()    // This should never have any arguments
    {
        if (codeFix != null)
        {
            throw new InvalidOperationException($"Cannot use {nameof(Verify)} with {nameof(builder.CodeFix)} set.");
        }
        foreach (var compilation in Compile(builder.ConcurrentAnalysis))
        {
            DiagnosticVerifier.Verify(compilation.Compilation, analyzers, builder.ErrorBehavior, builder.AdditionalFilePath, onlyDiagnosticIds, compilation.AdditionalSourceFiles);
        }
    }

    public void VerifyNoIssueReported()    // This should never have any arguments
    {
        foreach (var compilation in Compile(builder.ConcurrentAnalysis))
        {
            foreach (var analyzer in analyzers)
            {
                DiagnosticVerifier.VerifyNoIssueReported(compilation.Compilation, analyzer, builder.ErrorBehavior, builder.AdditionalFilePath, onlyDiagnosticIds);
            }
        }
    }

    public void VerifyCodeFix()     // This should never have any arguments
    {
        _ = codeFix ?? throw new InvalidOperationException($"{nameof(builder.CodeFix)} was not set.");
        var document = CreateProject(false).FindDocument(Path.GetFileName(builder.Paths.Single()));
        var codeFixVerifier = new CodeFixVerifier(analyzers.Single(), codeFix, document, builder.CodeFixTitle);
        var fixAllProvider = codeFix.GetFixAllProvider();
        foreach (var parseOptions in builder.ParseOptions.OrDefault(language.LanguageName))
        {
            codeFixVerifier.VerifyWhileDocumentChanges(parseOptions, TestCasePath(builder.CodeFixedPath));
            if (fixAllProvider is not null)
            {
                codeFixVerifier.VerifyFixAllProvider(fixAllProvider, parseOptions, TestCasePath(builder.CodeFixedPathBatch ?? builder.CodeFixedPath));
            }
        }
    }

    public void VerifyUtilityAnalyzerProducesEmptyProtobuf()     // This should never have any arguments
    {
        foreach (var compilation in Compile(false))
        {
            DiagnosticVerifier.Verify(compilation.Compilation, analyzers.Single(), builder.AdditionalFilePath);
            new FileInfo(builder.ProtobufPath).Length.Should().Be(0, "protobuf file should be empty");
        }
    }

    public void VerifyUtilityAnalyzer<TMessage>(Action<IReadOnlyList<TMessage>> verifyProtobuf)
        where TMessage : IMessage<TMessage>, new()
    {
        foreach (var compilation in Compile(false))
        {
            DiagnosticVerifier.Verify(compilation.Compilation, analyzers.Single(), builder.AdditionalFilePath);
            verifyProtobuf(ReadProtobuf().ToList());
        }

        IEnumerable<TMessage> ReadProtobuf()
        {
            using var input = File.OpenRead(builder.ProtobufPath);
            var parser = new MessageParser<TMessage>(() => new TMessage());
            while (input.Position < input.Length)
            {
                yield return parser.ParseDelimitedFrom(input);
            }
        }
    }

    public IEnumerable<CompilationData> Compile(bool concurrentAnalysis)
    {
        using var scope = new EnvironmentVariableScope { EnableConcurrentAnalysis = concurrentAnalysis };
        return CreateProject(concurrentAnalysis).Solution.Compile(builder.ParseOptions.ToArray()).Select(x => new CompilationData(x, null));
    }

    private ProjectBuilder CreateProject(bool concurrentAnalysis)
    {
        var paths = builder.Paths.Select(TestCasePath).ToList();
        var contentFilePaths = paths.Where(IsRazorOrCshtml).ToArray();
        var sourceFilePaths = paths.Except(contentFilePaths).ToArray();
        var contentSnippets = builder.Snippets.Where(x => IsRazorOrCshtml(x.FileName)).ToArray();
        var sourceSnippets = builder.Snippets.Where(x => !contentSnippets.Contains(x)).ToArray(); // Cannot use Except because it Distincts the elements

        contentSnippets = contentSnippets.Select(x =>
        {
            var tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestCases", x.FileName);
            File.WriteAllText(tempFilePath, x.Content);
            return x with { FileName = tempFilePath };
        }).ToArray();

        var editorConfigGenerator = new EditorConfigGenerator(Directory.GetCurrentDirectory());
        var hasContentDocuments = contentFilePaths.Length > 0 || contentSnippets.Length > 0;
        concurrentAnalysis = !hasContentDocuments && concurrentAnalysis; // Concurrent analysis is not supported for Razor or cshtml files due to namespace issues
        var concurrentContentFiles = concurrentAnalysis && builder.AutogenerateConcurrentFiles ? CreateConcurrencyTest(contentFilePaths) : [];
        return SolutionBuilder.Create()
            .AddProject(language, builder.OutputKind)
            .AddDocuments(sourceFilePaths)
            .AddAdditionalDocuments(contentFilePaths)
            .AddDocuments(concurrentAnalysis && builder.AutogenerateConcurrentFiles ? CreateConcurrencyTest(sourceFilePaths) : [])
            .AddAdditionalDocuments(concurrentContentFiles)
            .AddAnalyzerReferences(hasContentDocuments ? SourceGeneratorProvider.SourceGenerators : [])
            .AddSnippets(sourceSnippets)
            .AddAdditionalDocuments(contentSnippets.Length > 0 ? contentSnippets.Select(x => x.FileName) : [])
            .AddReferences(builder.References)
            .AddReferences(hasContentDocuments ? NuGetMetadataReference.MicrosoftAspNetCoreAppRef("7.0.17") : [])
            .AddReferences(hasContentDocuments ? NuGetMetadataReference.SystemTextEncodingsWeb("7.0.0") : [])
            .AddAnalyzerConfigDocument(
                Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig"),
                editorConfigGenerator.Generate(concurrentContentFiles.Concat(contentFilePaths).Concat(contentSnippets.Select(x => x.FileName))));
    }

    private IEnumerable<string> CreateConcurrencyTest(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var newPath = Path.ChangeExtension(path, ".Concurrent" + Path.GetExtension(path));
            var content = File.ReadAllText(path, Encoding.UTF8);
            File.WriteAllText(newPath, InsertConcurrentNamespace(content));
            yield return newPath;
        }
    }

    private string InsertConcurrentNamespace(string content)
    {
        return language.LanguageName switch
        {
            LanguageNames.CSharp => $"namespace AppendedNamespaceForConcurrencyTest {{ {content} {Environment.NewLine}}}",  // Last line can be a comment
            LanguageNames.VisualBasic => content.Insert(ImportsIndexVB(), "Namespace AppendedNamespaceForConcurrencyTest : ") + Environment.NewLine + " : End Namespace",
            _ => throw new UnexpectedLanguageException(language)
        };

        int ImportsIndexVB() =>
            ImportsRegexVB.Match(content) is { Success: true } match ? match.Index + match.Length + 1 : 0;
    }

    private string TestCaseDirectory() =>
        Path.GetFullPath(builder.BasePath == null ? TestCases : Path.Combine(TestCases, builder.BasePath));

    private string TestCasePath(string fileName) =>
        Path.Combine(TestCaseDirectory(), fileName);

    private void ValidateSingleAnalyzer(string propertyName)
    {
        if (builder.Analyzers.Length != 1)
        {
            throw new ArgumentException($"When {propertyName} is set, {nameof(builder.Analyzers)} must contain only 1 analyzer, but {analyzers.Length} were found.");
        }
    }

    private void ValidateExtension(string path)
    {
        if (!Path.GetExtension(path).Equals(language.FileExtension, StringComparison.OrdinalIgnoreCase) && !IsRazorOrCshtml(path))
        {
            throw new ArgumentException($"Path '{path}' doesn't match {language.LanguageName} file extension '{language.FileExtension}'.");
        }
    }

    private void ValidateCodeFix()
    {
        _ = builder.CodeFixedPath ?? throw new ArgumentException($"{nameof(builder.CodeFixedPath)} was not set.");
        ValidateSingleAnalyzer(nameof(builder.CodeFix));
        if (builder.Paths.Length != 1)
        {
            throw new ArgumentException($"{nameof(builder.Paths)} must contain only 1 file, but {builder.Paths.Length} were found.");
        }
        if (builder.Snippets.Any())
        {
            throw new ArgumentException($"{nameof(builder.Snippets)} must be empty when {nameof(builder.CodeFix)} is set.");
        }
        ValidateExtension(builder.CodeFixedPath);
        if (builder.CodeFixedPathBatch is not null)
        {
            ValidateExtension(builder.CodeFixedPathBatch);
        }
        if (codeFix.GetType().GetCustomAttribute<ExportCodeFixProviderAttribute>() is { } codeFixAttribute)
        {
            if (codeFixAttribute.Languages.Single() != language.LanguageName)
            {
                throw new ArgumentException($"{analyzers.Single().GetType().Name} language {language.LanguageName} does not match {codeFix.GetType().Name} language.");
            }
        }
        else
        {
            throw new ArgumentException($"{codeFix.GetType().Name} does not have {nameof(ExportCodeFixProviderAttribute)}.");
        }
        if (!analyzers.Single().SupportedDiagnostics.Select(x => x.Id).Intersect(codeFix.FixableDiagnosticIds).Any())
        {
            throw new ArgumentException($"{analyzers.Single().GetType().Name} does not support diagnostics fixable by the {codeFix.GetType().Name}.");
        }
    }

    private static bool IsRazorOrCshtml(string path) =>
        Path.GetExtension(path) is { } extension
        && (extension.Equals(".razor", StringComparison.OrdinalIgnoreCase) || extension.Equals(".cshtml", StringComparison.OrdinalIgnoreCase));

    public sealed record CompilationData(Compilation Compilation, string[] AdditionalSourceFiles);
}
