/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace SonarAnalyzer.AnalysisContext;

public class SonarAnalysisContextBase
{
    protected static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<string, bool>> FileInclusionCache = new();
    protected static readonly ConditionalWeakTable<Compilation, ImmutableHashSet<string>> UnchangedFilesCache = new();

    protected SonarAnalysisContextBase() { }
}

public abstract class SonarAnalysisContextBase<TContext> : SonarAnalysisContextBase, IAnalysisContext
{
    private const string RazorGeneratedFileSuffix = "_razor.g.cs";

    public abstract Compilation Compilation { get; }
    public abstract AnalyzerOptions Options { get; }
    public abstract CancellationToken Cancel { get; }

    public SonarAnalysisContext AnalysisContext { get; }
    public TContext Context { get; }

    protected SonarAnalysisContextBase(SonarAnalysisContext analysisContext, TContext context)
    {
        AnalysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
        Context = context;
    }

    /// <param name="tree">Tree to decide on. Can be null for Symbol-based and Compilation-based scenarios. And we want to analyze those too.</param>
    /// <param name="generatedCodeRecognizer">When set, generated trees are analyzed only when language-specific 'analyzeGeneratedCode' configuration property is also set.</param>
    public bool ShouldAnalyzeTree(SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        this.SonarLintXml() is var sonarLintXml
        && (generatedCodeRecognizer is null
            || sonarLintXml.AnalyzeGeneratedCode(Compilation.Language)
            || !tree.IsConsideredGenerated(generatedCodeRecognizer, this.IsRazorAnalysisEnabled()))
        && (tree is null || (!IsUnchanged(tree) && !IsExcluded(sonarLintXml, tree.FilePath)));

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = true, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    public bool IsUnchanged(SyntaxTree tree)
    {
        // Hot path: Use TryGetValue to prevent the allocation of the GetValue factory delegate in the common case
        var unchangedFiles = UnchangedFilesCache.TryGetValue(Compilation, out var unchangedFilesFromCache)
            ? unchangedFilesFromCache
            : UnchangedFilesCache.GetValue(Compilation, _ => CreateUnchangedFilesHashSet());
        return unchangedFiles.Contains(MapFilePath(tree));
    }

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    private bool IsExcluded(SonarLintXmlReader sonarLintXml, string filePath)
    {
        // If ProjectType is not 'Unknown' it means we are in S4NET context and all files are analyzed.
        // If ProjectType is 'Unknown' then we are in SonarLint or NuGet context and we need to check if the file has been excluded from analysis through SonarLint.xml.
        if (this.ProjectConfiguration().ProjectType == ProjectType.Unknown)
        {
            var fileInclusionCache = FileInclusionCache.GetOrCreateValue(Compilation);
            // Hot path: Don't use GetOrAdd with the value factory parameter. It allocates a delegate which causes GC pressure.
            var isIncluded = fileInclusionCache.TryGetValue(filePath, out var result)
                ? result
                : fileInclusionCache.GetOrAdd(filePath, sonarLintXml.IsFileIncluded(filePath, this.IsTestProject()));
            return !isIncluded;
        }
        return false;
    }

    private ImmutableHashSet<string> CreateUnchangedFilesHashSet() =>
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, this.ProjectConfiguration().AnalysisConfig?.UnchangedFiles() ?? Array.Empty<string>());

    private static string MapFilePath(SyntaxTree tree) =>
        // Currently only .razor file hashes are stored in the cache.
        //
        // For a file like `Pages\Component.razor`, the compiler generated path has the following pattern:
        // Microsoft.NET.Sdk.Razor.SourceGenerators\Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator\Pages_Component_razor.g.cs
        // In order to avoid rebuilding the original file path we have to read it from the pragma directive.
        //
        // This should be updated for .cshtml files as well once https://github.com/SonarSource/sonar-dotnet/issues/8032 is done.
        tree.FilePath.EndsWith(RazorGeneratedFileSuffix, StringComparison.OrdinalIgnoreCase)
            ? tree.GetOriginalFilePath()
            : tree.FilePath;
}
