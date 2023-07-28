/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using Roslyn.Utilities;
using static SonarAnalyzer.Helpers.DiagnosticDescriptorFactory;

namespace SonarAnalyzer.AnalysisContext;

public class SonarAnalysisContextBase
{
    protected static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<string, bool>> FileInclusionCache = new();
    protected static readonly ConditionalWeakTable<Compilation, ImmutableHashSet<string>> UnchangedFilesCache = new();
    protected static readonly SourceTextValueProvider<ProjectConfigReader> ProjectConfigProvider = new(x => new ProjectConfigReader(x));
    protected static readonly SourceTextValueProvider<SonarLintXmlReader> SonarLintXmlProvider = new(x => new SonarLintXmlReader(x));

    protected SonarAnalysisContextBase() { }
}

public abstract class SonarAnalysisContextBase<TContext> : SonarAnalysisContextBase
{
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
    public virtual bool ShouldAnalyzeTree(SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        SonarLintXml() is var sonarLintXml
        && (generatedCodeRecognizer is null || sonarLintXml.AnalyzeGeneratedCode(Compilation.Language) || !tree.IsConsideredGenerated(generatedCodeRecognizer, Compilation))
        && (tree is null || (!IsUnchanged(tree) && !IsExcluded(sonarLintXml, tree.FilePath)));

    /// <summary>
    /// Reads configuration from SonarProjectConfig.xml file and caches the result for scope of this analysis.
    /// </summary>
    public ProjectConfigReader ProjectConfiguration()
    {
        if (Options.SonarProjectConfig() is { } sonarProjectConfig)
        {
            return sonarProjectConfig.GetText() is { } sourceText
                // TryGetValue catches all exceptions from SourceTextValueProvider and returns false when exception is thrown
                && AnalysisContext.TryGetValue(sourceText, ProjectConfigProvider, out var cachedProjectConfigReader)
                ? cachedProjectConfigReader
                : throw new InvalidOperationException($"File '{Path.GetFileName(sonarProjectConfig.Path)}' has been added as an AdditionalFile but could not be read and parsed.");
        }
        else
        {
            return ProjectConfigReader.Empty;
        }
    }

    /// <summary>
    /// Reads the properties from the SonarLint.xml file and caches the result for the scope of this analysis.
    /// </summary>
    public SonarLintXmlReader SonarLintXml()
    {
        if (Options.SonarLintXml() is { } sonarLintXml)
        {
            return sonarLintXml.GetText() is { } sourceText
                && AnalysisContext.TryGetValue(sourceText, SonarLintXmlProvider, out var sonarLintXmlReader)
                ? sonarLintXmlReader
                : throw new InvalidOperationException($"File '{Path.GetFileName(sonarLintXml.Path)}' has been added as an AdditionalFile but could not be read and parsed.");
        }
        else
        {
            return Helpers.SonarLintXmlReader.Empty;
        }
    }

    public bool IsTestProject()
    {
        var projectType = ProjectConfiguration().ProjectType;
        return projectType == ProjectType.Unknown
            ? Compilation.IsTest()              // SonarLint, NuGet or Scanner <= 5.0
            : projectType == ProjectType.Test;  // Scanner >= 5.1 does authoritative decision that we follow
    }

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = true, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    public bool IsUnchanged(SyntaxTree tree)
    {
        // Hotpath: Use TryGetValue to prevent the allocation of the GetValue factory delegate in the common case
        var unchangedFiles = UnchangedFilesCache.TryGetValue(Compilation, out var unchangedFilesFromCache)
            ? unchangedFilesFromCache
            : UnchangedFilesCache.GetValue(Compilation, _ => CreateUnchangedFilesHashSet());
        return unchangedFiles.Contains(tree.FilePath);
    }

    public bool HasMatchingScope(ImmutableArray<DiagnosticDescriptor> descriptors)
    {
        // Performance: Don't use descriptors.Any(HasMatchingScope), the delegate creation allocates too much memory. https://github.com/SonarSource/sonar-dotnet/issues/7438
        foreach (var descriptor in descriptors)
        {
            if (HasMatchingScope(descriptor))
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
        // SonarLint & Standalone NuGet: Respect the scope as before.
        return IsTestProject()
            ? ContainsTag(TestSourceScopeTag) && !(ProjectConfiguration().IsScannerRun && ContainsTag(MainSourceScopeTag) && !ContainsTag(UtilityTag))
            : ContainsTag(MainSourceScopeTag);

        bool ContainsTag(string tag) =>
            descriptor.CustomTags.Contains(tag);
    }

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/7439", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    private bool IsExcluded(SonarLintXmlReader sonarLintXml, string filePath)
    {
        // If ProjectType is not 'Unknown' it means we are in S4NET context and all files are analyzed.
        // If ProjectType is 'Unknown' then we are in SonarLint or NuGet context and we need to check if the file has been excluded from analysis through SonarLint.xml.
        if (ProjectConfiguration().ProjectType == ProjectType.Unknown)
        {
            var fileInclusionCache = FileInclusionCache.GetOrCreateValue(Compilation);
            // Hotpath: Don't use GetOrAdd with the value factory parameter. It allocates a delegate which causes GC preasure.
            var isIncluded = fileInclusionCache.TryGetValue(filePath, out var result)
                ? result
                : fileInclusionCache.GetOrAdd(filePath, sonarLintXml.IsFileIncluded(filePath, IsTestProject()));
            return !isIncluded;
        }
        return false;
    }

    private ImmutableHashSet<string> CreateUnchangedFilesHashSet() =>
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, ProjectConfiguration().AnalysisConfig?.UnchangedFiles() ?? Array.Empty<string>());
}
