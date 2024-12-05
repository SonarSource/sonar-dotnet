/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.AnalysisContext;

public sealed class SonarSyntaxTreeReportingContext : SonarTreeReportingContextBase<SyntaxTreeAnalysisContext>
{
    public override SyntaxTree Tree => Context.Tree;
    public override Compilation Compilation { get; }    // SyntaxTreeAnalysisContext doesn't hold a Compilation reference, we need to provide it from CompilationStart context via constructor
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;

    internal SonarSyntaxTreeReportingContext(SonarAnalysisContext analysisContext, SyntaxTreeAnalysisContext context, Compilation compilation) : base(analysisContext, context) =>
        Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));

    public override ReportingContext CreateReportingContext(Diagnostic diagnostic) =>
        new(this, diagnostic);
}
