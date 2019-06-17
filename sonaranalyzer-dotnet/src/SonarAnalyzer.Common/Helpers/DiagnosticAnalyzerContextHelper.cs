/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto:contact@sonarsource.com
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    internal static class DiagnosticAnalyzerContextHelper
    {
        #region Register*ActionInNonGenerated

        public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(
            this SonarAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Action<SyntaxNodeAnalysisContext> action,
            params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (ShouldAnalyze(context, generatedCodeRecognizer, c.GetSyntaxTree(), c.Compilation, c.Options))
                    {
                        action(c);
                    }
                },
                syntaxKinds);
        }

        public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(
            this ParameterLoadingAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Action<SyntaxNodeAnalysisContext> action,
            params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (ShouldAnalyze(context.GetInnerContext(), generatedCodeRecognizer, c.GetSyntaxTree(), c.Compilation, c.Options))
                    {
                        action(c);
                    }
                },
                syntaxKinds.ToImmutableArray());
        }

        public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(
            this CompilationStartAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Action<SyntaxNodeAnalysisContext> action,
            params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (ShouldAnalyze(context, generatedCodeRecognizer, c.GetSyntaxTree(), c.Compilation, c.Options))
                    {
                        action(c);
                    }
                },
                syntaxKinds);
        }

        public static void RegisterSyntaxTreeActionInNonGenerated(
            this SonarAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Action<SyntaxTreeAnalysisContext> action)
        {
            context.RegisterCompilationStartAction(
                csac =>
                {
                    csac.RegisterSyntaxTreeAction(
                        c =>
                        {
                            if (ShouldAnalyze(context, generatedCodeRecognizer, c.GetSyntaxTree(), csac.Compilation, c.Options))
                            {
                                action(c);
                            }
                        });
                });
        }

        public static void RegisterSyntaxTreeActionInNonGenerated(
            this ParameterLoadingAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Action<SyntaxTreeAnalysisContext> action)
        {
            context.RegisterCompilationStartAction(
                csac =>
                {
                    csac.RegisterSyntaxTreeAction(
                        c =>
                        {
                            if (ShouldAnalyze(context.GetInnerContext(), generatedCodeRecognizer, c.GetSyntaxTree(), csac.Compilation, c.Options))
                            {
                                action(c);
                            }
                        });
                });
        }

        public static void RegisterCodeBlockStartActionInNonGenerated<TLanguageKindEnum>(
            this SonarAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
        {
            context.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                c =>
                {
                    if (ShouldAnalyze(context, generatedCodeRecognizer, c.GetSyntaxTree(), c.SemanticModel.Compilation, c.Options))
                    {
                        action(c);
                    }
                });
        }

        #endregion Register*ActionInNonGenerated

        #region ReportDiagnosticIfNonGenerated

        public static void ReportDiagnosticIfNonGenerated(
            this CompilationAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Diagnostic diagnostic,
            Compilation compilation)
        {
            if (ShouldAnalyze(context, generatedCodeRecognizer, diagnostic.Location.SourceTree, context.Compilation, context.Options))
            {
                context.ReportDiagnosticWhenActive(diagnostic);
            }
        }

        public static void ReportDiagnosticIfNonGenerated(
            this SymbolAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Diagnostic diagnostic,
            Compilation compilation)
        {
            if (ShouldAnalyze(generatedCodeRecognizer, diagnostic.Location.SourceTree, compilation, context.Options))
            {
                context.ReportDiagnosticWhenActive(diagnostic);
            }
        }

        public static void ReportDiagnosticIfNonGenerated(
            this SymbolAnalysisContext context,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Diagnostic diagnostic)
        {
            context.ReportDiagnosticIfNonGenerated(generatedCodeRecognizer, diagnostic, context.Compilation);
        }

        #endregion ReportDiagnosticIfNonGenerated

        private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<SyntaxTree, bool>> Cache
            = new ConditionalWeakTable<Compilation, ConcurrentDictionary<SyntaxTree, bool>>();

        private static bool ShouldAnalyze(SonarAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, SyntaxTree syntaxTree, Compilation c, AnalyzerOptions options) =>
            context.ShouldAnalyzeGenerated(c, options) ||
            !syntaxTree.IsGenerated(generatedCodeRecognizer, c);

        private static bool ShouldAnalyze(CompilationStartAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, SyntaxTree syntaxTree, Compilation c, AnalyzerOptions options) =>
            SonarAnalysisContext.ShouldAnalyzeGenerated(context, c, options) ||
            !syntaxTree.IsGenerated(generatedCodeRecognizer, c);

        private static bool ShouldAnalyze(CompilationAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, SyntaxTree syntaxTree, Compilation c, AnalyzerOptions options) =>
            SonarAnalysisContext.ShouldAnalyzeGenerated(context, c, options) ||
            !syntaxTree.IsGenerated(generatedCodeRecognizer, c);

        internal static bool ShouldAnalyze(GeneratedCodeRecognizer generatedCodeRecognizer, SyntaxTree syntaxTree, Compilation c, AnalyzerOptions options) =>
            options.ShouldAnalyzeGeneratedCode(c.Language) ||
            !syntaxTree.IsGenerated(generatedCodeRecognizer, c);

        internal static bool IsGenerated(this SyntaxTree tree,
            GeneratedCodeRecognizer generatedCodeRecognizer,
            Compilation compilation)
        {
            if (tree == null)
            {
                return false;
            }

            //this is locking if the compilation is not present in the Cache.
            var cache = Cache.GetOrCreateValue(compilation);
            if (cache.TryGetValue(tree, out var result))
            {
                return result;
            }

            var generated = generatedCodeRecognizer.IsGenerated(tree);
            cache.TryAdd(tree, generated);
            return generated;
        }
    }
}
