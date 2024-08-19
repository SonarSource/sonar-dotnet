/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Helpers
{
    internal class ReportingContext : IReportingContext
    {
        private readonly Action<Diagnostic> roslynReportDiagnostic;

        public SyntaxTree SyntaxTree { get; }
        public Diagnostic Diagnostic { get; }
        public Compilation Compilation { get; }

        public ReportingContext(SonarSyntaxNodeReportingContext context, Diagnostic diagnostic)
            : this(diagnostic, context.Context.ReportDiagnostic, context.Compilation, context.Tree) { }

        public ReportingContext(SonarSyntaxTreeReportingContext context, Diagnostic diagnostic)
            : this(diagnostic, context.Context.ReportDiagnostic, context.Compilation, context.Tree) { }

        public ReportingContext(SonarCompilationReportingContext context, Diagnostic diagnostic)
            : this(diagnostic, context.Context.ReportDiagnostic, context.Compilation, diagnostic.Location?.SourceTree) { }

        public ReportingContext(SonarSymbolReportingContext context, Diagnostic diagnostic)
            : this(diagnostic, context.Context.ReportDiagnostic, context.Compilation, diagnostic.Location?.SourceTree) { }

        public ReportingContext(SonarCodeBlockReportingContext context, Diagnostic diagnostic)
            : this(diagnostic, context.Context.ReportDiagnostic, context.Compilation, context.Tree) { }

        public ReportingContext(SonarSemanticModelReportingContext context, Diagnostic diagnostic)
            : this(diagnostic, context.Context.ReportDiagnostic, context.Compilation, context.Tree) { }

        private ReportingContext(Diagnostic diagnostic,
                                 Action<Diagnostic> roslynReportDiagnostic,
                                 Compilation compilation,
                                 SyntaxTree syntaxTree)
        {
            Diagnostic = diagnostic;
            this.roslynReportDiagnostic = roslynReportDiagnostic;
            Compilation = compilation;
            SyntaxTree = syntaxTree;
        }

        public void ReportDiagnostic(Diagnostic diagnostic) =>
            roslynReportDiagnostic(diagnostic);
    }
}
