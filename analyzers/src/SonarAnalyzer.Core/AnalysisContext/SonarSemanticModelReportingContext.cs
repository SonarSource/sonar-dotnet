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

public sealed class SonarSemanticModelReportingContext : SonarTreeReportingContextBase<SemanticModelAnalysisContext>
{
    public override SyntaxTree Tree => SemanticModel.SyntaxTree;
    public override Compilation Compilation => Context.SemanticModel.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;
    public SemanticModel SemanticModel => Context.SemanticModel;

    internal SonarSemanticModelReportingContext(SonarAnalysisContext analysisContext, SemanticModelAnalysisContext context) : base(analysisContext, context) { }

    public override ReportingContext CreateReportingContext(Diagnostic diagnostic) =>
        new(this, diagnostic);
}
