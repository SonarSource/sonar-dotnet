/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.TestFramework.Build;

public static class ProjectCompiler
{
    private static readonly Lazy<ImmutableArray<ISourceGenerator>> Generators = new(LoadGenerators);

    public static Compilation Compile(Project project, ParseOptions parseOptions = null)
    {
        parseOptions ??= project.ParseOptions;
        var compilation = project.WithParseOptions(parseOptions).GetCompilationAsync().Result;
        var razorFiles = project.AdditionalDocuments
            .Where(x => IsRazorFile(x.Name))
            .Select(x => (AdditionalText)new WorkspaceAdditionalText(x.Name, x.GetTextAsync().Result))
            .ToList();
        if (razorFiles.Count == 0)
        {
            return compilation;
        }
        else
        {
            GeneratorDriver driver = CSharpGeneratorDriver.Create(Generators.Value, razorFiles, (CSharpParseOptions)parseOptions, new RazorAnalyzerConfigOptionsProvider(Directory.GetCurrentDirectory()));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            if (diagnostics.FirstOrDefault(x => x.Id == "CS8785") is { } generatorFailure)
            {
                throw new InvalidOperationException($"The Razor source generator failed: {generatorFailure}");
            }
            return outputCompilation;
        }
    }

    private static bool IsRazorFile(string path) =>
        path is not null
        && (path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase));

    private static ImmutableArray<ISourceGenerator> LoadGenerators()
    {
        var generators = new List<ISourceGenerator>();
        var failures = new List<string>();
        foreach (var reference in SdkPathProvider.SourceGenerators)
        {
            reference.AnalyzerLoadFailed += OnLoadFailed;
            try
            {
                generators.AddRange(reference.GetGenerators(LanguageNames.CSharp));
            }
            finally
            {
                reference.AnalyzerLoadFailed -= OnLoadFailed;
            }
            void OnLoadFailed(object _, AnalyzerLoadFailureEventArgs e) =>
                failures.Add($"{reference.FullPath}: {e.ErrorCode} {e.Message} {e.Exception?.Message}".Trim());
        }
        if (generators.Count == 0)
        {
            throw new InvalidOperationException(failures.Count > 0
                ? $"Failed to load the Razor source generator(s): {string.Join("; ", failures)}. There may be a mismatch between the Razor compiler and the referenced Roslyn version."
                : $"No Razor source generators were found in {string.Join("; ", SdkPathProvider.SourceGenerators.Select(x => x.FullPath))}.");
        }
        return generators.ToImmutableArray();
    }

    private sealed class WorkspaceAdditionalText : AdditionalText
    {
        private readonly SourceText text;

        public override string Path { get; }

        public WorkspaceAdditionalText(string path, SourceText text)
        {
            Path = path;
            this.text = text;
        }

        public override SourceText GetText(CancellationToken cancellationToken = default) => text;
    }
}
