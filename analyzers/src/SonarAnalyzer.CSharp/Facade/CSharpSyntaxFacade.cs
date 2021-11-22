﻿/*
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

namespace SonarAnalyzer.Helpers.Facade
{
    internal sealed class CSharpSyntaxFacade : SyntaxFacade<SyntaxKind>
    {
        public override SyntaxKind Kind(SyntaxNode node) => node.Kind();

        public override ComparisonKind ComparisonKind(SyntaxNode node) =>
           node.Kind() switch
           {
               SyntaxKind.EqualsExpression => Helpers.ComparisonKind.Equals,
               SyntaxKind.NotEqualsExpression => Helpers.ComparisonKind.NotEquals,
               SyntaxKind.LessThanExpression => Helpers.ComparisonKind.LessThan,
               SyntaxKind.LessThanOrEqualExpression => Helpers.ComparisonKind.LessThanOrEqual,
               SyntaxKind.GreaterThanExpression => Helpers.ComparisonKind.GreaterThan,
               SyntaxKind.GreaterThanOrEqualExpression => Helpers.ComparisonKind.GreaterThanOrEqual,
               _ => Helpers.ComparisonKind.None,
           };

        public override bool IsKind(SyntaxNode node, SyntaxKind kind) => node.IsKind(kind);

        public override bool IsKind(SyntaxToken token, SyntaxKind kind) => token.IsKind(kind);

        public override bool IsAnyKind(SyntaxNode node, ISet<SyntaxKind> syntaxKinds) => node.IsAnyKind(syntaxKinds);

        public override bool IsAnyKind(SyntaxNode node, params SyntaxKind[] syntaxKinds) => node.IsAnyKind(syntaxKinds);

        public override bool IsNullLiteral(SyntaxNode node) => node.IsNullLiteral();

        public override SyntaxNode BinaryExpressionLeft(SyntaxNode binaryExpression) =>
            Cast<BinaryExpressionSyntax>(binaryExpression).Left;

        public override SyntaxNode BinaryExpressionRight(SyntaxNode binaryExpression) =>
            Cast<BinaryExpressionSyntax>(binaryExpression).Right;

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
                _ => throw InvalidOperation(node, nameof(NodeExpression))
            };

        public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
            RemoveParentheses(node) switch
            {
                AttributeArgumentSyntax attribute => attribute.NameColon?.Name.Identifier,
                BaseTypeDeclarationSyntax baseType => baseType.Identifier,
                DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier,
                EnumMemberDeclarationSyntax enumMember => enumMember.Identifier,
                InvocationExpressionSyntax invocation => NodeIdentifier(invocation.Expression),
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier,
                MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier,
                ParameterSyntax parameter => parameter.Identifier,
                PropertyDeclarationSyntax property => property.Identifier,
                SimpleNameSyntax simpleName => simpleName.Identifier,
                VariableDeclaratorSyntax variable => variable.Identifier,
                null => null,
                _ => throw InvalidOperation(node, nameof(NodeIdentifier)),
            };

        public override string NodeStringTextValue(SyntaxNode node) =>
            node switch
            {
                InterpolatedStringExpressionSyntax interpolatedStringExpression => interpolatedStringExpression.GetContentsText(),
                LiteralExpressionSyntax literalExpression => literalExpression.Token.ValueText,
                _ => string.Empty
            };

        public override SyntaxNode RemoveConditionalAcesss(SyntaxNode node)
        {
            var whenNotNull = node.RemoveParentheses();
            while (whenNotNull is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                whenNotNull = conditionalAccess.WhenNotNull.RemoveParentheses();
            }
            return whenNotNull;
        }

        public override SyntaxNode RemoveParentheses(SyntaxNode node) =>
            node.RemoveParentheses();
    }
}
