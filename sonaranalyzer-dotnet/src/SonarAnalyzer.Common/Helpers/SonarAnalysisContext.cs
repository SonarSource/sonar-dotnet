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
    public class SonarAnalysisContext
    {
        private readonly AnalysisContext context;
        private readonly IEnumerable<DiagnosticDescriptor> supportedDiagnostics;

        /// <summary>
        /// This delegate is only set by the VSIX when the projects have the NuGet package installed to avoid repeated behaviors.
        /// </summary>
        /// <remarks>
        /// This delegate should always be kept in sync with its usage in SonarLint for Visual Studio. See file:
        /// https://github.com/SonarSource/sonarlint-visualstudio/blob/12119be2157542259fe3be7ce99bb14123092a0f/src/Integration.Vsix/SonarAnalyzerManager.cs
        /// </remarks>
        public static Func<SyntaxTree, DiagnosticDescriptor, bool> ShouldAnalysisBeDisabled { get; set; }

        // This delegate should always be kept in sync with its usage in SonarLint for Visual Studio. See file:
        // https://github.com/SonarSource/sonarlint-visualstudio/blob/34bbe9f9576337eeb578ebba78a61a1d9c6740ac/src/Integration.Vsix/Suppression/DelegateInjector.cs
        public static Func<SyntaxTree, Diagnostic, bool> ShouldDiagnosticBeReported { get; set; } = (s, d) => true;

        internal SonarAnalysisContext(AnalysisContext context, IEnumerable<DiagnosticDescriptor> supportedDiagnostics)
        {
            this.supportedDiagnostics = supportedDiagnostics ?? throw new ArgumentNullException(nameof(supportedDiagnostics));
            this.context = context;
        }

        internal void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) =>
            context.RegisterCodeBlockAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.CodeBlock.SyntaxTree, supportedDiagnostics))
                    {
                        action(c);
                    }
                });

        internal void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
             where TLanguageKindEnum : struct =>
            context.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                c =>
                {
                    if (!IsAnalysisDisabled(c.CodeBlock.SyntaxTree, supportedDiagnostics))
                    {
                        action(c);
                    }
                });

        internal void RegisterCompilationAction(Action<CompilationAnalysisContext> action) =>
            context.RegisterCompilationAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.Compilation.SyntaxTrees.FirstOrDefault(), supportedDiagnostics))
                    {
                        action(c);

                    }
                });

        public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.Compilation.SyntaxTrees.FirstOrDefault(), supportedDiagnostics))
                    {
                        action(c);
                    }
                });

        internal void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action) =>
            context.RegisterSemanticModelAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.SemanticModel.SyntaxTree, supportedDiagnostics))
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
                    if (!IsAnalysisDisabled(c.Node.SyntaxTree, supportedDiagnostics))
                    {
                        action(c);
                    }
                }, syntaxKinds);

        internal void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action) =>
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    if (!IsAnalysisDisabled(c.Tree, supportedDiagnostics))
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
                    if (!IsAnalysisDisabled(c.Symbol.Locations.FirstOrDefault(l => l.SourceTree != null)?.SourceTree,
                            supportedDiagnostics))
                    {
                        action(c);
                    }
                }, symbolKinds);

        internal static bool IsAnalysisDisabled(SyntaxTree tree, IEnumerable<DiagnosticDescriptor> supportedDiagnostics)
        {
            if (supportedDiagnostics == null)
            {
                // Legacy mode only used by the code fix provider to avoid double registration of the code fix
                return tree != null &&
                    ShouldAnalysisBeDisabled != null &&
                    ShouldAnalysisBeDisabled(tree, null);
            }

            // If any of the diagnostic is enabled then allow to run the rule action BUT filter out at the time when the
            // issue is being reported.
            return tree != null &&
                ShouldAnalysisBeDisabled != null &&
                supportedDiagnostics.Any(diagnosticDescriptor => ShouldAnalysisBeDisabled(tree, diagnosticDescriptor));
        }
    }
}
