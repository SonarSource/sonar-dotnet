/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

public sealed class SonarCodeBlockStartAnalysisContext<TSyntaxKind> : SonarAnalysisContextBase<CodeBlockStartAnalysisContext<TSyntaxKind>> where TSyntaxKind : struct
{
    public override Compilation Compilation => Context.SemanticModel.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;
    public SyntaxNode CodeBlock => Context.CodeBlock;
    public ISymbol OwningSymbol => Context.OwningSymbol;
    public SemanticModel Model => Context.SemanticModel;

    internal SonarCodeBlockStartAnalysisContext(SonarAnalysisContext analysisContext, CodeBlockStartAnalysisContext<TSyntaxKind> context) : base(analysisContext, context) { }

    public void RegisterNodeAction(Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] symbolKinds) =>
        Context.RegisterSyntaxNodeAction(x => action(new(AnalysisContext, x)), symbolKinds);

    public void RegisterCodeBlockEndAction(Action<SonarCodeBlockReportingContext> action) =>
        Context.RegisterCodeBlockEndAction(x => action(new(AnalysisContext, x)));
}
