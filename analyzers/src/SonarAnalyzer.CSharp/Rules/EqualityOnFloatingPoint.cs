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
public sealed class EqualityOnFloatingPoint : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1244";
    private const string MessageFormat = "Do not check floating point {0} with exact values, use {1} instead.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly Dictionary<string, string> SpecialMembers = new()
    {
        { nameof(double.NaN), nameof(double.IsNaN) },
        { nameof(double.PositiveInfinity), nameof(double.IsPositiveInfinity) },
        { nameof(double.NegativeInfinity), nameof(double.IsNegativeInfinity) },
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            CheckEquality,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression);

        context.RegisterNodeAction(
            CheckLogicalExpression,
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression);
    }

    private static void CheckLogicalExpression(SonarSyntaxNodeReportingContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        if (TryGetBinaryExpression(binaryExpression.Left) is { } left
            && TryGetBinaryExpression(binaryExpression.Right) is { } right
            && CSharpEquivalenceChecker.AreEquivalent(right.Right, left.Right)
            && CSharpEquivalenceChecker.AreEquivalent(right.Left, left.Left)
            && IsIndirectEquality(context.Model, binaryExpression, left, right) is var isEquality
            && IsIndirectInequality(context.Model, binaryExpression, left, right) is var isInequality
            && (isEquality || isInequality))
        {
            context.ReportIssue(Rule, binaryExpression, MessageEqualityPart(isEquality), "a range");
        }
    }

    private static string MessageEqualityPart(bool isEquality) =>
        isEquality ? "equality" : "inequality";

    private static void CheckEquality(SonarSyntaxNodeReportingContext context)
    {
        var equals = (BinaryExpressionSyntax)context.Node;
        if (context.Model.GetSymbolInfo(equals).Symbol is IMethodSymbol { ContainingType: { } container } method
            && IsFloatingPointType(container)
            && (method.IsOperatorEquals() || method.IsOperatorNotEquals())
            && !IsConstantZero(context.Model, equals.Left)
            && !IsConstantZero(context.Model, equals.Right))
        {
            var messageEqualityPart = MessageEqualityPart(equals.IsKind(SyntaxKind.EqualsExpression));
            var proposed = ProposedMessageForMemberAccess(context, equals.Right)
                ?? ProposedMessageForMemberAccess(context, equals.Left)
                ?? ProposedMessageForIdentifier(context, equals.Right)
                ?? ProposedMessageForIdentifier(context, equals.Left)
                ?? "a range";
            context.ReportIssue(Rule, equals.OperatorToken, messageEqualityPart, proposed);
        }
    }

    // Zero is exactly representable in IEEE 754 and is the default value of floating-point types,
    // so comparing against it is a legitimate way to check for the default value or to guard against divide-by-zero.
    // Integer literal types (int, long, uint, ulong) are included because the literal `0` in `f == 0` has type int,
    // `f == 0L` has type long, etc.
    private static bool IsConstantZero(SemanticModel model, ExpressionSyntax expression) =>
        model.GetConstantValue(expression).Value switch
        {
            0 => true,
            0d => true,
            0f => true,
            0L => true,
            0U => true,
            0UL => true,
            _ => false
        };

    private static string ProposedMessageForMemberAccess(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression) =>
        expression is MemberAccessExpressionSyntax memberAccess
        && SpecialMembers.TryGetValue(memberAccess.GetName(), out var proposedMethod)
        && context.Model.GetTypeInfo(memberAccess).ConvertedType is { } type
        && IsFloatingPointType(type)
            ? $"'{type.ToMinimalDisplayString(context.Model, memberAccess.SpanStart)}.{proposedMethod}()'"
            : null;

    private static string ProposedMessageForIdentifier(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression) =>
        expression is IdentifierNameSyntax identifier
        && SpecialMembers.TryGetValue(identifier.GetName(), out var proposedMethod)
        && context.Model.GetSymbolInfo(identifier).Symbol is { ContainingType: { } type }
        && IsFloatingPointType(type)
            ? $"'{proposedMethod}()'"
            : null;

    // Returns true for the floating point types that suffer from equivalence problems. All .NET floating point types have this problem except `decimal.`
    // - Reason for excluding `decimal`: the documentation for the `decimal.Equals()` method does not have a "Precision in Comparisons" section as the other .NET floating point types.
    // - Power-2-based types like `double` implement `IFloatingPointIeee754`, but power-10-based `decimal` implements `IFloatingPoint`.
    // - `IFloatingPointIeee754` defines `Epsilon` which indicates problems with equivalence checking.
    private static bool IsFloatingPointType(ITypeSymbol type) =>
        type.IsAny(KnownType.FloatingPointNumbers)
        || (type.Is(KnownType.System_Numerics_IEqualityOperators_TSelf_TOther_TResult) // The operator originates from a virtual static member
            && type is INamedTypeSymbol { TypeArguments: { } typeArguments }           // Arguments of TSelf, TOther, TResult
            && typeArguments.Any(IsFloatingPointType))
        || (type is ITypeParameterSymbol { ConstraintTypes: { } constraintTypes }      // constraints of TSelf or of TSelf, TOther, TResult from IEqualityOperators
            && constraintTypes.Any(x => x.DerivesOrImplements(KnownType.System_Numerics_IFloatingPointIeee754_TSelf)));

    private static BinaryExpressionSyntax TryGetBinaryExpression(ExpressionSyntax expression) =>
        expression.RemoveParentheses() as BinaryExpressionSyntax;

    private static bool IsIndirectInequality(SemanticModel model, BinaryExpressionSyntax binaryExpression, BinaryExpressionSyntax left, BinaryExpressionSyntax right) =>
        binaryExpression.IsKind(SyntaxKind.LogicalOrExpression)
        && IsOperatorPair(left, right, SyntaxKind.GreaterThanToken, SyntaxKind.LessThanToken)
        && HasFloatingType(model, right);

    private static bool IsIndirectEquality(SemanticModel model, BinaryExpressionSyntax binaryExpression, BinaryExpressionSyntax left, BinaryExpressionSyntax right) =>
        binaryExpression.IsKind(SyntaxKind.LogicalAndExpression)
        && IsOperatorPair(left, right, SyntaxKind.GreaterThanEqualsToken, SyntaxKind.LessThanEqualsToken)
        && HasFloatingType(model, right);

    private static bool HasFloatingType(SemanticModel model, BinaryExpressionSyntax binary) =>
        IsExpressionFloatingType(model, binary.Right) || IsExpressionFloatingType(model, binary.Left);

    private static bool IsExpressionFloatingType(SemanticModel model, ExpressionSyntax expression) =>
        IsFloatingPointType(model.GetTypeInfo(expression).Type);

    private static bool IsOperatorPair(BinaryExpressionSyntax left, BinaryExpressionSyntax right, SyntaxKind first, SyntaxKind second) =>
        (left.OperatorToken.IsKind(first) && right.OperatorToken.IsKind(second))
        || (left.OperatorToken.IsKind(second) && right.OperatorToken.IsKind(first));
}
