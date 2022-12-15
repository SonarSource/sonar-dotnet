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

namespace SonarAnalyzer;

internal static class DiagnosticAnalyzerContextHelper   // FIXME: Rename and move
{
    private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<SyntaxTree, bool>> GeneratedCodeCache = new();

    public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(this SonarAnalysisContext context, // FIXME: Move them
                                                                                 GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                                 Action<SyntaxNodeAnalysisContext> action,
                                                                                 params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct =>
        context.RegisterSyntaxNodeAction(c =>
            {
                if (c.ShouldAnalyze(generatedCodeRecognizer))   // FIXME: Unify
                {
                    action(c.Context);
                }
            }, syntaxKinds);

    public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(this ParameterLoadingAnalysisContext context,
                                                                                 GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                                 Action<SyntaxNodeAnalysisContext> action,
                                                                                 params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct =>
        context.Context.RegisterSyntaxNodeActionInNonGenerated(generatedCodeRecognizer, action, syntaxKinds);

    public static void RegisterSyntaxNodeActionInNonGenerated<TLanguageKindEnum>(this SonarCompilationStartAnalysisContext context,
                                                                                 GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                                 Action<SyntaxNodeAnalysisContext> action,
                                                                                 params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct =>
        context.AnalysisContext.RegisterSyntaxNodeActionInNonGenerated(generatedCodeRecognizer, action, syntaxKinds);

    public static void RegisterSyntaxTreeActionInNonGenerated(this SonarAnalysisContext context, GeneratedCodeRecognizer generatedCodeRecognizer, Action<SonarSyntaxTreeAnalysisContext> action) =>
        context.RegisterSyntaxTreeAction(c =>
            {
                if (c.ShouldAnalyze(generatedCodeRecognizer))   // FIXME: Unify
                {
                    action(c);
                }
            });

    public static void RegisterSyntaxTreeActionInNonGenerated(this ParameterLoadingAnalysisContext context,
                                                              GeneratedCodeRecognizer generatedCodeRecognizer,
                                                              Action<SonarSyntaxTreeAnalysisContext> action)
    {
        // This is tricky. SyntaxTree actions do not have compilation. So we register them in CompilationStart.
        // ParametrizedAnalyzer postpones CompilationStartActions to enforce that parameters are already set when the postponed action is executed.
        var wrappedAction = context.Context.WrapSyntaxTreeAction(c =>
        {
            if (c.ShouldAnalyze(generatedCodeRecognizer))
            {
                action(c);
            }
        });
        context.RegisterPostponedAction(startContext => wrappedAction(startContext.Context));
    }

    public static void RegisterCodeBlockStartActionInNonGenerated<TLanguageKindEnum>(this SonarAnalysisContext context,
                                                                                     GeneratedCodeRecognizer generatedCodeRecognizer,
                                                                                     Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct =>
        context.RegisterCodeBlockStartAction<TLanguageKindEnum>(c =>
            {
                if (c.ShouldAnalyze(generatedCodeRecognizer))   // FIXME: Unify
                {
                    action(c.Context);
                }
            });

    public static bool IsGenerated(this SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer, Compilation compilation)  // FIXME: Move
    {
        if (tree == null)
        {
            return false;
        }
        var cache = GeneratedCodeCache.GetOrCreateValue(compilation);
        return cache.GetOrAdd(tree, generatedCodeRecognizer.IsGenerated);
    }
}
