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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    internal class ReportingContext : IReportingContext
    {
        private readonly Action<Diagnostic> contextSpecificReport;

        public ReportingContext(SyntaxNodeAnalysisContext context, Diagnostic diagnostic)
        {
            SyntaxTree = context.GetSyntaxTree();
            Compilation = context.Compilation;
            Diagnostic = diagnostic;
            this.contextSpecificReport = context.ReportDiagnostic;
        }

        public ReportingContext(SyntaxTreeAnalysisContext context, Diagnostic diagnostic)
        {
            SyntaxTree = context.GetSyntaxTree();
            Compilation = null;
            Diagnostic = diagnostic;
            this.contextSpecificReport = context.ReportDiagnostic;
        }

        public ReportingContext(CompilationAnalysisContext context, Diagnostic diagnostic)
        {
            SyntaxTree = context.GetSyntaxTree();
            Compilation = context.Compilation;
            Diagnostic = diagnostic;
            this.contextSpecificReport = context.ReportDiagnostic;
        }

        public ReportingContext(SymbolAnalysisContext context, Diagnostic diagnostic)
        {
            SyntaxTree = context.GetSyntaxTree();
            Compilation = context.Compilation;
            Diagnostic = diagnostic;
            this.contextSpecificReport = context.ReportDiagnostic;
        }

        public ReportingContext(CodeBlockAnalysisContext context, Diagnostic diagnostic)
        {
            SyntaxTree = context.GetSyntaxTree();
            Compilation = context.SemanticModel.Compilation;
            Diagnostic = diagnostic;
            this.contextSpecificReport = context.ReportDiagnostic;
        }

        public SyntaxTree SyntaxTree { get; }

        public Diagnostic Diagnostic { get; }

        public Compilation Compilation { get; }

        public void ReportDiagnostic(Diagnostic diagnostic) => this.contextSpecificReport(diagnostic);
    }
}
