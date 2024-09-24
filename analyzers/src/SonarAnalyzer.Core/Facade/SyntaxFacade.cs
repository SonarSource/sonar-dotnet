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
    public abstract bool IsAnyKind(SyntaxNode node, params TSyntaxKind[] syntaxKinds);
    public abstract bool IsAnyKind(SyntaxTrivia trivia, params TSyntaxKind[] syntaxKinds);
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
    public abstract bool TryGetInterpolatedTextValue(SyntaxNode node, SemanticModel model, out string interpolatedValue);
    public abstract bool TryGetOperands(SyntaxNode invocation, out SyntaxNode left, out SyntaxNode right);

    protected static T Cast<T>(SyntaxNode node) where T : SyntaxNode =>
        node as T ?? throw new InvalidCastException($"A {node.GetType().Name} node can not be cast to a {typeof(T).Name} node.");

    protected static Exception InvalidOperation(SyntaxNode node, string method) =>
        new InvalidOperationException($"{method} can not be performed on a {node.GetType().Name} node.");
}
