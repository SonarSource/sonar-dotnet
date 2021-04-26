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
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.Helpers.Facade
{
    internal sealed class VisualBasicSyntaxFacade : SyntaxFacade<SyntaxKind>
    {
        public override bool IsNullLiteral(SyntaxNode node) => node.IsNothingLiteral();

        public override SyntaxKind Kind(SyntaxNode node) => node.Kind();

        public override bool IsKind(SyntaxNode node, SyntaxKind kind) => node.IsKind(kind);

        public override bool IsAnyKind(SyntaxNode node, ISet<SyntaxKind> syntaxKinds) => node.IsAnyKind(syntaxKinds);

        public override IEnumerable<SyntaxNode> EnumMembers(SyntaxNode @enum) =>
            @enum == null ? Enumerable.Empty<SyntaxNode>() : Cast<EnumStatementSyntax>(@enum).Parent.ChildNodes().OfType<EnumMemberDeclarationSyntax>();

        public override SyntaxToken? InvocationIdentifier(SyntaxNode invocation) =>
            invocation == null ? null : Cast<InvocationExpressionSyntax>(invocation).GetMethodCallIdentifier();

        public override SyntaxNode NodeExpression(SyntaxNode node) =>
            node switch
            {
                InvocationExpressionSyntax invocation => invocation.Expression,
                SyncLockStatementSyntax syncLock => syncLock.Expression,
                null => null,
                _ => throw Unexpected(node)
            };

        public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
            node switch
            {
                EnumStatementSyntax enumStatement => enumStatement.Identifier,
                EnumMemberDeclarationSyntax enumMember => enumMember.Identifier,
                SimpleNameSyntax simpleName => simpleName.Identifier,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier,
                ParameterSyntax parameter => parameter.Identifier.Identifier,
                ModifiedIdentifierSyntax variable => variable.Identifier,
                null => null,
                _ => throw Unexpected(node)
            };

        public override string NodeStringTextValue(SyntaxNode node) =>
            node switch
            {
                InterpolatedStringExpressionSyntax interpolatedStringExpression => interpolatedStringExpression.GetContentsText(),
                LiteralExpressionSyntax literalExpression => literalExpression.Token.ValueText,
                _ => string.Empty
            };
    }
}
