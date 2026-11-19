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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Roslyn.Utilities;
using SonarAnalyzer.Core.Configuration;
using static SonarAnalyzer.Core.Analyzers.DiagnosticDescriptorFactory;

namespace SonarAnalyzer.Core.Extensions;

public static class IAnalysisContextExtensions
{
    private const string RazorGeneratedFileSuffix = "_razor.g.cs";
    private const string RazorDeclGeneratedFileSuffix = "_razor.decl.g.cs";

    private static readonly ConditionalWeakTable<Compilation, ImmutableHashSet<string>> UnchangedFilesCache = new();
    private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<string, bool>> FileInclusionCache = new();

    extension<T>(T context) where T : IAnalysisContext // Don't change to (this IAnalysisContext context) because this would cause boxing
    {
        /// <summary>
        /// Reads the properties from the SonarLint.xml file and caches the result for the scope of this analysis.
        /// </summary>
        public SonarLintXmlReader SonarLintXml() =>
            context.Options.SonarLintXml(context.AnalysisContext);

        public bool IsRazorAnalysisEnabled() =>
            context.AnalysisContext.IsRazorAnalysisEnabled(context.Options, context.Compilation);

        public bool IsTestProject()
        {
            var projectType = context.ProjectConfiguration().ProjectType;
            return projectType == ProjectType.Unknown
                ? context.Compilation.IsTest()              // SonarLint, NuGet or Scanner <= 5.0
                : projectType == ProjectType.Test;          // Scanner >= 5.1 does authoritative decision that we follow
        }

        /// <summary>
        /// Reads configuration from SonarProjectConfig.xml file and caches the result for scope of this analysis.
        /// </summary>
        public ProjectConfigReader ProjectConfiguration() =>
            context.AnalysisContext.ProjectConfiguration(context.Options);

        public bool HasMatchingScope(ImmutableArray<DiagnosticDescriptor> descriptors)
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

        public bool HasMatchingScope(DiagnosticDescriptor descriptor)
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
        public bool ShouldAnalyzeTree(SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) =>
            context.SonarLintXml() is var sonarLintXml
            && (generatedCodeRecognizer is null
                || sonarLintXml.AnalyzeGeneratedCode(context.Compilation.Language)
                || !tree.IsConsideredGenerated(generatedCodeRecognizer, context.IsRazorAnalysisEnabled()))
            && (tree is null || (!context.IsUnchanged(tree) && !context.IsExcluded(sonarLintXml, tree.FilePath)));

        /// <summary>
        /// A tree is unchanged when all the files it maps to are unchanged. A generated Razor tree can map to several files.
        /// </summary>
        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
        public bool IsUnchanged(SyntaxTree tree)
        {
            var unchangedFiles = context.UnchangedFiles();
            if (unchangedFiles.IsEmpty)
            {
                return false;   // Don't read the mapped files outside of incremental PR analysis
            }
            return IsRazorGeneratedTree(tree)
                ? IsRazorTreeUnchanged(tree, unchangedFiles)
                : unchangedFiles.Contains(tree.FilePath);
        }

        /// <summary>
        /// Checks a single file. Utility analyzers report their messages for the file that owns the tree, so they don't need to consider all mapped files.
        /// </summary>
        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
        public bool IsUnchanged(string filePath) =>
            context.UnchangedFiles().Contains(filePath);

        /// <summary>
        /// False outside of incremental PR analysis. Lets callers skip computing a file path when no file can be unchanged.
        /// </summary>
        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
        public bool HasUnchangedFiles() =>
            !context.UnchangedFiles().IsEmpty;

        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
        private ImmutableHashSet<string> UnchangedFiles() =>
            // Hot path: Use TryGetValue to prevent the allocation of the GetValue factory delegate in the common case.
            // The factory lambda lives in a separate method, because a closure over 'context' would be allocated on each call of this method.
            UnchangedFilesCache.TryGetValue(context.Compilation, out var unchangedFilesFromCache)
                ? unchangedFilesFromCache
                : UnchangedFilesGetOrAdd(context.Compilation, context.ProjectConfiguration());

        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
        private bool IsExcluded(SonarLintXmlReader sonarLintXml, string filePath)
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
    }

    private static ImmutableHashSet<string> UnchangedFilesGetOrAdd(Compilation compilation, ProjectConfigReader config) =>
        UnchangedFilesCache.GetValue(compilation, _ => CreateUnchangedFilesHashSet(config));

    private static ImmutableHashSet<string> CreateUnchangedFilesHashSet(ProjectConfigReader config) =>
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, config.AnalysisConfig?.UnchangedFiles() ?? []);

    private static bool IsRazorGeneratedTree(SyntaxTree tree) =>
        // Currently only .razor file hashes are stored in the cache.
        //
        // For a file like `Pages\Component.razor`, the compiler generated path has the following pattern:
        // Microsoft.NET.Sdk.Razor.SourceGenerators\Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator\Pages_Component_razor.g.cs
        // In order to avoid rebuilding the original file paths we have to read them from the #pragma checksum and #line directives.
        // For markup-only components, the checksum is the only reference to the component in the *_razor.g.cs tree.
        //
        // This should be updated for .cshtml files as well once https://github.com/SonarSource/sonar-dotnet/issues/8032 is done.
        tree.EndsWith(RazorGeneratedFileSuffix) || tree.EndsWith(RazorDeclGeneratedFileSuffix);

    private static bool IsRazorTreeUnchanged(SyntaxTree tree, ImmutableHashSet<string> unchangedFiles)
    {
        // Stop at the first changed file. The tree of a changed component references it in one of its first directives, only trees with all files unchanged are visited entirely.
        var hasMappedFile = false;
        foreach (var file in tree.GetRoot().DescendantNodes(static x => x.ContainsDirectives, descendIntoTrivia: true).Select(SyntaxTreeExtensions.FileFromDirective).WhereNotNull())
        {
            if (!unchangedFiles.Contains(file))
            {
                return false;
            }
            hasMappedFile = true;
        }
        return hasMappedFile || unchangedFiles.Contains(tree.FilePath);
    }
}
