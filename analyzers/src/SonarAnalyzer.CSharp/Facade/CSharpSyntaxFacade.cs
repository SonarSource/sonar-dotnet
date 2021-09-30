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
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Helpers.Facade
{
    internal sealed class CSharpSyntaxFacade : SyntaxFacade<SyntaxKind>
    {
        public override SyntaxKind Kind(SyntaxNode node) => node.Kind();

        public override bool IsKind(SyntaxNode node, SyntaxKind kind) => node.IsKind(kind);

        public override bool IsKind(SyntaxToken token, SyntaxKind kind) => token.IsKind(kind);

        public override bool IsAnyKind(SyntaxNode node, ISet<SyntaxKind> syntaxKinds) => node.IsAnyKind(syntaxKinds);

        public override bool IsAnyKind(SyntaxNode node, params SyntaxKind[] syntaxKinds) => node.IsAnyKind(syntaxKinds);

        public override bool IsNullLiteral(SyntaxNode node) => node.IsNullLiteral();

        public override IEnumerable<SyntaxNode> EnumMembers(SyntaxNode @enum) =>
            @enum == null ? Enumerable.Empty<SyntaxNode>() : Cast<EnumDeclarationSyntax>(@enum).Members;

        public override SyntaxToken? InvocationIdentifier(SyntaxNode invocation) =>
            invocation == null ? null : Cast<InvocationExpressionSyntax>(invocation).GetMethodCallIdentifier();

        public override SyntaxNode NodeExpression(SyntaxNode node) =>
            node switch
            {
                AttributeArgumentSyntax attributeArgument => attributeArgument.Expression,
                InvocationExpressionSyntax invocation => invocation.Expression,
                LockStatementSyntax @lock => @lock.Expression,
                null => null,
                _ => throw Unexpected(node)
            };

        public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
            node switch
            {
                AttributeArgumentSyntax attribute => attribute.NameColon?.Name.Identifier,
                BaseTypeDeclarationSyntax baseType => baseType.Identifier,
                DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier,
                EnumMemberDeclarationSyntax enumMember => enumMember.Identifier,
                SimpleNameSyntax simpleName => simpleName.Identifier,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier,
                MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier,
                ParameterSyntax parameter => parameter.Identifier,
                PropertyDeclarationSyntax property => property.Identifier,
                VariableDeclaratorSyntax variable => variable.Identifier,
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

        public override SyntaxNode RemoveParentheses(SyntaxNode expression)
        {
            var current = expression;
            while (current?.IsAnyKind(SyntaxKind.ParenthesizedExpression, SyntaxKindEx.ParenthesizedPattern) ?? false)
            {
                current = current.IsKind(SyntaxKindEx.ParenthesizedPattern)
                    ? ((ParenthesizedPatternSyntaxWrapper)current).Pattern
                    : ((ParenthesizedExpressionSyntax)current).Expression;
            }
            return current;
        }
    }
}
