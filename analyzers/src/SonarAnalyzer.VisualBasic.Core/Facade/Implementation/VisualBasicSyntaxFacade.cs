/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using ComparisonKindEnum = SonarAnalyzer.Core.Syntax.Utilities.ComparisonKind;

namespace SonarAnalyzer.VisualBasic.Core.Facade.Implementation;

internal sealed class VisualBasicSyntaxFacade : SyntaxFacade<SyntaxKind>
{
    public override bool AreEquivalent(SyntaxNode firstNode, SyntaxNode secondNode) =>
        SyntaxFactory.AreEquivalent(firstNode, secondNode);

    public override IEnumerable<SyntaxNode> ArgumentExpressions(SyntaxNode node) =>
        ArgumentList(node)?.OfType<ArgumentSyntax>().Select(x => x.GetExpression()).WhereNotNull() ?? Enumerable.Empty<SyntaxNode>();

    public override IReadOnlyList<SyntaxNode> ArgumentList(SyntaxNode node) =>
        node.ArgumentList()?.Arguments;

    public override int? ArgumentIndex(SyntaxNode argument) =>
        Cast<ArgumentSyntax>(argument).GetArgumentIndex();

    public override SyntaxToken? ArgumentNameColon(SyntaxNode argument) =>
        (argument as SimpleArgumentSyntax)?.NameColonEquals?.Name.Identifier;

    public override SyntaxNode AssignmentLeft(SyntaxNode assignment) =>
        Cast<AssignmentStatementSyntax>(assignment).Left;

    public override SyntaxNode AssignmentRight(SyntaxNode assignment) =>
        Cast<AssignmentStatementSyntax>(assignment).Right;

    public override ImmutableArray<SyntaxNode> AssignmentTargets(SyntaxNode assignment) =>
        ImmutableArray.Create<SyntaxNode>(Cast<AssignmentStatementSyntax>(assignment).Left);

    public override SyntaxNode BinaryExpressionLeft(SyntaxNode binary) =>
        Cast<BinaryExpressionSyntax>(binary).Left;

    public override SyntaxNode BinaryExpressionRight(SyntaxNode binary) =>
        Cast<BinaryExpressionSyntax>(binary).Right;

    public override SyntaxNode CastType(SyntaxNode cast) =>
        Cast<CastExpressionSyntax>(cast).Type;

    public override SyntaxNode CastExpression(SyntaxNode cast) =>
        Cast<CastExpressionSyntax>(cast).Expression;

    public override ComparisonKind ComparisonKind(SyntaxNode node) =>
        node is BinaryExpressionSyntax { OperatorToken: var token }
            ? token.ToComparisonKind()
            : ComparisonKindEnum.None;

    public override IEnumerable<SyntaxNode> EnumMembers(SyntaxNode @enum) =>
        @enum is null ? Enumerable.Empty<SyntaxNode>() : Cast<EnumStatementSyntax>(@enum).Parent.ChildNodes().OfType<EnumMemberDeclarationSyntax>();

    public override ImmutableArray<SyntaxToken> FieldDeclarationIdentifiers(SyntaxNode node) =>
        Cast<FieldDeclarationSyntax>(node).Declarators.SelectMany(x => x.Names.Select(n => n.Identifier)).ToImmutableArray();

    public override bool HasExactlyNArguments(SyntaxNode invocation, int count) =>
        Cast<InvocationExpressionSyntax>(invocation).HasExactlyNArguments(count);

    public override SyntaxToken? InvocationIdentifier(SyntaxNode invocation) =>
        invocation is null ? null : Cast<InvocationExpressionSyntax>(invocation).GetMethodCallIdentifier();

    public override bool IsAnyKind(SyntaxNode node, ISet<SyntaxKind> syntaxKinds) => node.IsAnyKind(syntaxKinds);

    [Obsolete("Either use '.Kind() is A or B' or the overload with the ISet instead.")]
    public override bool IsAnyKind(SyntaxNode node, params SyntaxKind[] syntaxKinds) => node.IsAnyKind(syntaxKinds);

    public override bool IsAnyKind(SyntaxTrivia trivia, ISet<SyntaxKind> syntaxKinds) => trivia.IsAnyKind(syntaxKinds);

    public override bool IsInExpressionTree(SemanticModel model, SyntaxNode node) =>
        node.IsInExpressionTree(model);

    public override bool IsKind(SyntaxNode node, SyntaxKind kind) => node.IsKind(kind);

    public override bool IsKind(SyntaxToken token, SyntaxKind kind) => token.IsKind(kind);

    public override bool IsKind(SyntaxTrivia trivia, SyntaxKind kind) => trivia.IsKind(kind);

    public override bool IsKnownAttributeType(SemanticModel model, SyntaxNode attribute, KnownType knownType) =>
        AttributeSyntaxExtensions.IsKnownType(Cast<AttributeSyntax>(attribute), knownType, model);

    public override bool IsMemberAccessOnKnownType(SyntaxNode memberAccess, string name, KnownType knownType, SemanticModel model) =>
        Cast<MemberAccessExpressionSyntax>(memberAccess).IsMemberAccessOnKnownType(name, knownType, model);

    public override bool IsNullLiteral(SyntaxNode node) => node.IsNothingLiteral();

    public override bool IsStatic(SyntaxNode node) =>
        Cast<MethodBlockSyntax>(node).IsShared();

    /// <inheritdoc cref="ExpressionSyntaxExtensions.IsWrittenTo(ExpressionSyntax, SemanticModel, CancellationToken)"/>
    public override bool IsWrittenTo(SyntaxNode expression, SemanticModel model, CancellationToken cancel) =>
        Cast<ExpressionSyntax>(expression).IsWrittenTo(model, cancel);

    public override SyntaxKind Kind(SyntaxNode node) => node.Kind();

    public override string LiteralText(SyntaxNode literal) =>
        Cast<LiteralExpressionSyntax>(literal).Token.ValueText;

    public override ImmutableArray<SyntaxToken> LocalDeclarationIdentifiers(SyntaxNode node) =>
        Cast<LocalDeclarationStatementSyntax>(node).Declarators.SelectMany(x => x.Names.Select(n => n.Identifier)).ToImmutableArray();

    public override SyntaxKind[] ModifierKinds(SyntaxNode node) =>
        node is StructureBlockSyntax structureBlock
            ? structureBlock.StructureStatement.Modifiers.Select(x => x.Kind()).ToArray()
            : Array.Empty<SyntaxKind>();

    public override SyntaxNode NodeExpression(SyntaxNode node) =>
        node switch
        {
            ArgumentSyntax x => x.GetExpression(),
            InterpolationSyntax x => x.Expression,
            InvocationExpressionSyntax x => x.Expression,
            SyncLockStatementSyntax x => x.Expression,
            ReturnStatementSyntax x => x.Expression,
            MemberAccessExpressionSyntax x => x.Expression,
            null => null,
            _ => throw InvalidOperation(node, nameof(NodeExpression)),
        };

    public override SyntaxToken? NodeIdentifier(SyntaxNode node) =>
        node.GetIdentifier();

    public override SyntaxToken? ObjectCreationTypeIdentifier(SyntaxNode objectCreation) =>
        objectCreation is null ? null : Cast<ObjectCreationExpressionSyntax>(objectCreation).GetObjectCreationTypeIdentifier();

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

    public override string StringValue(SyntaxNode node, SemanticModel model) =>
        node.StringValue(model);

    public override string InterpolatedTextValue(SyntaxNode node, SemanticModel model) =>
        Cast<InterpolatedStringExpressionSyntax>(node).InterpolatedTextValue(model);

    public override Pair<SyntaxNode, SyntaxNode> Operands(SyntaxNode invocation) =>
        Cast<InvocationExpressionSyntax>(invocation).Operands();

    public override SyntaxNode ParseExpression(string expression) =>
        SyntaxFactory.ParseExpression(expression);
}
