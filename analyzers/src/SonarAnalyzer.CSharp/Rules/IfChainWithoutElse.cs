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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IfChainWithoutElse : IfChainWithoutElseBase<SyntaxKind, IfStatementSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind SyntaxKind => SyntaxKind.IfStatement;
        protected override string ElseClause => "else";

        protected override bool IsElseIfWithoutElse(IfStatementSyntax ifSyntax) =>
           ifSyntax.Parent.IsKind(SyntaxKind.ElseClause)
           && (ifSyntax.Else == null || IsEmptyBlock(ifSyntax.Else));

        protected override Location IssueLocation(SonarSyntaxNodeReportingContext context, IfStatementSyntax ifSyntax)
        {
            var parentElse = (ElseClauseSyntax)ifSyntax.Parent;
            var diff = ifSyntax.IfKeyword.Span.End - parentElse.ElseKeyword.SpanStart;
            return Location.Create(context.Node.SyntaxTree, new TextSpan(parentElse.ElseKeyword.SpanStart, diff));
        }

        private static bool IsEmptyBlock(ElseClauseSyntax elseClause) =>
            elseClause.Statement is BlockSyntax blockSyntax
            && !(blockSyntax.Statements.Count > 0 || blockSyntax.DescendantTrivia().Any(x => x.IsComment() || x.IsKind(SyntaxKind.DisabledTextTrivia)));
    }
}
