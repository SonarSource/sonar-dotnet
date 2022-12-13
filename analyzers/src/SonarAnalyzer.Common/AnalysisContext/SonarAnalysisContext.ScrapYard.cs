/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;
using static SonarAnalyzer.Helpers.DiagnosticDescriptorFactory;

namespace SonarAnalyzer;

/// <summary>
/// SonarC# and SonarVB specific context for initializing an analyzer. This type acts as a wrapper around Roslyn
/// <see cref="AnalysisContext"/> to allow for specialized control over the analyzer.
/// Here is the list of fine-grained changes we are doing:
/// - Avoid duplicated issues when the analyzer NuGet (SonarAnalyzer) and the VSIX (SonarLint) are installed simultaneously.
/// - Allow a specific kind of rule-set for SonarLint (enable/disable a rule).
/// - Prevent reporting an issue when it was suppressed on SonarQube.
/// </summary>
public partial class SonarAnalysisContext
{
    public delegate bool TryGetValueDelegate<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value);     // FIXME: Done

    private static readonly SourceTextValueProvider<bool> ShouldAnalyzeGeneratedCS = CreateAnalyzeGeneratedProvider(LanguageNames.CSharp);
    private static readonly SourceTextValueProvider<bool> ShouldAnalyzeGeneratedVB = CreateAnalyzeGeneratedProvider(LanguageNames.VisualBasic);
    private static readonly SourceTextValueProvider<ProjectConfigReader> ProjectConfigProvider = new(x => new ProjectConfigReader(x));  // FIXME: Done
    private static readonly ConditionalWeakTable<Compilation, ImmutableHashSet<string>> UnchangedFilesCache = new();

    public bool ShouldAnalyzeGenerated(Compilation c, AnalyzerOptions options) =>
        ShouldAnalyzeGenerated(context.TryGetValue, c, options);

    public static bool ShouldAnalyze(TryGetValueDelegate<bool> tryGetBool,
                                     TryGetValueDelegate<ProjectConfigReader> tryGetProjectConfigReader,
                                     GeneratedCodeRecognizer generatedCodeRecognizer,
                                     SyntaxTree tree,
                                     Compilation compilation,
                                     AnalyzerOptions options) =>
        !IsUnchanged(tryGetProjectConfigReader, tree, compilation, options)
        && (ShouldAnalyzeGenerated(tryGetBool, compilation, options) || !tree.IsGenerated(generatedCodeRecognizer, compilation));

    public bool IsScannerRun(AnalyzerOptions options) =>
        ProjectConfiguration(options).IsScannerRun;

    public static bool IsScannerRun(CompilationAnalysisContext context) =>
        ProjectConfiguration(context.TryGetValue, context.Options).IsScannerRun;

    public bool IsTestProject(Compilation c, AnalyzerOptions options) =>
        IsTestProject(context.TryGetValue, c, options);

    public static bool IsTestProject(CompilationAnalysisContext analysisContext) =>
        IsTestProject(analysisContext.TryGetValue, analysisContext.Compilation, analysisContext.Options);

    internal static bool IsRegisteredActionEnabled(IEnumerable<DiagnosticDescriptor> diagnostics, SyntaxTree tree) =>
        ShouldExecuteRegisteredAction == null || tree == null || ShouldExecuteRegisteredAction(diagnostics, tree);

    // FIXME: Use the other one
    public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
        context.RegisterCompilationStartAction(c => Execute<SonarCompilationStartAnalysisContext, CompilationStartAnalysisContext>(new(this, c), x => action(x.Context)));

    //public void RegisterCompilationStartAction(Action<SonarCompilationStartAnalysisContext> action) =>
    //    context.RegisterCompilationStartAction(c => Execute<SonarCompilationStartAnalysisContext, CompilationStartAnalysisContext>(new(this, c), action));

    // FIXME: Use the other one
    public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds) =>
        context.RegisterSymbolAction(c => Execute<SonarSymbolAnalysisContext, SymbolAnalysisContext>(new(this, c), x => action(x.Context)), symbolKinds);

    //public void RegisterSymbolAction(Action<SonarSymbolAnalysisContext> action, params SymbolKind[] symbolKinds) =>
    //    context.RegisterSymbolAction(c => Execute<SonarSymbolAnalysisContext, SymbolAnalysisContext>(new(this, c), action), symbolKinds);

    // FIXME: Use the other one
    internal void RegisterCodeBlockStartAction<TSyntaxKind>(Action<CodeBlockStartAnalysisContext<TSyntaxKind>> action) where TSyntaxKind : struct =>
        context.RegisterCodeBlockStartAction<TSyntaxKind>(c => Execute<SonarCodeBlockStartAnalysisContext<TSyntaxKind>, CodeBlockStartAnalysisContext<TSyntaxKind>>(new(this, c), x => action(x.Context)));

    //internal void RegisterCodeBlockStartAction<TSyntaxKind>(Action<SonarCodeBlockStartAnalysisContext<TSyntaxKind>> action) where TSyntaxKind : struct =>
    //    context.RegisterCodeBlockStartAction<TSyntaxKind>(c => Execute<SonarCodeBlockStartAnalysisContext<TSyntaxKind>, CodeBlockStartAnalysisContext<TSyntaxKind>>(new(this, c), action));

    // FIXME: Use the other one
    internal void RegisterSyntaxNodeAction<TSyntaxKind>(Action<SyntaxNodeAnalysisContext> action, params TSyntaxKind[] syntaxKinds) where TSyntaxKind : struct =>
        context.RegisterSyntaxNodeAction(c => Execute<SonarSyntaxNodeAnalysisContext, SyntaxNodeAnalysisContext>(new(this, c), x => action(x.Context)), syntaxKinds);

    //internal void RegisterSyntaxNodeAction<TSyntaxKind>(Action<SonarSyntaxNodeAnalysisContext> action, params TSyntaxKind[] syntaxKinds) where TSyntaxKind : struct =>
    //    context.RegisterSyntaxNodeAction(c => Execute<SonarSyntaxNodeAnalysisContext, SyntaxNodeAnalysisContext>(new(this, c), action), syntaxKinds);

    /// <summary>
    /// Reads configuration from SonarProjectConfig.xml file and caches the result for scope of this analysis.
    /// </summary>
    internal ProjectConfigReader ProjectConfiguration(AnalyzerOptions options) =>   // FIXME Done
        ProjectConfiguration(context.TryGetValue, options);

    internal static ProjectConfigReader ProjectConfiguration(TryGetValueDelegate<ProjectConfigReader> tryGetValue, AnalyzerOptions options) // FIXME: Done
    {
        if (options.SonarProjectConfig() is { } sonarProjectConfigXml)
        {
            return sonarProjectConfigXml.GetText() is { } sourceText
                // TryGetValue catches all exceptions from SourceTextValueProvider and returns false when thrown
                && tryGetValue(sourceText, ProjectConfigProvider, out var cachedProjectConfigReader)
                ? cachedProjectConfigReader
                : throw new InvalidOperationException($"File {Path.GetFileName(sonarProjectConfigXml.Path)} has been added as an AdditionalFile but could not be read and parsed.");
        }
        else
        {
            return ProjectConfigReader.Empty;
        }
    }

    internal static bool IsAnalysisScopeMatching(Compilation compilation, bool isTestProject, bool isScannerRun, IEnumerable<DiagnosticDescriptor> diagnostics)
    {
        // We don't know the project type without the compilation so let's run the rule
        return compilation == null || diagnostics.Any(IsMatching);

        bool IsMatching(DiagnosticDescriptor descriptor)
        {
            // MMF-2297: Test Code as 1st Class Citizen is not ready on server side yet.
            // ScannerRun: Only utility rules and rules with TEST-ONLY scope are executed for test projects for now.
            // SonarLint & Standalone Nuget: Respect the scope as before.
            return isTestProject
                ? ContainsTag(TestSourceScopeTag) && !(isScannerRun && ContainsTag(MainSourceScopeTag) && !ContainsTag(UtilityTag))
                : ContainsTag(MainSourceScopeTag);

            bool ContainsTag(string tag) =>
                descriptor.CustomTags.Contains(tag);
        }
    }

    private static bool IsUnchanged(TryGetValueDelegate<ProjectConfigReader> tryGetValue, SyntaxTree tree, Compilation compilation, AnalyzerOptions options) =>
        UnchangedFilesCache.GetValue(compilation, _ => CreateUnchangedFilesHashSet(tryGetValue, options)).Contains(tree.FilePath);

    private static ImmutableHashSet<string> CreateUnchangedFilesHashSet(TryGetValueDelegate<ProjectConfigReader> tryGetValue, AnalyzerOptions options) =>
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, ProjectConfiguration(tryGetValue, options).AnalysisConfig?.UnchangedFiles() ?? Array.Empty<string>());

    private static bool IsTestProject(TryGetValueDelegate<ProjectConfigReader> tryGetValue, Compilation compilation, AnalyzerOptions options)   // FIXME: Done
    {
        var projectType = ProjectConfiguration(tryGetValue, options).ProjectType;
        return projectType == ProjectType.Unknown
            ? compilation.IsTest()              // SonarLint, NuGet or Scanner <= 5.0
            : projectType == ProjectType.Test;  // Scanner >= 5.1 does authoritative decision that we follow
    }

    private static SourceTextValueProvider<bool> CreateAnalyzeGeneratedProvider(string language) =>
        new(x => PropertiesHelper.ReadAnalyzeGeneratedCodeProperty(ParseXmlSettings(x), language));

    private static IEnumerable<XElement> ParseXmlSettings(SourceText sourceText)
    {
        try
        {
            return XDocument.Parse(sourceText.ToString()).Descendants("Setting");
        }
        catch
        {
            // cannot log the exception, so ignore it
            return Enumerable.Empty<XElement>();
        }
    }

    private static bool ShouldAnalyzeGenerated(TryGetValueDelegate<bool> tryGetValue, Compilation c, AnalyzerOptions options) =>
        options.SonarLintXml() is { } sonarLintXml
        && tryGetValue(sonarLintXml.GetText(), ShouldAnalyzeGeneratedProvider(c.Language), out var shouldAnalyzeGenerated)
        && shouldAnalyzeGenerated;

    private static SourceTextValueProvider<bool> ShouldAnalyzeGeneratedProvider(string language) =>
        language == LanguageNames.CSharp ? ShouldAnalyzeGeneratedCS : ShouldAnalyzeGeneratedVB;
}
