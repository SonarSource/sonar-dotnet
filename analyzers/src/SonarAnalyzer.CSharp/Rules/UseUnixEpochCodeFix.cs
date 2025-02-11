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

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class UseUnixEpochCodeFix : UseUnixEpochCodeFixBase<SyntaxKind>
    {
        protected override SyntaxNode ReplaceConstructorWithField(SyntaxNode root, SyntaxNode node, SonarCodeFixContext context)
        {
            ExpressionSyntax typeNode;
            if (node.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression))
            {
                var semanticModel = context.Document.GetSemanticModelAsync(context.Cancel).ConfigureAwait(false).GetAwaiter().GetResult();
                typeNode = SyntaxFactory.IdentifierName(semanticModel.GetTypeInfo(node).Type.Name);
            }
            else
            {
                typeNode = ((ObjectCreationExpressionSyntax)node).Type;
            }

            var leadingTrivia = node.GetLeadingTrivia();
            var trailingTrivia = node.GetTrailingTrivia();
            return root.ReplaceNode(node,
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeNode,
                    SyntaxFactory.IdentifierName("UnixEpoch")).WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia));
        }
    }
}
