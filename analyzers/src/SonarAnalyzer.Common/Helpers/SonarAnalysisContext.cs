/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using static SonarAnalyzer.Helpers.DiagnosticDescriptorBuilder;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// SonarC# and SonarVB specific context for initializing an analyzer. This type acts as a wrapper around Roslyn
    /// <see cref="AnalysisContext"/> to allow for specialized control over the analyzer.
    /// Here is the list of fine-grained changes we are doing:
    /// - Avoid duplicated issues when the analyzer NuGet (SonarAnalyzer) and the VSIX (SonarLint) are installed simultaneously.
    /// - Allow a specific kind of rule-set for SonarLint (enable/disable a rule).
    /// - Prevent reporting an issue when it was suppressed on SonarQube.
    /// </summary>
    public class SonarAnalysisContext
    {
        private delegate bool TryGetValueDelegate<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value);

        private const string SonarProjectConfigFileName = "SonarProjectConfig.xml";
        private static readonly Regex WebConfigRegex = new Regex(@"[\\\/]web\.([^\\\/]+\.)?config$", RegexOptions.IgnoreCase);

        private static readonly SourceTextValueProvider<bool> ShouldAnalyzeGeneratedCS = CreateAnalyzeGeneratedProvider(LanguageNames.CSharp);
        private static readonly SourceTextValueProvider<bool> ShouldAnalyzeGeneratedVB = CreateAnalyzeGeneratedProvider(LanguageNames.VisualBasic);
        private static readonly Lazy<ProjectConfigReader> EmptyProjectConfig = new Lazy<ProjectConfigReader>(() => new ProjectConfigReader(null, null));
        private static readonly SourceTextValueProvider<ProjectConfigReader> ProjectConfigProvider =
            new SourceTextValueProvider<ProjectConfigReader>(x => new ProjectConfigReader(x, SonarProjectConfigFileName));

        private readonly AnalysisContext context;
        private readonly IEnumerable<DiagnosticDescriptor> supportedDiagnostics;

        /// <summary>
        /// This delegate is called on all specific contexts, after the registration to the <see cref="AnalysisContext"/>, to
        /// control whether or not the action should be executed.
        /// </summary>
        /// <remarks>
        /// Currently this delegate is set by SonarLint (4.0+) when the project has the NuGet package installed to avoid
        /// duplicated analysis and issues. When both the NuGet and the VSIX are available, NuGet will take precedence and VSIX
        /// will be inhibited.
        /// </remarks>
        public static Func<IEnumerable<DiagnosticDescriptor>, SyntaxTree, bool> ShouldExecuteRegisteredAction { get; set; }

        /// <summary>
        /// This delegates control whether or not a diagnostic should be reported to Roslyn.
        /// </summary>
        /// <remarks>
        /// Currently this delegate is set by SonarLint (older than v4.0) to provide a suppression mechanism (i.e. specific
        /// issues turned off on the bound SonarQube).
        /// </remarks>
        public static Func<SyntaxTree, Diagnostic, bool> ShouldDiagnosticBeReported { get; set; }

        /// <summary>
        /// This delegate is used to supersede the default reporting action.
        /// When this delegate is set, the delegate set for <see cref="ShouldDiagnosticBeReported"/> is ignored.
        /// </summary>
        /// <remarks>
        /// Currently this delegate is set by SonarLint (4.0+) to control how the diagnostic should be reported to Roslyn
        /// (including not being reported).
        /// </remarks>
        public static Action<IReportingContext> ReportDiagnostic { get; set; }

        internal SonarAnalysisContext(AnalysisContext context, IEnumerable<DiagnosticDescriptor> supportedDiagnostics)
        {
            this.supportedDiagnostics = supportedDiagnostics ?? throw new ArgumentNullException(nameof(supportedDiagnostics));
            this.context = context;
        }

        public bool ShouldAnalyzeGenerated(Compilation c, AnalyzerOptions options) =>
            ShouldAnalyzeGenerated(context, c, options);

        public bool IsScannerRun(AnalyzerOptions options) =>
            ProjectConfiguration(options).IsScannerRun;

        public static bool IsScannerRun(CompilationAnalysisContext context) =>
            ProjectConfiguration(context.TryGetValue, context.Options).IsScannerRun;

        public bool IsTestProject(Compilation c, AnalyzerOptions options) =>
            IsTestProject(context.TryGetValue, c, options);

        public static bool IsTestProject(CompilationAnalysisContext analysisContext) =>
            IsTestProject(analysisContext.TryGetValue, analysisContext.Compilation, analysisContext.Options);

        public static bool ShouldAnalyzeGenerated(AnalysisContext analysisContext, Compilation c, AnalyzerOptions options) =>
            ShouldAnalyzeGenerated(analysisContext.TryGetValue, c, options);

        public static bool ShouldAnalyzeGenerated(CompilationAnalysisContext analysisContext, Compilation c, AnalyzerOptions options) =>
            ShouldAnalyzeGenerated(analysisContext.TryGetValue, c, options);

        public static bool ShouldAnalyzeGenerated(CompilationStartAnalysisContext analysisContext, Compilation c, AnalyzerOptions options) =>
            ShouldAnalyzeGenerated(analysisContext.TryGetValue, c, options);

        public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
            RegisterContextAction(context.RegisterCompilationStartAction, action, c => c.GetFirstSyntaxTree(), c => c.Compilation, c => c.Options);

        public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds) =>
            RegisterContextAction(act => context.RegisterSymbolAction(act, symbolKinds), action, c => c.GetFirstSyntaxTree(), c => c.Compilation, c => c.Options);

        internal static bool IsRegisteredActionEnabled(IEnumerable<DiagnosticDescriptor> diagnostics, SyntaxTree tree) =>
            ShouldExecuteRegisteredAction == null || tree == null || ShouldExecuteRegisteredAction(diagnostics, tree);

        internal void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
            where TLanguageKindEnum : struct =>
            RegisterContextAction(context.RegisterCodeBlockStartAction, action, c => c.GetSyntaxTree(), c => c.SemanticModel.Compilation, c => c.Options);

        internal void RegisterCompilationAction(Action<CompilationAnalysisContext> action) =>
            RegisterContextAction(context.RegisterCompilationAction, action, c => c.GetFirstSyntaxTree(), c => c.Compilation, c => c.Options);

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
            where TLanguageKindEnum : struct =>
            RegisterSyntaxNodeAction(action, syntaxKinds.ToArray());

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds)
            where TLanguageKindEnum : struct =>
            RegisterContextAction(x => context.RegisterSyntaxNodeAction(x, syntaxKinds), action, c => c.GetSyntaxTree(), c => c.Compilation, c => c.Options);

        internal IEnumerable<string> GetWebConfig(CompilationAnalysisContext c)
        {
            return ProjectConfiguration(c.Options).FilesToAnalyze.FindFiles(WebConfigRegex).Where(ShouldProcess);

            static bool ShouldProcess(string path) => !Path.GetFileName(path).Equals("web.debug.config", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads configuration from SonarProjectConfig.xml file and caches the result for scope of this analysis.
        /// </summary>
        internal ProjectConfigReader ProjectConfiguration(AnalyzerOptions options) =>
            ProjectConfiguration(context.TryGetValue, options);

        internal static bool IsAnalysisScopeMatching(Compilation compilation, bool isTestProject, bool isScannerRun, IEnumerable<DiagnosticDescriptor> diagnostics)
        {
            if (compilation == null)
            {
                return true; // We don't know whether this is a Main or Test source so let's run the rule
            }
            // MMF-2297: Test Code as 1st Class Citizen is not ready on server side yet.
            // ScannerRun: Only utility rules and rules with TEST-ONLY scope are executed for test projects for now.
            // SonarLint & Standalone Nuget: Respect the scope as before.
            return isTestProject
                ? ContainsTag(TestSourceScopeTag) && !(isScannerRun && ContainsTag(MainSourceScopeTag) && !ContainsTag(UtilityTag))
                : ContainsTag(MainSourceScopeTag);

            bool ContainsTag(string tag) =>
                diagnostics.Any(d => d.CustomTags.Contains(tag));
        }

        private static bool IsTestProject(TryGetValueDelegate<ProjectConfigReader> tryGetValue, Compilation compilation, AnalyzerOptions options)
        {
            var projectType = ProjectConfiguration(tryGetValue, options).ProjectType;
            return projectType == ProjectType.Unknown
                ? compilation.IsTest()              // SonarLint, NuGet or Scanner <= 5.0
                : projectType == ProjectType.Test;  // Scanner >= 5.1 does authoritative decision that we follow
        }

        private static SourceTextValueProvider<bool> CreateAnalyzeGeneratedProvider(string language) =>
            new SourceTextValueProvider<bool>(x => PropertiesHelper.ReadAnalyzeGeneratedCodeProperty(ParseXmlSettings(x), language));

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

        private static ProjectConfigReader ProjectConfiguration(TryGetValueDelegate<ProjectConfigReader> tryGetValue, AnalyzerOptions options)
        {
            if (options.AdditionalFiles.FirstOrDefault(IsSonarProjectConfig) is { } sonarProjectConfigXml)
            {
                return sonarProjectConfigXml.GetText() is { } sourceText
                    // TryGetValue catches all exceptions from SourceTextValueProvider and returns false when thrown
                    && tryGetValue(sourceText, ProjectConfigProvider, out var cachedProjectConfigReader)
                    ? cachedProjectConfigReader
                    : throw new InvalidOperationException($"File {Path.GetFileName(sonarProjectConfigXml.Path)} has been added as an AdditionalFile but could not be read and parsed.");
            }
            else
            {
                return EmptyProjectConfig.Value;
            }
        }

        private static bool ShouldAnalyzeGenerated(TryGetValueDelegate<bool> tryGetValue, Compilation c, AnalyzerOptions options) =>
            options.AdditionalFiles.FirstOrDefault(f => ParameterLoader.IsSonarLintXml(f.Path)) is { } sonarLintXml
            && tryGetValue(sonarLintXml.GetText(), ShouldAnalyzeGeneratedProvider(c.Language), out var shouldAnalyzeGenerated)
            && shouldAnalyzeGenerated;

        private static SourceTextValueProvider<bool> ShouldAnalyzeGeneratedProvider(string language) =>
            language == LanguageNames.CSharp ? ShouldAnalyzeGeneratedCS : ShouldAnalyzeGeneratedVB;

        private void RegisterContextAction<TContext>(Action<Action<TContext>> registrationAction,
                                                     Action<TContext> registeredAction,
                                                     Func<TContext, SyntaxTree> getSyntaxTree,
                                                     Func<TContext, Compilation> getCompilation,
                                                     Func<TContext, AnalyzerOptions> getAnalyzerOptions) =>
            registrationAction(c =>
                {
                    // For each action registered on context we need to do some pre-processing before actually calling the rule.
                    // First, we need to ensure the rule does apply to the current scope (main vs test source).
                    // Second, we call an external delegate (set by SonarLint for VS) to ensure the rule should be run (usually
                    // the decision is made on based on whether the project contains the analyzer as NuGet).
                    var compilation = getCompilation(c);
                    var isTestProject = IsTestProject(compilation, getAnalyzerOptions(c));

                    if (IsAnalysisScopeMatching(compilation, isTestProject, IsScannerRun(getAnalyzerOptions(c)), supportedDiagnostics)
                        && IsRegisteredActionEnabled(supportedDiagnostics, getSyntaxTree(c)))
                    {
                        registeredAction(c);
                    }
                });

        private static bool IsSonarProjectConfig(AdditionalText additionalText) =>
            additionalText.Path != null
            && ParameterLoader.ConfigurationFilePathMatchesExpected(additionalText.Path, SonarProjectConfigFileName);
    }
}
