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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SonarAnalyzer.Helpers
{
    internal static class DiagnosticAnalyzerContextHelper
    {
        private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<SyntaxTree, bool>> Cache = new ConditionalWeakTable<Compilation, ConcurrentDictionary<SyntaxTree, bool>>();

        public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(this SonarAnalysisContext context,
                                                                                     GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                                     Action<SyntaxNodeAnalysisContext> action,
                                                                                     params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct =>
            context.RegisterSyntaxNodeAction(c =>
                {
                    if (ShouldAnalyze(context, generatedCodeRecognizer, c.GetSyntaxTree(), c.Compilation, c.Options))
                    {
                        action(c);
                    }
                }, syntaxKinds);

        public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(this ParameterLoadingAnalysisContext context,
                                                                                     GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                                     Action<SyntaxNodeAnalysisContext> action,
                                                                                     params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct =>
            context.Context.RegisterSyntaxNodeAction(c =>
                {
                    if (ShouldAnalyze(context.Context, generatedCodeRecognizer, c.GetSyntaxTree(), c.Compilation, c.Options))
                    {
                        action(c);
                    }
                }, syntaxKinds.ToImmutableArray());

        public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(this CompilationStartAnalysisContext context,
                                                                                     GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                                     Action<SyntaxNodeAnalysisContext> action,
                                                                                     params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct =>
            context.RegisterSyntaxNodeAction(c =>
                {
                    if (ShouldAnalyze(context, generatedCodeRecognizer, c.GetSyntaxTree(), c.Compilation, c.Options))
                    {
                        action(c);
                    }
                }, syntaxKinds);

        public static void RegisterSyntaxTreeActionInNonGenerated(this SonarAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, Action<SyntaxTreeAnalysisContext> action) =>
            context.RegisterCompilationStartAction(csac =>
                csac.RegisterSyntaxTreeAction(c =>
                    {
                        if (ShouldAnalyze(context, generatedCodeRecognizer, c.GetSyntaxTree(), csac.Compilation, c.Options))
                        {
                            action(c);
                        }
                    }));

        public static void RegisterSyntaxTreeActionInNonGenerated(this ParameterLoadingAnalysisContext context,
                                                                  GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                  Action<SyntaxTreeAnalysisContext> action) =>
            context.RegisterCompilationStartAction(csac =>
                csac.RegisterSyntaxTreeAction(c =>
                    {
                        if (ShouldAnalyze(context.Context, generatedCodeRecognizer, c.GetSyntaxTree(), csac.Compilation, c.Options))
                        {
                            action(c);
                        }
                    }));

        public static void RegisterCodeBlockStartActionInNonGenerated<TLanguageKindEnum>(this SonarAnalysisContext context,
                                                                                         GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                                         Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct =>
            context.RegisterCodeBlockStartAction<TLanguageKindEnum>(c =>
                {
                    if (ShouldAnalyze(context, generatedCodeRecognizer, c.GetSyntaxTree(), c.SemanticModel.Compilation, c.Options))
                    {
                        action(c);
                    }
                });

        public static void ReportDiagnosticIfNonGenerated(this CompilationAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, Diagnostic diagnostic)
        {
            if (ShouldAnalyze(context, generatedCodeRecognizer, diagnostic.Location.SourceTree, context.Compilation, context.Options))
            {
                context.ReportIssue(diagnostic);
            }
        }

        public static void ReportDiagnosticIfNonGenerated(this SymbolAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, Diagnostic diagnostic)
        {
            if (ShouldAnalyze(generatedCodeRecognizer, diagnostic.Location.SourceTree, context.Compilation, context.Options))
            {
                context.ReportIssue(diagnostic);
            }
        }

        public static bool IsGenerated(this SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer, Compilation compilation)
        {
            if (tree == null)
            {
                return false;
            }
            var cache = Cache.GetOrCreateValue(compilation);    // This is locking if the compilation is not present in the Cache.
            return cache.GetOrAdd(tree, x => generatedCodeRecognizer.IsGenerated(x));
        }

        public static bool ShouldAnalyze(GeneratedCodeRecognizer generatedCodeRecognizer, SyntaxTree syntaxTree, Compilation compilation, AnalyzerOptions options) =>
            SonarAnalysisContext.ShouldAnalyze(syntaxTree, compilation, options)
            && (PropertiesHelper.ReadAnalyzeGeneratedCodeProperty(PropertiesHelper.GetSettings(options), compilation.Language) || !syntaxTree.IsGenerated(generatedCodeRecognizer, compilation));

        public static bool ShouldAnalyze(SonarAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, SyntaxTree syntaxTree, Compilation compilation, AnalyzerOptions options) =>
            SonarAnalysisContext.ShouldAnalyze(syntaxTree, compilation, options)
            && (context.ShouldAnalyzeGenerated(compilation, options) || !syntaxTree.IsGenerated(generatedCodeRecognizer, compilation));

        private static bool ShouldAnalyze(CompilationStartAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, SyntaxTree syntaxTree, Compilation compilation, AnalyzerOptions options) =>
            SonarAnalysisContext.ShouldAnalyze(syntaxTree, compilation, options)
            && (SonarAnalysisContext.ShouldAnalyzeGenerated(context, compilation, options) || !syntaxTree.IsGenerated(generatedCodeRecognizer, compilation));

        private static bool ShouldAnalyze(CompilationAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, SyntaxTree syntaxTree, Compilation compilation, AnalyzerOptions options) =>
            SonarAnalysisContext.ShouldAnalyze(syntaxTree, compilation, options)
            && (SonarAnalysisContext.ShouldAnalyzeGenerated(context, compilation, options) || !syntaxTree.IsGenerated(generatedCodeRecognizer, compilation));
    }
}
