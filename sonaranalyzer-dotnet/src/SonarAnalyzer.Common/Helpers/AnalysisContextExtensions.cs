/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
        public static SyntaxTree GetSyntaxTree(this CompilationAnalysisContext context) =>
            context.Compilation.SyntaxTrees.FirstOrDefault();
#pragma warning disable RS1012 // Start action has no registered actions.
        public static SyntaxTree GetSyntaxTree(this CompilationStartAnalysisContext context) =>
#pragma warning restore RS1012 // Start action has no registered actions.
            context.Compilation.SyntaxTrees.FirstOrDefault();
        public static SyntaxTree GetSyntaxTree(this SymbolAnalysisContext context) =>
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


        public static void ReportDiagnosticWhenActive(this SyntaxNodeAnalysisContext context, Diagnostic diagnostic) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic));
        public static void ReportDiagnosticWhenActive(this SyntaxTreeAnalysisContext context, Diagnostic diagnostic) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic));
        public static void ReportDiagnosticWhenActive(this CompilationAnalysisContext context, Diagnostic diagnostic) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic));
        public static void ReportDiagnosticWhenActive(this SymbolAnalysisContext context, Diagnostic diagnostic) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic));
        public static void ReportDiagnosticWhenActive(this CodeBlockAnalysisContext context, Diagnostic diagnostic) =>
            ReportDiagnostic(new ReportingContext(context, diagnostic));

        private static void ReportDiagnostic(ReportingContext reportingContext)
        {
            // This is the new way SonarLint will handle how and what to report...
            if (SonarAnalysisContext.ReportDiagnostic != null)
            {
                Debug.Assert(SonarAnalysisContext.ShouldDiagnosticBeReported == null, "Not expecting SonarLint to set both the " +
                    "old and the new delegates.");
                SonarAnalysisContext.ReportDiagnostic(reportingContext);
                return;
            }

            // ... but for compatibility purposes we need to keep handling the old-fashioned way
            if (SonarAnalysisContext.AreAnalysisScopeMatching(reportingContext.Compilation, new[] { reportingContext.Diagnostic.Descriptor }) &&
                !VbcHelper.IsTriggeringVbcError(reportingContext.Diagnostic) &&
                (SonarAnalysisContext.ShouldDiagnosticBeReported?.Invoke(reportingContext.SyntaxTree, reportingContext.Diagnostic) ?? true))
            {
                reportingContext.ReportDiagnostic(reportingContext.Diagnostic);
            }
        }
    }
}
