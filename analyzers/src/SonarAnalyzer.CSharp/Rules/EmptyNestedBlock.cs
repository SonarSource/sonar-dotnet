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
