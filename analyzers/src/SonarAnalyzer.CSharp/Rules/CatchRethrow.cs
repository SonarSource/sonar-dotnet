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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CatchRethrow : CatchRethrowBase<SyntaxKind, CatchClauseSyntax>
{
    private static readonly BlockSyntax ThrowBlock = SyntaxFactory.Block(SyntaxFactory.ThrowStatement());

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool ContainsOnlyThrow(CatchClauseSyntax currentCatch) =>
        CSharpEquivalenceChecker.AreEquivalent(currentCatch.Block, ThrowBlock);

    protected override CatchClauseSyntax[] AllCatches(SyntaxNode node) =>
        ((TryStatementSyntax)node).Catches.ToArray();

    protected override SyntaxNode DeclarationType(CatchClauseSyntax catchClause) =>
        catchClause.Declaration?.Type;

    protected override bool HasFilter(CatchClauseSyntax catchClause) =>
        catchClause.Filter is not null;

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(RaiseOnInvalidCatch, SyntaxKind.TryStatement);
}
