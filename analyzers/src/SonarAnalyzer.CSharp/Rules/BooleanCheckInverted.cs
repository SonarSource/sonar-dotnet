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
public sealed class BooleanCheckInverted : BooleanCheckInvertedBase<BinaryExpressionSyntax>
{
    private static readonly ISet<SyntaxKind> UnsafeInversionOperators = new HashSet<SyntaxKind>
        {
            SyntaxKind.GreaterThanToken,
            SyntaxKind.GreaterThanEqualsToken,
            SyntaxKind.LessThanToken,
            SyntaxKind.LessThanEqualsToken,
        };

    private static readonly Dictionary<SyntaxKind, string> OppositeTokens = new()
        {
            { SyntaxKind.GreaterThanToken, "<=" },
            { SyntaxKind.GreaterThanEqualsToken, "<" },
            { SyntaxKind.LessThanToken, ">=" },
            { SyntaxKind.LessThanEqualsToken, ">" },
            { SyntaxKind.EqualsEqualsToken, "!=" },
            { SyntaxKind.ExclamationEqualsToken, "==" },
        };

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            AnalysisAction(Rule),
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression);

    protected override bool IsUnsafeInversionOperation(BinaryExpressionSyntax expression, SemanticModel model) =>
        expression.OperatorToken.IsAnyKind(UnsafeInversionOperators)
        && (IsNullable(expression.Left, model)
            || IsNullable(expression.Right, model)
            || IsConditionalAccessExpression(expression.Left)
            || IsConditionalAccessExpression(expression.Right)
            || IsFloatingPoint(expression.Left, model)
            || IsFloatingPoint(expression.Right, model));

    protected override SyntaxNode LogicalNotNode(BinaryExpressionSyntax expression) =>
        expression.GetSelfOrTopParenthesizedExpression().Parent is PrefixUnaryExpressionSyntax prefixUnaryExpression
        && prefixUnaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationToken)
            ? prefixUnaryExpression
            : null;

    protected override string SuggestedReplacement(BinaryExpressionSyntax expression) =>
        OppositeTokens[expression.OperatorToken.Kind()];

    private static bool IsConditionalAccessExpression(ExpressionSyntax expression) =>
        expression.RemoveParentheses().IsKind(SyntaxKind.ConditionalAccessExpression);
}
