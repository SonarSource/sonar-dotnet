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

namespace SonarAnalyzer.Core.Facade;

public abstract class SyntaxFacade<TSyntaxKind> where TSyntaxKind : struct
{
    public abstract bool AreEquivalent(SyntaxNode firstNode, SyntaxNode secondNode);
    public abstract IEnumerable<SyntaxNode> ArgumentExpressions(SyntaxNode node);
    public abstract int? ArgumentIndex(SyntaxNode argument);
    public abstract IReadOnlyList<SyntaxNode> ArgumentList(SyntaxNode node);
    public abstract SyntaxToken? ArgumentNameColon(SyntaxNode argument);
    public abstract SyntaxNode AssignmentLeft(SyntaxNode assignment);
    public abstract SyntaxNode AssignmentRight(SyntaxNode assignment);
    public abstract ImmutableArray<SyntaxNode> AssignmentTargets(SyntaxNode assignment);
    public abstract SyntaxNode BinaryExpressionLeft(SyntaxNode binary);
    public abstract SyntaxNode BinaryExpressionRight(SyntaxNode binary);
    public abstract SyntaxNode CastType(SyntaxNode cast);
    public abstract SyntaxNode CastExpression(SyntaxNode cast);
    public abstract ComparisonKind ComparisonKind(SyntaxNode node);
    public abstract IEnumerable<SyntaxNode> EnumMembers(SyntaxNode @enum);
    public abstract ImmutableArray<SyntaxToken> FieldDeclarationIdentifiers(SyntaxNode node);
    public abstract bool HasExactlyNArguments(SyntaxNode invocation, int count);
    public abstract SyntaxToken? InvocationIdentifier(SyntaxNode invocation);
    public abstract bool IsAnyKind(SyntaxNode node, ISet<TSyntaxKind> syntaxKinds);
    [Obsolete("Either use '.Kind() is A or B' or the overload with the ISet instead.")]
    public abstract bool IsAnyKind(SyntaxNode node, params TSyntaxKind[] syntaxKinds);
    public abstract bool IsAnyKind(SyntaxTrivia trivia, ISet<TSyntaxKind> syntaxKinds);
    public abstract bool IsInExpressionTree(SemanticModel model, SyntaxNode node);
    public abstract bool IsKind(SyntaxNode node, TSyntaxKind kind);
    public abstract bool IsKind(SyntaxToken token, TSyntaxKind kind);
    public abstract bool IsKind(SyntaxTrivia trivia, TSyntaxKind kind);
    public abstract bool IsKnownAttributeType(SemanticModel model, SyntaxNode attribute, KnownType knownType);
    public abstract bool IsMemberAccessOnKnownType(SyntaxNode memberAccess, string name, KnownType knownType, SemanticModel model);
    public abstract bool IsNullLiteral(SyntaxNode node);
    public abstract bool IsStatic(SyntaxNode node);
    public abstract bool IsWrittenTo(SyntaxNode expression, SemanticModel model, CancellationToken cancel);
    public abstract TSyntaxKind Kind(SyntaxNode node);
    public abstract string LiteralText(SyntaxNode literal);
    public abstract ImmutableArray<SyntaxToken> LocalDeclarationIdentifiers(SyntaxNode node);
    public abstract TSyntaxKind[] ModifierKinds(SyntaxNode node);
    public abstract SyntaxNode NodeExpression(SyntaxNode node);
    public abstract SyntaxToken? NodeIdentifier(SyntaxNode node);
    public abstract SyntaxToken? ObjectCreationTypeIdentifier(SyntaxNode objectCreation);
    public abstract SyntaxNode RemoveConditionalAccess(SyntaxNode node);
    public abstract SyntaxNode RemoveParentheses(SyntaxNode node);
    public abstract string StringValue(SyntaxNode node, SemanticModel model);
    public abstract string InterpolatedTextValue(SyntaxNode node, SemanticModel model);
    public abstract Pair<SyntaxNode, SyntaxNode> Operands(SyntaxNode invocation);
    public abstract SyntaxNode ParseExpression(string expression);

    protected static T Cast<T>(SyntaxNode node) where T : SyntaxNode =>
        node as T ?? throw new InvalidCastException($"A {node.GetType().Name} node can not be cast to a {typeof(T).Name} node.");

    protected static Exception InvalidOperation(SyntaxNode node, string method) =>
        new InvalidOperationException($"{method} can not be performed on a {node.GetType().Name} node.");
}
