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

namespace SonarAnalyzer.Helpers.Facade
{
    internal sealed class CSharpSyntaxFacade : SyntaxFacade<SyntaxKind>
    {
        public override SyntaxKind Kind(SyntaxNode node) => node.Kind();

        public override bool IsNullLiteral(SyntaxNode node) => node.IsNullLiteral();

        public override IEnumerable<SyntaxNode> EnumMembers(SyntaxNode @enum) =>
            @enum == null ? Enumerable.Empty<SyntaxNode>() : Cast<EnumDeclarationSyntax>(@enum).Members;

        public override SyntaxToken? InvocationIdentifier(SyntaxNode invocation) =>
            invocation == null ? null : Cast<InvocationExpressionSyntax>(invocation).GetMethodCallIdentifier();

        public override SyntaxNode NodeExpression(SyntaxNode node) =>
            node switch
            {
                InvocationExpressionSyntax invocation => invocation.Expression,
                LockStatementSyntax @lock => @lock.Expression,
                null => null,
                _ => throw Unexpected(node)
            };

        public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
            node switch
            {
                EnumDeclarationSyntax enumDeclaration => enumDeclaration.Identifier,
                EnumMemberDeclarationSyntax enumMember => enumMember.Identifier,
                IdentifierNameSyntax identifierName => identifierName.Identifier,
                ParameterSyntax parameter => parameter.Identifier,
                VariableDeclaratorSyntax variable => variable.Identifier,
                null => null,
                _ => throw Unexpected(node)
            };
    }
}
