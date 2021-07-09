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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class EmptyNestedBlock : EmptyNestedBlockBase<SyntaxKind>
    {
        private static readonly ISet<SyntaxKind> AllowedContainerKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.AnonymousMethodExpression
        };

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } = new[]
        {
                SyntaxKind.Block,
                SyntaxKind.SwitchStatement
        };

        protected override IEnumerable<SyntaxNode> EmptyBlocks(SyntaxNode node)
        {
            if ((node is SwitchStatementSyntax switchNode && IsEmpty(switchNode))
                || (node is BlockSyntax blockNode && IsNestedAndEmpty(blockNode)))
            {
                return new[] { node };
            }

            return Enumerable.Empty<SyntaxNode>();
        }

        private static bool IsEmpty(SwitchStatementSyntax node) =>
            !node.Sections.Any();

        private static bool IsEmpty(BlockSyntax node) =>
            !node.Statements.Any() && !ContainsComment(node);

        private static bool IsNestedAndEmpty(BlockSyntax node) =>
            IsNested(node) && IsEmpty(node);

        private static bool IsNested(BlockSyntax node) =>
            !AllowedContainerKinds.Contains(node.Parent.Kind());

        private static bool ContainsComment(BlockSyntax node) =>
            ContainsComment(node.OpenBraceToken.TrailingTrivia) || ContainsComment(node.CloseBraceToken.LeadingTrivia);

        private static bool ContainsComment(SyntaxTriviaList trivias) =>
            trivias.Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
    }
}
