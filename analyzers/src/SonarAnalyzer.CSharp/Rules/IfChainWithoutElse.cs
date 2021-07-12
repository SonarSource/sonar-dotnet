/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class IfChainWithoutElse : IfChainWithoutElseBase<SyntaxKind, IfStatementSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind SyntaxKind => SyntaxKind.IfStatement;
        protected override string ElseClause => "else";

        protected override bool IsElseIfWithoutElse(IfStatementSyntax ifSyntax) =>
           ifSyntax.Parent.IsKind(SyntaxKind.ElseClause)
           && (ifSyntax.Else == null || IsEmptyBlock(ifSyntax.Else));

        protected override Location IssueLocation(SyntaxNodeAnalysisContext context, IfStatementSyntax ifSyntax)
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
