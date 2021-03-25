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

using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    public static class AnalysisContextExtensions
    {
        public static SyntaxTree GetSyntaxTree(this SyntaxNodeAnalysisContext context) =>
            context.Node.SyntaxTree;

        public static SyntaxTree GetSyntaxTree(this SyntaxTreeAnalysisContext context) =>
            context.Tree;

        public static SyntaxTree GetFirstSyntaxTree(this CompilationAnalysisContext context) =>
            context.Compilation.SyntaxTrees.FirstOrDefault();

#pragma warning disable RS1012 // Start action has no registered actions.
        public static SyntaxTree GetFirstSyntaxTree(this CompilationStartAnalysisContext context) =>
#pragma warning restore RS1012 // Start action has no registered actions.
            context.Compilation.SyntaxTrees.FirstOrDefault();

        public static SyntaxTree GetFirstSyntaxTree(this SymbolAnalysisContext context) =>
            context.Symbol.Locations.FirstOrDefault(l => l.SourceTree != null)?.SourceTree;

        public static SyntaxTree GetSyntaxTree(this CodeBlockAnalysisContext context) =>
            context.CodeBlock.SyntaxTree;

#pragma warning disable RS1012 // Start action has no registered actions.
        public static SyntaxTree GetSyntaxTree<TLanguageKindEnum>(this CodeBlockStartAnalysisContext<TLanguageKindEnum> context)
#pragma warning restore RS1012 // Start action has no registered actions.
            where TLanguageKindEnum : struct =>
            context.CodeBlock.SyntaxTree;

        public static SyntaxTree GetSyntaxTree(this SemanticModelAnalysisContext context) =>
            context.SemanticModel.SyntaxTree;

        /// <param name="verifyScopeContext">Provide value for this argument only if the class has more than one SupportedDiagnostics.</param>
        public static void ReportDiagnosticWhenActive(this SyntaxNodeAnalysisContext context, Diagnostic diagnostic, SonarAnalysisContext verifyScopeContext = null) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic), verifyScopeContext?.IsTestProject(context.Compilation, context.Options));

        // SyntaxTreeAnalysisContext doesn't have a Compilation => verifyScopeContext is never needed, because IsAnalysisScopeMatching returns always true.
        public static void ReportDiagnosticWhenActive(this SyntaxTreeAnalysisContext context, Diagnostic diagnostic) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic), null);

        public static void ReportDiagnosticWhenActive(this CompilationAnalysisContext context, Diagnostic diagnostic) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic), SonarAnalysisContext.IsTestProject(context));

        /// <param name="verifyScopeContext">Provide value for this argument only if the class has more than one SupportedDiagnostics.</param>
        public static void ReportDiagnosticWhenActive(this SymbolAnalysisContext context, Diagnostic diagnostic, SonarAnalysisContext verifyScopeContext = null) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic), verifyScopeContext?.IsTestProject(context.Compilation, context.Options));

        /// <param name="verifyScopeContext">Provide value for this argument only if the class has more than one SupportedDiagnostics.</param>
        public static void ReportDiagnosticWhenActive(this CodeBlockAnalysisContext context, Diagnostic diagnostic, SonarAnalysisContext verifyScopeContext = null) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic), verifyScopeContext?.IsTestProject(context.SemanticModel.Compilation, context.Options));

        private static void ReportDiagnostic(ReportingContext reportingContext, bool? isTestProject)
        {
            if (isTestProject.HasValue && !SonarAnalysisContext.IsAnalysisScopeMatching(reportingContext.Compilation, isTestProject.Value, new[] { reportingContext.Diagnostic.Descriptor }))
            {
                return;
            }

            // This is the current way SonarLint will handle how and what to report.
            if (SonarAnalysisContext.ReportDiagnostic != null)
            {
                Debug.Assert(SonarAnalysisContext.ShouldDiagnosticBeReported == null, "Not expecting SonarLint to set both the old and the new delegates.");
                SonarAnalysisContext.ReportDiagnostic(reportingContext);
                return;
            }

            // Standalone NuGet, Scanner run and SonarLint < 4.0 used with latest NuGet
            if (!VbcHelper.IsTriggeringVbcError(reportingContext.Diagnostic)
                && (SonarAnalysisContext.ShouldDiagnosticBeReported?.Invoke(reportingContext.SyntaxTree, reportingContext.Diagnostic) ?? true))
            {
                reportingContext.ReportDiagnostic(reportingContext.Diagnostic);
            }
        }
    }
}
