/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    public class SonarAnalysisContext
    {
        private readonly AnalysisContext context;
        public static Func<SyntaxTree, bool> ShouldAnalysisBeDisabled { get; set; }
        public static Func<SyntaxTree, Diagnostic, bool> ShouldDiagnosticBeReported { get; set; } = (s, d) => true;

        internal SonarAnalysisContext(AnalysisContext context)
        {
            this.context = context;
        }

        internal void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
        {
            context.RegisterCodeBlockAction(
                c =>
                {
                    if (IsAnalysisDisabled(c.CodeBlock.SyntaxTree))
                    {
                        return;
                    }

                    action(c);
                });
        }

        internal void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
             where TLanguageKindEnum : struct
        {
            context.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                c =>
                {
                    if (IsAnalysisDisabled(c.CodeBlock.SyntaxTree))
                    {
                        return;
                    }

                    action(c);
                });
        }

        internal void RegisterCompilationAction(Action<CompilationAnalysisContext> action)
        {
            context.RegisterCompilationAction(
                c =>
                {
                    if (IsAnalysisDisabled(c.Compilation.SyntaxTrees.FirstOrDefault()))
                    {
                        return;
                    }

                    action(c);
                });
        }

        public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsAnalysisDisabled(c.Compilation.SyntaxTrees.FirstOrDefault()))
                    {
                        return;
                    }

                    action(c);
                });
        }

        internal void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
        {
            context.RegisterSemanticModelAction(
                c =>
                {
                    if (IsAnalysisDisabled(c.SemanticModel.SyntaxTree))
                    {
                        return;
                    }

                    action(c);
                });
        }

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action,
            ImmutableArray<TLanguageKindEnum> syntaxKinds)
            where TLanguageKindEnum : struct
        {
            RegisterSyntaxNodeAction(action, syntaxKinds.ToArray());
        }

        internal void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action,
            params TLanguageKindEnum[] syntaxKinds)
            where TLanguageKindEnum : struct
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (IsAnalysisDisabled(c.Node.SyntaxTree))
                    {
                        return;
                    }

                    action(c);
                },
                syntaxKinds);
        }

        internal void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action)
        {
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    if (IsAnalysisDisabled(c.Tree))
                    {
                        return;
                    }

                    action(c);
                });
        }

        internal void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
        {
            RegisterSymbolAction(action, symbolKinds.ToArray());
        }

        public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    if (IsAnalysisDisabled(c.Symbol.Locations.FirstOrDefault(l => l.SourceTree != null)?.SourceTree))
                    {
                        return;
                    }

                    action(c);
                },
                symbolKinds);
        }

        internal static bool IsAnalysisDisabled(SyntaxTree tree)
        {
            if (ShouldAnalysisBeDisabled == null)
            {
                return false;
            }

            return tree != null && ShouldAnalysisBeDisabled(tree);
        }
    }
}
