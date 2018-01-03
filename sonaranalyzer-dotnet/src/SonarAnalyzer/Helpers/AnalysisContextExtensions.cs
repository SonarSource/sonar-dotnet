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
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    public static class AnalysisContextExtensions
    {
        private static readonly Regex VbNetErrorPattern = new Regex(@"\s+error\s*:",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

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
            ReportWhenNotSuppressed(context.Symbol.Locations.FirstOrDefault(l => l.SourceTree != null)?.SourceTree, diagnostic,
                d => context.ReportDiagnostic(d));
        }

        public static void ReportDiagnosticWhenActive(this CodeBlockAnalysisContext context, Diagnostic diagnostic)
        {
            ReportWhenNotSuppressed(context.CodeBlock.SyntaxTree, diagnostic, d => context.ReportDiagnostic(d));
        }

        private static void ReportWhenNotSuppressed(SyntaxTree tree, Diagnostic diagnostic, Action<Diagnostic> report)
        {
            if (SonarAnalysisContext.ShouldDiagnosticBeReported(tree, diagnostic))
            {
                // VB.Net complier (VBC) post-process issues and will fail if the line contains the VbNetErrorPattern.
                // See https://github.com/dotnet/roslyn/issues/5724
                // As a workaround we will prevent reporting the issue if the issue is on a line with the error pattern and the
                // language VB.Net
                // TODO: Remove this workaround when issue is fixed on Microsoft side.
                var rootNode = diagnostic.Location.SourceTree?.GetRoot();

                if (rootNode != null &&
                    rootNode.Language == LanguageNames.VisualBasic)
                {
                    var diagnosticNode = rootNode.FindNode(diagnostic.Location.SourceSpan);
                    var lineContent = diagnosticNode?.ToString();

                    if (lineContent != null &&
                        VbNetErrorPattern.IsMatch(lineContent))
                    {
                        return;
                    }
                }

                report(diagnostic);
            }
        }
    }
}
