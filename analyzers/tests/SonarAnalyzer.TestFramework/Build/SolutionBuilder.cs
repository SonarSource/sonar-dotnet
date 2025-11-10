/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Build;

public readonly struct SolutionBuilder
{
    private const string GeneratedAssemblyName = "project";

    private static readonly IEnumerable<GlobalImport> DefaultGlobalImportsVisualBasic = GlobalImport.Parse(
        "Microsoft.VisualBasic",
        "System",
        "System.Collections",
        "System.Collections.Generic",
        "System.Data",
        "System.Diagnostics",
        "System.Linq",
        "System.Xml.Linq",
        "System.Threading.Tasks");
    private readonly Solution solution;

    public IReadOnlyList<ProjectId> ProjectIds => solution.ProjectIds;

    private SolutionBuilder(Solution solution) =>
        this.solution = solution;

    public ProjectBuilder AddProject(AnalyzerLanguage language, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary) =>
        AddProject(language, $"{GeneratedAssemblyName}{ProjectIds.Count}", outputKind);

    public static SolutionBuilder Create() =>
        FromSolution(new AdhocWorkspace().CurrentSolution);

    public static SolutionBuilder CreateSolutionFromPath(string path, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, IEnumerable<MetadataReference> additionalReferences = null) =>
        Create()
            .AddProject(AnalyzerLanguage.FromPath(path), outputKind: outputKind)
            .AddDocument(path)
            .AddReferences(additionalReferences)
            .Solution;

    public static SolutionBuilder FromSolution(Solution solution) =>
        new(solution);

    public ImmutableArray<Compilation> Compile(params ParseOptions[] parseOptions) =>
        solution.Projects.SelectMany(x => Compile(x, parseOptions)).ToImmutableArray();

    private static IEnumerable<Compilation> Compile(Project project, ParseOptions[] parseOptions) =>
        parseOptions.OrDefault(project.Language).Select(x => project.WithParseOptions(x).GetCompilationAsync().Result);

    private ProjectBuilder AddProject(AnalyzerLanguage language, string projectName, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        var project = solution.AddProject(projectName, projectName, language.LanguageName);
        var compilationOptions = project.CompilationOptions.WithOutputKind(outputKind);
        compilationOptions = language.LanguageName switch
        {
            LanguageNames.CSharp => ((CSharpCompilationOptions)compilationOptions).WithAllowUnsafe(true),
            LanguageNames.VisualBasic => ((VisualBasicCompilationOptions)compilationOptions).WithGlobalImports(DefaultGlobalImportsVisualBasic),
            _ => throw new UnexpectedLanguageException(language)
        };
        project = project.WithCompilationOptions(compilationOptions);

        var projectBuilder = ProjectBuilder.FromProject(project).AddReferences(MetadataReferenceFacade.ProjectDefaultReferences);
        return language == AnalyzerLanguage.VisualBasic
            ? projectBuilder.AddReferences(MetadataReferenceFacade.MicrosoftVisualBasic)
            : projectBuilder;
    }
}
