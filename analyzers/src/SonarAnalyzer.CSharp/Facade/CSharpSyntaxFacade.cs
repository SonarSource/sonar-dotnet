/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using System.Collections.Immutable;
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

        public override IEnumerable<SyntaxNode> ArgumentExpressions(SyntaxNode node) =>
            node switch
            {
                ObjectCreationExpressionSyntax creation => creation.ArgumentList?.Arguments.Select(x => x.Expression) ?? Enumerable.Empty<SyntaxNode>(),
                null => Enumerable.Empty<SyntaxNode>(),
                var _ when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(node)
                    => ((ImplicitObjectCreationExpressionSyntaxWrapper)node).ArgumentList?.Arguments.Select(x => x.Expression) ?? Enumerable.Empty<SyntaxNode>(),
                _ => throw InvalidOperation(node, nameof(ArgumentExpressions))
            };

        public override ImmutableArray<SyntaxNode> AssignmentTargets(SyntaxNode assignment) =>
            Cast<AssignmentExpressionSyntax>(assignment).AssignmentTargets();

        public override SyntaxNode AssignmentRight(SyntaxNode assignment) =>
            Cast<AssignmentExpressionSyntax>(assignment).Right;

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
                InterpolationSyntax interpolation => interpolation.Expression,
                null => null,
                _ => throw InvalidOperation(node, nameof(NodeExpression))
            };

        public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
            node.NodeIdentifier();

        public override string NodeStringTextValue(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node is InterpolatedStringExpressionSyntax interpolatedStringExpression)
            {
                interpolatedStringExpression.TryGetGetInterpolatedTextValue(semanticModel, out var interpolatedValue);
                return interpolatedValue ?? interpolatedStringExpression.GetContentsText();
            }
            else
            {
                return node is LiteralExpressionSyntax literalExpression ? literalExpression.Token.ValueText : string.Empty;
            }
        }

        public override SyntaxNode RemoveConditionalAccess(SyntaxNode node) =>
            node is ExpressionSyntax expression
                ? expression.RemoveConditionalAccess()
                : node;

        public override SyntaxNode RemoveParentheses(SyntaxNode node) =>
            node.RemoveParentheses();
    }
}
