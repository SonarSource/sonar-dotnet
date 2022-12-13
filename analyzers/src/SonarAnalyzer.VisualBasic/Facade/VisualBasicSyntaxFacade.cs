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

namespace SonarAnalyzer.Helpers.Facade;

internal sealed class VisualBasicSyntaxFacade : SyntaxFacade<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    public override bool IsNullLiteral(SyntaxNode node) => node.IsNothingLiteral();

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

    public override IEnumerable<SyntaxNode> ArgumentExpressions(SyntaxNode node) =>
        node switch
        {
            ObjectCreationExpressionSyntax creation => creation.ArgumentList?.Arguments.Select(x => x.GetExpression()) ?? Enumerable.Empty<SyntaxNode>(),
            null => Enumerable.Empty<SyntaxNode>(),
            _ => throw InvalidOperation(node, nameof(ArgumentExpressions))
        };

    public override ImmutableArray<SyntaxNode> AssignmentTargets(SyntaxNode assignment) =>
        ImmutableArray.Create<SyntaxNode>(Cast<AssignmentStatementSyntax>(assignment).Left);

    public override SyntaxNode AssignmentLeft(SyntaxNode assignment) =>
        Cast<AssignmentStatementSyntax>(assignment).Left;

    public override SyntaxNode AssignmentRight(SyntaxNode assignment) =>
        Cast<AssignmentStatementSyntax>(assignment).Right;

    public override SyntaxNode BinaryExpressionLeft(SyntaxNode binaryExpression) =>
        Cast<BinaryExpressionSyntax>(binaryExpression).Left;

    public override SyntaxNode BinaryExpressionRight(SyntaxNode binaryExpression) =>
        Cast<BinaryExpressionSyntax>(binaryExpression).Right;

    public override IEnumerable<SyntaxNode> EnumMembers(SyntaxNode @enum) =>
        @enum == null ? Enumerable.Empty<SyntaxNode>() : Cast<EnumStatementSyntax>(@enum).Parent.ChildNodes().OfType<EnumMemberDeclarationSyntax>();

    public override SyntaxToken? InvocationIdentifier(SyntaxNode invocation) =>
        invocation == null ? null : Cast<InvocationExpressionSyntax>(invocation).GetMethodCallIdentifier();

    public override ImmutableArray<SyntaxToken> LocalDeclarationIdentifiers(SyntaxNode node) =>
        Cast<LocalDeclarationStatementSyntax>(node).Declarators.SelectMany(d => d.Names.Select(n => n.Identifier)).ToImmutableArray();

    public override ImmutableArray<SyntaxToken> FieldDeclarationIdentifiers(SyntaxNode node) =>
        Cast<FieldDeclarationSyntax>(node).Declarators.SelectMany(d => d.Names.Select(n => n.Identifier)).ToImmutableArray();

    public override SyntaxNode NodeExpression(SyntaxNode node) =>
        node switch
        {
            ArgumentSyntax x => x.GetExpression(),
            InterpolationSyntax x => x.Expression,
            InvocationExpressionSyntax x => x.Expression,
            SyncLockStatementSyntax x => x.Expression,
            ReturnStatementSyntax x => x.Expression,
            null => null,
            _ => throw InvalidOperation(node, nameof(NodeExpression)),
        };

    public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
        node.GetIdentifier();

    public override SyntaxNode RemoveConditionalAccess(SyntaxNode node)
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

    public override bool TryGetGetInterpolatedTextValue(SyntaxNode node, SemanticModel semanticModel, out string interpolatedValue) =>
        Cast<InterpolatedStringExpressionSyntax>(node).TryGetGetInterpolatedTextValue(semanticModel, out interpolatedValue);

    public override bool IsStatic(SyntaxNode node) => Cast<MethodBlockSyntax>(node).IsShared();

    protected override SyntaxToken Token(SyntaxNode node) => node is LiteralExpressionSyntax literal ? literal.Token : default;
}
