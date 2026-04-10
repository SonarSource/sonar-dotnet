/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExpressionComplexity : ExpressionComplexityBase<SyntaxKind>
{
    protected override ILanguageFacade Language { get; } = CSharpFacade.Instance;

    protected override HashSet<SyntaxKind> TransparentKinds { get; } =
        [
            // Binary
            SyntaxKind.CoalesceExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftExpression,
            SyntaxKindEx.UnsignedRightShiftExpression,
            SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,

            // Unary
            SyntaxKind.ParenthesizedExpression,
            SyntaxKind.LogicalNotExpression,
            SyntaxKindEx.ParenthesizedPattern,
            SyntaxKindEx.NotPattern,
        ];

    protected override HashSet<SyntaxKind> ComplexityIncreasingKinds { get; } =
        [
            SyntaxKind.ConditionalExpression,
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKindEx.CoalesceAssignmentExpression,
            SyntaxKindEx.AndPattern,
            SyntaxKindEx.OrPattern
        ];

    protected override SyntaxNode[] ExpressionChildren(SyntaxNode node) =>
        node.IsAnyKind(TransparentKinds) || node.IsAnyKind(ComplexityIncreasingKinds)
            ? node switch
            {
                ConditionalExpressionSyntax conditional => [conditional.Condition, conditional.WhenTrue, conditional.WhenFalse],
                BinaryExpressionSyntax binary => [binary.Left, binary.Right],
                { RawKind: (int)SyntaxKindEx.AndPattern or (int)SyntaxKindEx.OrPattern } pattern when (BinaryPatternSyntaxWrapper)pattern is var patternWrapper =>
                    [patternWrapper.Left.Node, patternWrapper.Right.Node],
                AssignmentExpressionSyntax assigment => [assigment.Left, assigment.Right],
                ParenthesizedExpressionSyntax { Expression: { } expression } => [expression],
                PrefixUnaryExpressionSyntax { Operand: { } operand } => [operand],
                { RawKind: (int)SyntaxKindEx.ParenthesizedPattern } parenthesized when (ParenthesizedPatternSyntaxWrapper)parenthesized is var parenthesizedWrapped =>
                    [parenthesizedWrapped.Pattern.Node],
                { RawKind: (int)SyntaxKindEx.NotPattern } negated when (UnaryPatternSyntaxWrapper)negated is var negatedWrapper =>
                    [negatedWrapper.Pattern.Node],
                _ => [],
            }
            : [];
}
