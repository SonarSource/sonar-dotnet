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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    public static class AnalysisContextExtensions
    {
        public static void ReportDiagnosticWhenActive(this SyntaxNodeAnalysisContext context, Diagnostic diagnostic)
        {
            ReportWhenNotSuppressed(context.Node.SyntaxTree, diagnostic, d => context.ReportDiagnostic(d));
        }

        public static void ReportDiagnosticWhenActive(this SyntaxTreeAnalysisContext context, Diagnostic diagnostic)
        {
            ReportWhenNotSuppressed(context.Tree, diagnostic, d => context.ReportDiagnostic(d));
        }

        public static void ReportDiagnosticWhenActive(this CompilationAnalysisContext context, Diagnostic diagnostic)
        {
            ReportWhenNotSuppressed(context.Compilation.SyntaxTrees.FirstOrDefault(), diagnostic,
                d => context.ReportDiagnostic(d));
        }

        public static void ReportDiagnosticWhenActive(this SymbolAnalysisContext context, Diagnostic diagnostic)
        {
            ReportWhenNotSuppressed(context.Symbol.Locations.FirstOrDefault(l => l.SourceTree != null)?.SourceTree,
                diagnostic, d => context.ReportDiagnostic(d));
        }

        public static void ReportDiagnosticWhenActive(this CodeBlockAnalysisContext context, Diagnostic diagnostic)
        {
            ReportWhenNotSuppressed(context.CodeBlock.SyntaxTree, diagnostic, d => context.ReportDiagnostic(d));
        }

        private static void ReportWhenNotSuppressed(SyntaxTree tree, Diagnostic diagnostic, Action<Diagnostic> report)
        {
            if (SonarAnalysisContext.ShouldDiagnosticBeReported(tree, diagnostic))
            {
                report(diagnostic);
            }
        }
    }
}
