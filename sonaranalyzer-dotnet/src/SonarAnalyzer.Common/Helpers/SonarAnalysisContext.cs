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
    /// - Allow a specific kind of rule-set for SonarLint (enable/disable a rule).
    /// - Prevent reporting an issue when it was suppressed on SonarQube.
    /// </summary>
    public class SonarAnalysisContext
    {
        private readonly AnalysisContext context;
        private readonly IEnumerable<DiagnosticDescriptor> supportedDiagnostics;

        /// <summary>
        /// This delegate is used to decide whether or not the <see cref="SonarDiagnosticAnalyzer"/> should be registered against
        /// the <see cref="AnalysisContext"/>.
        /// </summary>
        /// <remarks>
        /// Currently this delegate is set by SonarLint (4.0+) Standalone and NewConnected to control the registration based on
        /// the activation status of the rule.
        /// </remarks>
        public static Func<IEnumerable<DiagnosticDescriptor>, bool> ShouldRegisterContextAction { get; set; }

        /// <summary>
        /// This delegate is called on all specific contexts, after the registration to the <see cref="AnalysisContext"/>, to
        /// control whether or not the action should be executed.
        /// </summary>
        /// <remarks>
        /// Currently this delegate is set by SonarLint (4.0+) when the project has the NuGet package installed to avoid
        /// duplicated analysis and issues. When both the NuGet and the VSIX are available, NuGet will take precedence and VSIX
        /// will be inhibited.
        /// </remarks>
        public static Func<SyntaxTree, bool> ShouldExecuteRegisteredAction { get; set; }

        /// <summary>
        /// This delegates control whether or not a diagnostic should be reported to Roslyn.
        /// </summary>
        /// <remarks>
        /// Currently this delegate is set by SonarLint (older than v4.0) to provide a suppression mechanism (i.e. specific
        /// issues turned off on the bound SonarQube).
        /// </remarks>
        [Obsolete("This delegate is now obsolete, SonarLint should be using 'ReportDiagnosticAction' instead.")]
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

        internal void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) =>
            RegisterContextAction(context.RegisterCodeBlockAction, action, c => c.GetSyntaxTree());

        internal void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
             where TLanguageKindEnum : struct =>
            RegisterContextAction(context.RegisterCodeBlockStartAction, action, c => c.GetSyntaxTree());

        internal void RegisterCompilationAction(Action<CompilationAnalysisContext> action) =>
            RegisterContextAction(context.RegisterCompilationAction, action, c => c.GetSyntaxTree());

        public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
            RegisterContextAction(context.RegisterCompilationStartAction, action, c => c.GetSyntaxTree());

        internal void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action) =>
            RegisterContextAction(context.RegisterSemanticModelAction, action, c => c.GetSyntaxTree());

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action,
            ImmutableArray<TLanguageKindEnum> syntaxKinds)
            where TLanguageKindEnum : struct =>
            RegisterSyntaxNodeAction(action, syntaxKinds.ToArray());

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action,
            params TLanguageKindEnum[] syntaxKinds)
            where TLanguageKindEnum : struct =>
            RegisterContextAction(act => context.RegisterSyntaxNodeAction(act, syntaxKinds), action, c => c.GetSyntaxTree());

        internal void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action) =>
            RegisterContextAction(context.RegisterSyntaxTreeAction, action, c => c.GetSyntaxTree());

        internal void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds) =>
            RegisterSymbolAction(action, symbolKinds.ToArray());

        public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds) =>
            RegisterContextAction(act => context.RegisterSymbolAction(act, symbolKinds), action, c => c.GetSyntaxTree());

        private void RegisterContextAction<TContext>(Action<Action<TContext>> registrationAction, Action<TContext> registeredAction,
            Func<TContext, SyntaxTree> getSyntaxTree)
        {
            /*
             * For performance reasons, we don't want to register callbacks for rules that should not be run. However, we don't
             * necessarily have enough information to make the decision up front, so the check is broken into two parts:
             *
             * 1.   Pre-registration: at this point we only know the rules that are being registered. If we can decide from the
             *      diagnostic descriptors that a rule should not be run then we won't register the callback with the analysis
             *      context.
             *
             * 2.   Post-registration: we are now being called back by the analysis context, so we now have access to the syntax
             *      tree (and project) that are being analyzed, and can use that information to decide whether to execute the
             *      logic of the rule.
             */

            if (ShouldRegisterContextAction?.Invoke(this.supportedDiagnostics) != false)
            {
                registrationAction(
                    c =>
                    {
                        if (IsRegisteredActionEnabled(getSyntaxTree(c)))
                        {
                            registeredAction(c);
                        }
                    });
            }
        }

        internal static bool IsRegisteredActionEnabled(SyntaxTree tree) =>
            ShouldExecuteRegisteredAction == null ||
            tree == null ||
            ShouldExecuteRegisteredAction(tree);
    }
}
