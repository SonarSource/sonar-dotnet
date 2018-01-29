/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// SonarC# and SonarVB specific context for initializing an analyzer. This type acts as a wrapper around Roslyn
    /// <see cref="AnalysisContext"/> to allow for specialized control over the analyzer.
    /// Here is the list of fine-grained changes we are doing:
    /// - Avoid duplicated issues when the analyzer NuGet (SonarAnalyzer) and the VSIX (SonarLint) are installed simultaneously.
    /// - Allow a specific kind of ruleset for SonarLint (enable/disable a rule).
    /// - Prevent reporting an issue when it was suppressed on SonarQube.
    /// </summary>
    public class SonarAnalysisContext
    {
        private readonly AnalysisContext context;
        private readonly IEnumerable<DiagnosticDescriptor> supportedDiagnostics;

        /// <summary>
        /// This delegate is set by SonarLint (older than v4.0) when the projects have the NuGet package installed to avoid
        /// duplicated analysis and issues. When both the NuGet and the VSIX are available, NuGet will take precendence and
        /// VSIX will be inhibited.
        /// </summary>
        /// <remarks>
        /// This delegate should always be kept in sync with its usage in SonarLint for Visual Studio. See file:
        /// https://github.com/SonarSource/sonarlint-visualstudio/blob/12119be2157542259fe3be7ce99bb14123092a0f/src/Integration.Vsix/SonarAnalyzerManager.cs
        /// </remarks>
        [Obsolete("This delegate is now obsolete, SonarLint should be using 'ShouldExecuteRuleFunc' instead.")]
        public static Func<SyntaxTree, bool> ShouldAnalysisBeDisabled { get; set; }

        /// <summary>
        /// This delegate is set by SonarLint (older than v4.0) to provide a suppression mechanism (i.e. specific issues turned off
        /// on SonarQube).
        /// </summary>
        /// <remarks>
        /// This delegate should always be kept in sync with its usage in SonarLint for Visual Studio. See file:
        /// https://github.com/SonarSource/sonarlint-visualstudio/blob/34bbe9f9576337eeb578ebba78a61a1d9c6740ac/src/Integration.Vsix/Suppression/DelegateInjector.cs
        /// </remarks>
        [Obsolete("This delegate is now obsolete, SonarLint should be using 'ReportDiagnosticAction' instead.")]
        public static Func<SyntaxTree, Diagnostic, bool> ShouldDiagnosticBeReported { get; set; }

        /// <summary>
        /// Newer versions of SonarLint (4.0+) should use this delegate in order to control whether a rule action should be
        /// executed. This will allow to turn off a rule when the new ruleset disable it.
        /// </summary>
        public static Func<AnalysisRunContext, bool> ShouldExecuteRuleFunc { get; set; }

        /// <summary>
        /// Newer versions of SonarLint (4.0+) should use this delegate in order to override the reporting action. This allows
        /// for advanced checked and control on SonarLint side.
        /// </summary>
        /// <remarks>
        /// This action will override the behavior provided by <see cref="ShouldDiagnosticBeReported"/>.
        /// </remarks>
        public static Action<ReportingContext> ReportDiagnosticAction { get; set; }

        internal SonarAnalysisContext(AnalysisContext context, IEnumerable<DiagnosticDescriptor> supportedDiagnostics)
        {
            this.supportedDiagnostics = supportedDiagnostics ?? throw new ArgumentNullException(nameof(supportedDiagnostics));
            this.context = context;
        }

        internal void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) =>
            context.RegisterCodeBlockAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.GetSyntaxTree(), supportedDiagnostics))
                    {
                        action(c);
                    }
                });

        internal void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
             where TLanguageKindEnum : struct =>
            context.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                c =>
                {
                    if (!IsAnalysisDisabled(c.GetSyntaxTree(), supportedDiagnostics))
                    {
                        action(c);
                    }
                });

        internal void RegisterCompilationAction(Action<CompilationAnalysisContext> action) =>
            context.RegisterCompilationAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.GetSyntaxTree(), supportedDiagnostics))
                    {
                        action(c);

                    }
                });

        public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.GetSyntaxTree(), supportedDiagnostics))
                    {
                        action(c);
                    }
                });

        internal void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action) =>
            context.RegisterSemanticModelAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.GetSyntaxTree(), supportedDiagnostics))
                    {
                        action(c);
                    }
                });

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action,
            ImmutableArray<TLanguageKindEnum> syntaxKinds)
            where TLanguageKindEnum : struct =>
            RegisterSyntaxNodeAction(action, syntaxKinds.ToArray());

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action,
            params TLanguageKindEnum[] syntaxKinds)
            where TLanguageKindEnum : struct =>
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.GetSyntaxTree(), supportedDiagnostics))
                    {
                        action(c);
                    }
                }, syntaxKinds);

        internal void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action) =>
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.GetSyntaxTree(), supportedDiagnostics))
                    {
                        action(c);
                    }
                });

        internal void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds) =>
            RegisterSymbolAction(action, symbolKinds.ToArray());

        public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds) =>
            context.RegisterSymbolAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.GetSyntaxTree(), supportedDiagnostics))
                    {
                        action(c);
                    }
                }, symbolKinds);

        internal static bool IsAnalysisDisabled(SyntaxTree tree, IEnumerable<DiagnosticDescriptor> supportedDiagnostics = null)
        {
            // This is the new way SonarLint will handle whether or not the analysis should be performed...
            // (checking `supportedDiagnostics != null` is to force providing the old behavior for SonarCodeFixProvider)
            if (ShouldExecuteRuleFunc != null &&
                supportedDiagnostics != null)
            {
                Debug.Assert(ShouldAnalysisBeDisabled == null, "Not expecting SonarLint to set both the old and the new" +
                    "delegates.");

                // If any of the diagnostic is enabled then allow to run the rule action BUT filter out at the time when the
                // issue is being reported.
                return !ShouldExecuteRuleFunc(new AnalysisRunContext(tree, supportedDiagnostics));
            }

            // ... but for compatibility purposes we need to keep handling the old-fashioned way
            return tree != null &&
                ShouldAnalysisBeDisabled != null &&
                ShouldAnalysisBeDisabled(tree);
        }
    }
}
