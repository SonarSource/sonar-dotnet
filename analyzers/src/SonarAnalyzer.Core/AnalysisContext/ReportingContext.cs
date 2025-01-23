/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.AnalysisContext;

public class ReportingContext : IReportingContext
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

    internal ReportingContext(Diagnostic diagnostic,
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
