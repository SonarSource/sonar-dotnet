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
using System.Xml.Linq;
using static SonarAnalyzer.Helpers.DiagnosticDescriptorFactory;

namespace SonarAnalyzer.AnalysisContext;

public class SonarAnalysisContextBase
{
    protected static readonly ConditionalWeakTable<Compilation, ImmutableHashSet<string>> UnchangedFilesCache = new();
    protected static readonly SourceTextValueProvider<ProjectConfigReader> ProjectConfigProvider = new(x => new ProjectConfigReader(x));
    private static readonly Lazy<SourceTextValueProvider<bool>> ShouldAnalyzeGeneratedCS = new(() => CreateAnalyzeGeneratedProvider(LanguageNames.CSharp));
    private static readonly Lazy<SourceTextValueProvider<bool>> ShouldAnalyzeGeneratedVB = new(() => CreateAnalyzeGeneratedProvider(LanguageNames.VisualBasic));

    protected SonarAnalysisContextBase() { }

    protected static SourceTextValueProvider<bool> ShouldAnalyzeGeneratedProvider(string language) =>
        language == LanguageNames.CSharp ? ShouldAnalyzeGeneratedCS.Value : ShouldAnalyzeGeneratedVB.Value;

    private static SourceTextValueProvider<bool> CreateAnalyzeGeneratedProvider(string language) =>
        new(x => PropertiesHelper.ReadAnalyzeGeneratedCodeProperty(PropertiesHelper.ParseXmlSettings(x), language));
}

public abstract class SonarAnalysisContextBase<TContext> : SonarAnalysisContextBase
{
    private readonly IGlobPatternMatcher globPatternMatcher;

    private readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<string, bool>> fileCache = new();

    public abstract SyntaxTree Tree { get; }
    public abstract Compilation Compilation { get; }
    public abstract AnalyzerOptions Options { get; }
    public abstract CancellationToken Cancel { get; }

    public SonarAnalysisContext AnalysisContext { get; }
    public TContext Context { get; }

    protected SonarAnalysisContextBase(SonarAnalysisContext analysisContext, TContext context)
    {
        AnalysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
        Context = context;
        globPatternMatcher = new GlobPatternMatcher();
    }

    /// <param name="tree">Tree to decide on. Can be null for Symbol-based and Compilation-based scenarios. And we want to analyze those too.</param>
    /// <param name="generatedCodeRecognizer">When set, generated trees are analyzed only when language-specific 'analyzeGeneratedCode' configuration property is also set.</param>
    public bool ShouldAnalyzeTree(SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        (generatedCodeRecognizer is null || ShouldAnalyzeGenerated() || !tree.IsGenerated(generatedCodeRecognizer, Compilation))
        && (tree is null || !IsUnchanged(tree));

    /// <summary>
    /// Check if the current file path is included or excuded and caches the result.
    /// </summary>
    public bool ShouldAnalyzeFile(Compilation compilation, XElement[] sonarLintSettings, string filePath) =>
        fileCache.GetValue(compilation, x => new()) is { } cache
        && cache.GetOrAdd(filePath, _ => ShouldAnalyzeFile(sonarLintSettings, filePath));

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

    public bool IsTestProject()
    {
        var projectType = ProjectConfiguration().ProjectType;
        return projectType == ProjectType.Unknown
            ? Compilation.IsTest()              // SonarLint, NuGet or Scanner <= 5.0
            : projectType == ProjectType.Test;  // Scanner >= 5.1 does authoritative decision that we follow
    }

    public bool IsUnchanged(SyntaxTree tree) =>
        UnchangedFilesCache.GetValue(Compilation, _ => CreateUnchangedFilesHashSet()).Contains(tree.FilePath);

    public bool HasMatchingScope(IEnumerable<DiagnosticDescriptor> descriptors) =>
        descriptors.Any(HasMatchingScope);

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

    private ImmutableHashSet<string> CreateUnchangedFilesHashSet() =>
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, ProjectConfiguration().AnalysisConfig?.UnchangedFiles() ?? Array.Empty<string>());

    private bool ShouldAnalyzeGenerated() =>
        Options.SonarLintXml() is { } sonarLintXml
        && AnalysisContext.TryGetValue(sonarLintXml.GetText(), ShouldAnalyzeGeneratedProvider(Compilation.Language), out var shouldAnalyzeGenerated)
        && shouldAnalyzeGenerated;

    private bool ShouldAnalyzeFile(XElement[] sonarLintSettings, string filePath) =>
        IsIncluded(PropertiesHelper.ReadSourceFileInclusionsProperty(sonarLintSettings), filePath)
        && !IsExcluded(PropertiesHelper.ReadSourceFileExclusionsProperty(sonarLintSettings), filePath);

    private bool IsIncluded(string[] inclusions, string filePath) =>
        inclusions is { Length: 0 } || inclusions.Any(x => globPatternMatcher.IsMatch(x, filePath));

    private bool IsExcluded(string[] exclusions, string filePath) =>
        exclusions is { Length: > 0 } && exclusions.Any(x => globPatternMatcher.IsMatch(x, filePath));
}
