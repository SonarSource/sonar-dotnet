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
    /// </summary>
    public class SonarAnalysisContext
    {
        private static Func<AnalysisRunContext, bool> canRunAnalysisFunc = context => true;

        private readonly AnalysisContext context;
        private readonly IEnumerable<DiagnosticDescriptor> supportedDiagnostics;

        public static Func<AnalysisRunContext, bool> ShouldAnalyze
        {
            get { return canRunAnalysisFunc; }
            set
            {
                if (value != null)
                {
                    canRunAnalysisFunc = value;
                }
            }
        }

        internal SonarAnalysisContext(AnalysisContext context, IEnumerable<DiagnosticDescriptor> supportedDiagnostics)
        {
            this.supportedDiagnostics = supportedDiagnostics ?? throw new ArgumentNullException(nameof(supportedDiagnostics));
            this.context = context;
        }

        internal void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) =>
            context.RegisterCodeBlockAction(
                c =>
                {
                    if (ShouldAnalyze(new AnalysisRunContext(c.GetSyntaxTree(), supportedDiagnostics)))
                    {
                        action(c);
                    }
                });

        internal void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
             where TLanguageKindEnum : struct =>
            context.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                c =>
                {
                    if (ShouldAnalyze(new AnalysisRunContext(c.GetSyntaxTree(), supportedDiagnostics)))
                    {
                        action(c);
                    }
                });

        internal void RegisterCompilationAction(Action<CompilationAnalysisContext> action) =>
            context.RegisterCompilationAction(
                c =>
                {
                    if (ShouldAnalyze(new AnalysisRunContext(c.GetSyntaxTree(), supportedDiagnostics)))
                    {
                        action(c);
                    }
                });

        public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action) =>
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (ShouldAnalyze(new AnalysisRunContext(c.GetSyntaxTree(), supportedDiagnostics)))
                    {
                        action(c);
                    }
                });

        internal void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action) =>
            context.RegisterSemanticModelAction(
                c =>
                {
                    if (ShouldAnalyze(new AnalysisRunContext(c.GetSyntaxTree(), supportedDiagnostics)))
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
                    if (ShouldAnalyze(new AnalysisRunContext(c.GetSyntaxTree(), supportedDiagnostics)))
                    {
                        action(c);
                    }
                }, syntaxKinds);

        internal void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action) =>
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    if (ShouldAnalyze(new AnalysisRunContext(c.GetSyntaxTree(), supportedDiagnostics)))
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
                    if (ShouldAnalyze(new AnalysisRunContext(c.GetSyntaxTree(), supportedDiagnostics)))
                    {
                        action(c);
                    }
                }, symbolKinds);
    }
}
