/*
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

namespace SonarAnalyzer.Helpers.Facade;

internal sealed class CSharpSyntaxFacade : SyntaxFacade<SyntaxKind>
{
    public override bool AreEquivalent(SyntaxNode firstNode, SyntaxNode secondNode) =>
        SyntaxFactory.AreEquivalent(firstNode, secondNode);

    public override IEnumerable<SyntaxNode> ArgumentExpressions(SyntaxNode node) =>
        ArgumentList(node)?.OfType<ArgumentSyntax>().Select(x => x.Expression) ?? Enumerable.Empty<SyntaxNode>();

    public override IReadOnlyList<SyntaxNode> ArgumentList(SyntaxNode node) =>
        node.ArgumentList()?.Arguments;

    public override int? ArgumentIndex(SyntaxNode argument) =>
        Cast<ArgumentSyntax>(argument).GetArgumentIndex();

    public override SyntaxToken? ArgumentNameColon(SyntaxNode argument) =>
        (argument as ArgumentSyntax)?.NameColon?.Name.Identifier;

    public override SyntaxNode AssignmentLeft(SyntaxNode assignment) =>
        Cast<AssignmentExpressionSyntax>(assignment).Left;

    public override SyntaxNode AssignmentRight(SyntaxNode assignment) =>
        Cast<AssignmentExpressionSyntax>(assignment).Right;

    public override ImmutableArray<SyntaxNode> AssignmentTargets(SyntaxNode assignment) =>
        Cast<AssignmentExpressionSyntax>(assignment).AssignmentTargets();

    public override SyntaxNode BinaryExpressionLeft(SyntaxNode binary) =>
        Cast<BinaryExpressionSyntax>(binary).Left;

    public override SyntaxNode BinaryExpressionRight(SyntaxNode binary) =>
        Cast<BinaryExpressionSyntax>(binary).Right;

    public override SyntaxNode CastType(SyntaxNode cast) =>
        Cast<CastExpressionSyntax>(cast).Type;

    public override SyntaxNode CastExpression(SyntaxNode cast) =>
        Cast<CastExpressionSyntax>(cast).Expression;

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

    public override IEnumerable<SyntaxNode> EnumMembers(SyntaxNode @enum) =>
        @enum == null ? Enumerable.Empty<SyntaxNode>() : Cast<EnumDeclarationSyntax>(@enum).Members;

    public override ImmutableArray<SyntaxToken> FieldDeclarationIdentifiers(SyntaxNode node) =>
        Cast<FieldDeclarationSyntax>(node).Declaration.Variables.Select(x => x.Identifier).ToImmutableArray();

    public override bool HasExactlyNArguments(SyntaxNode invocation, int count) =>
        Cast<InvocationExpressionSyntax>(invocation).HasExactlyNArguments(count);

    public override SyntaxToken? InvocationIdentifier(SyntaxNode invocation) =>
        invocation == null ? null : Cast<InvocationExpressionSyntax>(invocation).GetMethodCallIdentifier();

    public override bool IsAnyKind(SyntaxNode node, ISet<SyntaxKind> syntaxKinds) => node.IsAnyKind(syntaxKinds);

    public override bool IsAnyKind(SyntaxNode node, params SyntaxKind[] syntaxKinds) => node.IsAnyKind(syntaxKinds);

    public override bool IsAnyKind(SyntaxTrivia trivia, params SyntaxKind[] syntaxKinds) => trivia.IsAnyKind(syntaxKinds);

    public override bool IsInExpressionTree(SemanticModel model, SyntaxNode node) =>
        node.IsInExpressionTree(model);

    public override bool IsKind(SyntaxNode node, SyntaxKind kind) => node.IsKind(kind);

    public override bool IsKind(SyntaxToken token, SyntaxKind kind) => token.IsKind(kind);

    public override bool IsKind(SyntaxTrivia trivia, SyntaxKind kind) => trivia.IsKind(kind);

    public override bool IsKnownAttributeType(SemanticModel model, SyntaxNode attribute, KnownType knownType) =>
        AttributeSyntaxExtensions.IsKnownType(Cast<AttributeSyntax>(attribute), knownType, model);

    public override bool IsMemberAccessOnKnownType(SyntaxNode memberAccess, string name, KnownType knownType, SemanticModel semanticModel) =>
        Cast<MemberAccessExpressionSyntax>(memberAccess).IsMemberAccessOnKnownType(name, knownType, semanticModel);

    public override bool IsNullLiteral(SyntaxNode node) => node.IsNullLiteral();

    public override bool IsStatic(SyntaxNode node) =>
        Cast<BaseMethodDeclarationSyntax>(node).IsStatic();

    /// <inheritdoc cref="Microsoft.CodeAnalysis.CSharp.Extensions.ExpressionSyntaxExtensions.IsWrittenTo(ExpressionSyntax, SemanticModel, CancellationToken)"/>
    public override bool IsWrittenTo(SyntaxNode expression, SemanticModel semanticModel, CancellationToken cancellationToken) =>
        Cast<ExpressionSyntax>(expression).IsWrittenTo(semanticModel, cancellationToken);

    public override SyntaxKind Kind(SyntaxNode node) => node.Kind();

    public override string LiteralText(SyntaxNode literal) =>
        Cast<LiteralExpressionSyntax>(literal).Token.ValueText;

    public override ImmutableArray<SyntaxToken> LocalDeclarationIdentifiers(SyntaxNode node) =>
        Cast<LocalDeclarationStatementSyntax>(node).Declaration.Variables.Select(x => x.Identifier).ToImmutableArray();

    public override SyntaxKind[] ModifierKinds(SyntaxNode node) =>
        node is TypeDeclarationSyntax typeDeclaration
            ? typeDeclaration.Modifiers.Select(x => x.Kind()).ToArray()
            : Array.Empty<SyntaxKind>();

    public override SyntaxNode NodeExpression(SyntaxNode node) =>
        node switch
        {
            ArrowExpressionClauseSyntax x => x.Expression,
            ArgumentSyntax x => x.Expression,
            AttributeArgumentSyntax x => x.Expression,
            InterpolationSyntax x => x.Expression,
            InvocationExpressionSyntax x => x.Expression,
            LockStatementSyntax x => x.Expression,
            ReturnStatementSyntax x => x.Expression,
            MemberAccessExpressionSyntax x => x.Expression,
            null => null,
            _ => throw InvalidOperation(node, nameof(NodeExpression))
        };

    public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
        node.GetIdentifier();

    public override SyntaxToken? ObjectCreationTypeIdentifier(SyntaxNode objectCreation) =>
        objectCreation == null ? null : Cast<ObjectCreationExpressionSyntax>(objectCreation).GetObjectCreationTypeIdentifier();

    public override SyntaxNode RemoveConditionalAccess(SyntaxNode node) =>
        node is ExpressionSyntax expression
            ? expression.RemoveConditionalAccess()
            : node;

    public override SyntaxNode RemoveParentheses(SyntaxNode node) =>
        node.RemoveParentheses();

    public override string StringValue(SyntaxNode node, SemanticModel semanticModel) =>
        CSharpSyntaxHelper.StringValue(node, semanticModel);

    public override bool TryGetInterpolatedTextValue(SyntaxNode node, SemanticModel semanticModel, out string interpolatedValue) =>
        Cast<InterpolatedStringExpressionSyntax>(node).TryGetInterpolatedTextValue(semanticModel, out interpolatedValue);

    public override bool TryGetOperands(SyntaxNode invocation, out SyntaxNode left, out SyntaxNode right) =>
        Cast<InvocationExpressionSyntax>(invocation).TryGetOperands(out left, out right);
}
