/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
using SonarAnalyzer.Core.AnalysisContext;
using static SonarAnalyzer.Core.Analyzers.DiagnosticDescriptorFactory;

namespace SonarAnalyzer.Core.Extensions;

public static class IAnalysisContextExtensions
{
    private const string RazorGeneratedFileSuffix = "_razor.g.cs";

    private static readonly ConditionalWeakTable<Compilation, ImmutableHashSet<string>> UnchangedFilesCache = new();
    private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<string, bool>> FileInclusionCache = new();

    /// <summary>
    /// Reads the properties from the SonarLint.xml file and caches the result for the scope of this analysis.
    /// </summary>
    public static SonarLintXmlReader SonarLintXml<T>(this T context) where T : IAnalysisContext =>
        context.Options.SonarLintXml(context.AnalysisContext);

    public static bool IsRazorAnalysisEnabled<T>(this T context) where T : IAnalysisContext =>
        context.AnalysisContext.IsRazorAnalysisEnabled(context.Options, context.Compilation);

    public static bool IsTestProject<T>(this T context) where T : IAnalysisContext
    {
        var projectType = context.ProjectConfiguration().ProjectType;
        return projectType == ProjectType.Unknown
            ? context.Compilation.IsTest()              // SonarLint, NuGet or Scanner <= 5.0
            : projectType == ProjectType.Test;          // Scanner >= 5.1 does authoritative decision that we follow
    }

    /// <summary>
    /// Reads configuration from SonarProjectConfig.xml file and caches the result for scope of this analysis.
    /// </summary>
    public static ProjectConfigReader ProjectConfiguration<T>(this T context) where T : IAnalysisContext =>
        context.AnalysisContext.ProjectConfiguration(context.Options);

    public static bool HasMatchingScope<T>(this T context, ImmutableArray<DiagnosticDescriptor> descriptors) where T : IAnalysisContext
    {
        // Performance: Don't use descriptors.Any(HasMatchingScope), the delegate creation allocates too much memory. https://github.com/SonarSource/sonar-dotnet/issues/7438
        foreach (var descriptor in descriptors)
        {
            if (context.HasMatchingScope(descriptor))
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasMatchingScope<T>(this T context, DiagnosticDescriptor descriptor) where T : IAnalysisContext
    {
        // MMF-2297: Test Code as 1st Class Citizen is not ready on server side yet.
        // ScannerRun: Only utility rules and rules with TEST-ONLY scope are executed for test projects for now.
        // SonarLint & Standalone NuGet & Internal styling rules Txxxx: Respect the scope as before.
        return context.IsTestProject()
            ? ContainsTag(TestSourceScopeTag) && !(descriptor.Id.StartsWith("S") && context.ProjectConfiguration().IsScannerRun && ContainsTag(MainSourceScopeTag) && !ContainsTag(UtilityTag))
            : ContainsTag(MainSourceScopeTag);

        bool ContainsTag(string tag) =>
            descriptor.CustomTags.Contains(tag);
    }

    /// <param name="tree">Tree to decide on. Can be null for Symbol-based and Compilation-based scenarios. And we want to analyze those too.</param>
    /// <param name="generatedCodeRecognizer">When set, generated trees are analyzed only when language-specific 'analyzeGeneratedCode' configuration property is also set.</param>
    public static bool ShouldAnalyzeTree<T>(this T context, SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) where T : IAnalysisContext =>
        context.SonarLintXml() is var sonarLintXml
        && (generatedCodeRecognizer is null
            || sonarLintXml.AnalyzeGeneratedCode(context.Compilation.Language)
            || !tree.IsConsideredGenerated(generatedCodeRecognizer, context.IsRazorAnalysisEnabled()))
        && (tree is null || (!context.IsUnchanged(tree) && !context.IsExcluded(sonarLintXml, tree.FilePath)));

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = true, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    public static bool IsUnchanged<T>(this T context, SyntaxTree tree) where T : IAnalysisContext
    {
        // Hot path: Use TryGetValue to prevent the allocation of the GetValue factory delegate in the common case
        var unchangedFiles = UnchangedFilesCache.TryGetValue(context.Compilation, out var unchangedFilesFromCache)
            ? unchangedFilesFromCache
            : UnchangedFilesCache.GetValue(context.Compilation, _ => CreateUnchangedFilesHashSet(context.ProjectConfiguration()));
        return unchangedFiles.Contains(MapFilePath(tree));
    }

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    private static bool IsExcluded<T>(this T context, SonarLintXmlReader sonarLintXml, string filePath) where T : IAnalysisContext
    {
        // If ProjectType is not 'Unknown' it means we are in S4NET context and all files are analyzed.
        // If ProjectType is 'Unknown' then we are in SonarLint or NuGet context and we need to check if the file has been excluded from analysis through SonarLint.xml.
        if (context.ProjectConfiguration().ProjectType == ProjectType.Unknown)
        {
            var fileInclusionCache = FileInclusionCache.GetOrCreateValue(context.Compilation);
            // Hot path: Don't use GetOrAdd with the value factory parameter. It allocates a delegate which causes GC pressure.
            var isIncluded = fileInclusionCache.TryGetValue(filePath, out var result)
                ? result
                : fileInclusionCache.GetOrAdd(filePath, sonarLintXml.IsFileIncluded(filePath, context.IsTestProject()));
            return !isIncluded;
        }
        return false;
    }

    private static ImmutableHashSet<string> CreateUnchangedFilesHashSet(ProjectConfigReader config) =>
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, config.AnalysisConfig?.UnchangedFiles() ?? []);

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
