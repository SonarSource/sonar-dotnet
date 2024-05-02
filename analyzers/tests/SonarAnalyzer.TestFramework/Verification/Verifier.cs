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
using System.Xml.Linq;
using Google.Protobuf;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
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
        if (builder.IsRazor)
        {
            if (builder.ParseOptions.IsEmpty)
            {
                throw new ArgumentException($"{nameof(builder.IsRazor)} was set. {nameof(ParseOptions)} must be specified.");
            }
            else if (language != AnalyzerLanguage.CSharp)
            {
                throw new ArgumentException($"{nameof(builder.IsRazor)} was set for {language} analyzer. Only C# is supported.");
            }
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
        var document = CreateProject(false).FindDocument(Path.Combine(builder.BasePath ?? string.Empty, Path.GetFileName(builder.Paths.Single())));
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
        return builder.IsRazor
            ? CompileRazor()
            : CreateProject(concurrentAnalysis).Solution.Compile(builder.ParseOptions.ToArray()).Select(x => new CompilationData(x, null));
    }

    private IEnumerable<CompilationData> CompileRazor()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (_, failure) => Console.WriteLine(failure.Diagnostic);

        // Copy razor project directory and test case files to a temporary build location
        var tempPath = Path.Combine(Path.GetTempPath(), $"ut-razor-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            foreach (var langVersion in builder.ParseOptions.Cast<CSharpParseOptions>().Select(LangVersion))
            {
                var projectRoot = Path.Combine(tempPath, langVersion);
                Directory.CreateDirectory(projectRoot);
                var csProjPath = PrepareRazorProject(projectRoot, langVersion);
                var razorFiles = PrepareRazorFiles(projectRoot);
                // To avoid reference loading issues, ensure that the project references are restored before compilation.
                if (RestorePackages(csProjPath, projectRoot))
                {
                    yield return new(workspace.OpenProjectAsync(csProjPath).Result.GetCompilationAsync().Result, razorFiles.ToArray());
                }
                else
                {
                    throw new InvalidOperationException($"Failed to restore project {csProjPath}.");
                }
            }
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }

        static string LangVersion(CSharpParseOptions options) =>
            options.LanguageVersion switch
            {
                // 5 and 6 should not be needed here
                // 7 also does not support with Nullable context
                // 8 and 9 do not support global using directives
                LanguageVersion.CSharp10 => "10.0",
                LanguageVersion.CSharp11 => "11.0",
                LanguageVersion.CSharp12 => "12.0",
                _ => throw new NotSupportedException($"Unexpected language version {options.LanguageVersion}. Update this switch to add the new version.")
            };
    }

    private string PrepareRazorProject(string projectRoot, string langVersion)
    {
        // To improve: Paths are currently relative to entry assembly => needs to be duplicated in different projects for now.
        foreach (var file in Directory.GetFiles(@"TestCases\Razor\EmptyProject"))
        {
            File.Copy(file, Path.Combine(projectRoot, Path.GetFileName(file)));
        }
        var csProjPath = Path.Combine(projectRoot, "EmptyProject.csproj");
        var xml = XElement.Load(csProjPath);
        //xml.Descendants("LangVersion").Single().Value = langVersion;
        //var references = xml.Descendants("ItemGroup").Single();
        //foreach (var reference in builder.References)
        //{
        //    references.Add(new XElement("Reference", new XAttribute("Include", reference.Display)));
        //}
        xml.Save(csProjPath);
        return csProjPath;
    }

    private List<string> PrepareRazorFiles(string projectRoot)
    {
        var razorFiles = new List<string>();
        var snippetCount = 0;
        // To improve: Paths are currently relative to entry assembly => needs to be duplicated in different projects for now.
        foreach (var file in builder.Paths.Select(TestCasePath))
        {
            var filePath = Path.Combine(projectRoot, Path.GetFileName(file));
            File.Copy(file, filePath);
            if (IsRazorOrCshtml(filePath))
            {
                razorFiles.Add(filePath);
            }
        }
        foreach (var snippet in builder.Snippets)
        {
            var filePath = Path.Combine(projectRoot, snippet.FileName ?? $"snippet.{snippetCount++}{language.FileExtension}");
            File.WriteAllText(filePath, snippet.Content);
            if (IsRazorOrCshtml(filePath))
            {
                razorFiles.Add(filePath);
            }
        }
        return razorFiles;
    }

    private ProjectBuilder CreateProject(bool concurrentAnalysis)
    {
        var paths = builder.Paths.Select(TestCasePath).ToList();
        var contentFilePaths = paths.Where(IsRazorOrCshtml).ToArray();
        var sourceFilePaths = paths.Except(contentFilePaths).ToArray();
        var editorConfigGenerator = new EditorConfigGenerator(TestCaseDirectory());
        return SolutionBuilder.Create()
            .AddProject(language, builder.OutputKind)
            .AddDocuments(sourceFilePaths)
            .AddAdditionalDocuments(contentFilePaths)
            .AddDocuments(concurrentAnalysis && builder.AutogenerateConcurrentFiles ? CreateConcurrencyTest(sourceFilePaths) : [])
            .AddAdditionalDocuments(concurrentAnalysis && builder.AutogenerateConcurrentFiles ? CreateConcurrencyTest(contentFilePaths) : [])
            .AddAnalyzerReferences(contentFilePaths.Length > 0 ? SourceGeneratorProvider.SourceGenerators : [])
            .AddSnippets(builder.Snippets.ToArray())
            .AddReferences(builder.References)
            .AddAnalyzerConfigDocument(Path.Combine(TestCaseDirectory(), ".editorconfig"), editorConfigGenerator.Generate(contentFilePaths));
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

    private static bool RestorePackages(string path, string workingDirectory)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
            {
                FileName = Environment.GetEnvironmentVariable("DOTNET_PATH") ?? "dotnet",
                Arguments = $"restore \"{path}\"",
                WorkingDirectory = workingDirectory
            };

        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    public sealed record CompilationData(Compilation Compilation, string[] AdditionalSourceFiles);
}
