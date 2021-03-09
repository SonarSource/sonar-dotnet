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
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

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

        private static readonly SourceTextValueProvider<bool> ShouldAnalyzeGeneratedCS = CreatePropertyProvider(PropertiesHelper.AnalyzeGeneratedCodeCSharp);
        private static readonly SourceTextValueProvider<bool> ShouldAnalyzeGeneratedVB = CreatePropertyProvider(PropertiesHelper.AnalyzeGeneratedCodeVisualBasic);
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

        public static bool ShouldAnalyzeGenerated(AnalysisContext analysisContext, Compilation c, AnalyzerOptions options) =>
            ShouldAnalyzeGenerated(analysisContext.TryGetValue, c, options);

        public static bool ShouldAnalyzeGenerated(CompilationAnalysisContext analysisContext, Compilation c, AnalyzerOptions options) =>
            ShouldAnalyzeGenerated(analysisContext.TryGetValue, c, options);

        public static bool ShouldAnalyzeGenerated(CompilationStartAnalysisContext analysisContext, Compilation c, AnalyzerOptions options) =>
            ShouldAnalyzeGenerated(analysisContext.TryGetValue, c, options);

        public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
            RegisterContextAction(context.RegisterCompilationStartAction, action, c => c.GetFirstSyntaxTree(), c => c.Compilation);

        public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds) =>
            RegisterContextAction(act => context.RegisterSymbolAction(act, symbolKinds), action, c => c.GetFirstSyntaxTree(), c => c.Compilation);

        internal static bool IsRegisteredActionEnabled(IEnumerable<DiagnosticDescriptor> diagnostics, SyntaxTree tree) =>
            ShouldExecuteRegisteredAction == null || tree == null || ShouldExecuteRegisteredAction(diagnostics, tree);

        internal void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
            where TLanguageKindEnum : struct =>
            RegisterContextAction(context.RegisterCodeBlockStartAction, action, c => c.GetSyntaxTree(), c => c.SemanticModel.Compilation);

        internal void RegisterCompilationAction(Action<CompilationAnalysisContext> action) =>
            RegisterContextAction(context.RegisterCompilationAction, action, c => c.GetFirstSyntaxTree(), c => c.Compilation);

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
            where TLanguageKindEnum : struct =>
            RegisterSyntaxNodeAction(action, syntaxKinds.ToArray());

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds)
            where TLanguageKindEnum : struct =>
            RegisterContextAction(x => context.RegisterSyntaxNodeAction(x, syntaxKinds), action, c => c.GetSyntaxTree(), c => c.Compilation);

        /// <summary>
        /// Reads configuration from SonarProjectConfig.xml file and caches the result for scope of this analysis.
        /// </summary>
        internal ProjectConfigReader ProjectConfiguration(AnalyzerOptions options)
        {
            if (options.AdditionalFiles.FirstOrDefault(IsSonarProjectConfig) is { } sonarProjectConfigXml)
            {
                return sonarProjectConfigXml.GetText() is { } sourceText
                    // TryGetValue catches all exceptions from SourceTextValueProvider and returns false when thrown
                    && context.TryGetValue(sourceText, ProjectConfigProvider, out var cachedProjectConfigReader)
                    ? cachedProjectConfigReader
                    : throw new InvalidOperationException($"File {Path.GetFileName(sonarProjectConfigXml.Path)} has been added as an AdditionalFile but could not be read and parsed.");
            }
            else
            {
                return EmptyProjectConfig.Value;
            }
        }

        private static SourceTextValueProvider<bool> CreatePropertyProvider(string propertyName) =>
            new SourceTextValueProvider<bool>(x => PropertiesHelper.ReadBooleanProperty(ParseXmlSettings(x), propertyName));

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
            options.AdditionalFiles.FirstOrDefault(f => ParameterLoader.IsSonarLintXml(f.Path)) is { } sonarLintXml
            && tryGetValue(sonarLintXml.GetText(), ShouldAnalyzeGeneratedProvider(c.Language), out var shouldAnalyzeGenerated)
            && shouldAnalyzeGenerated;

        private static SourceTextValueProvider<bool> ShouldAnalyzeGeneratedProvider(string language) =>
            language == LanguageNames.CSharp ? ShouldAnalyzeGeneratedCS : ShouldAnalyzeGeneratedVB;

        private void RegisterContextAction<TContext>(Action<Action<TContext>> registrationAction,
                                                     Action<TContext> registeredAction,
                                                     Func<TContext, SyntaxTree> getSyntaxTree,
                                                     Func<TContext, Compilation> getCompilation) =>
            registrationAction(c =>
                {
                    // For each action registered on context we need to do some pre-processing before actually calling the rule.
                    // First, we need to ensure the rule does apply to the current scope (main vs test source).
                    // Second, we call an external delegate (set by SonarLint for VS) to ensure the rule should be run (usually
                    // the decision is made on based on whether the project contains the analyzer as NuGet).
                    if (getCompilation(c).IsAnalysisScopeMatching(supportedDiagnostics) && IsRegisteredActionEnabled(supportedDiagnostics, getSyntaxTree(c)))
                    {
                        registeredAction(c);
                    }
                });

        private static bool IsSonarProjectConfig(AdditionalText additionalText) =>
            additionalText.Path != null
            && ParameterLoader.ConfigurationFilePathMatchesExpected(additionalText.Path, SonarProjectConfigFileName);
    }
}
