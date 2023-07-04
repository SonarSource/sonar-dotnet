﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class UseUnixEpochCodeFix : UseUnixEpochCodeFixBase<SyntaxKind>
    {
        protected override SyntaxNode ReplaceConstructorWithField(SyntaxNode root, SyntaxNode node, CodeFixContext context)
        {
            ExpressionSyntax typeNode;
            if (node.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression))
            {
                var semanticModel = context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
                typeNode = SyntaxFactory.IdentifierName(semanticModel.GetTypeInfo(node).Type.Name);
            }
            else
            {
                typeNode = ((ObjectCreationExpressionSyntax)node).Type;
            }

            return root.ReplaceNode(node,
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeNode,
                    SyntaxFactory.IdentifierName("UnixEpoch")));
        }
    }
}
