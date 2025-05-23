﻿/*
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

namespace SonarAnalyzer.AnalysisContext;

public sealed class SonarCodeBlockStartAnalysisContext<TSyntaxKind> : SonarAnalysisContextBase<CodeBlockStartAnalysisContext<TSyntaxKind>> where TSyntaxKind : struct
{
    public override Compilation Compilation => Context.SemanticModel.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;
    public SyntaxNode CodeBlock => Context.CodeBlock;
    public ISymbol OwningSymbol => Context.OwningSymbol;
    public SemanticModel SemanticModel => Context.SemanticModel;

    internal SonarCodeBlockStartAnalysisContext(SonarAnalysisContext analysisContext, CodeBlockStartAnalysisContext<TSyntaxKind> context) : base(analysisContext, context) { }

    public void RegisterNodeAction(Action<SonarSyntaxNodeReportingContext> action, params TSyntaxKind[] symbolKinds) =>
        Context.RegisterSyntaxNodeAction(x => action(new(AnalysisContext, x)), symbolKinds);

    public void RegisterCodeBlockEndAction(Action<SonarCodeBlockReportingContext> action) =>
        Context.RegisterCodeBlockEndAction(x => action(new(AnalysisContext, x)));
}
