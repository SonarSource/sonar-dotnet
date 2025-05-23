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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EmptyNestedBlock : EmptyNestedBlockBase<SyntaxKind>
    {
        private static readonly SyntaxKind[] AllowedContainerKinds =
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

        protected override IEnumerable<SyntaxNode> EmptyBlocks(SyntaxNode node) =>
            (node is SwitchStatementSyntax switchNode && IsEmpty(switchNode))
                || (node is BlockSyntax blockNode && IsNestedAndEmpty(blockNode))
                    ? new[] { node }
                    : Enumerable.Empty<SyntaxNode>();

        private static bool IsEmpty(SwitchStatementSyntax node) =>
            !node.Sections.Any();

        private static bool IsNestedAndEmpty(BlockSyntax node) =>
            IsNested(node) && node.IsEmpty();

        private static bool IsNested(BlockSyntax node) =>
            !node.Parent.IsAnyKind(AllowedContainerKinds);
    }
}
