/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp;

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
            && IsIndirectEquality(context.SemanticModel, binaryExpression, left, right) is var isEquality
            && IsIndirectInequality(context.SemanticModel, binaryExpression, left, right) is var isInequality
            && (isEquality || isInequality))
        {
            context.ReportIssue(CreateDiagnostic(Rule, binaryExpression.GetLocation(), MessageEqualityPart(isEquality), "a range"));
        }
    }

    private static string MessageEqualityPart(bool isEquality) =>
        isEquality ? "equality" : "inequality";

    private static void CheckEquality(SonarSyntaxNodeReportingContext context)
    {
        var equals = (BinaryExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(equals).Symbol is IMethodSymbol { ContainingType: { } container } method
            && IsFloatingPointType(container)
            && (method.IsOperatorEquals() || method.IsOperatorNotEquals()))
        {
            var messageEqualityPart = MessageEqualityPart(equals.IsKind(SyntaxKind.EqualsExpression));
            var proposed = ProposedMessageForMemberAccess(context, equals.Right)
                ?? ProposedMessageForMemberAccess(context, equals.Left)
                ?? ProposedMessageForIdentifier(context, equals.Right)
                ?? ProposedMessageForIdentifier(context, equals.Left)
                ?? "a range";
            context.ReportIssue(CreateDiagnostic(Rule, equals.OperatorToken.GetLocation(), messageEqualityPart, proposed));
        }
    }

    private static string ProposedMessageForMemberAccess(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression) =>
        expression is MemberAccessExpressionSyntax memberAccess
        && SpecialMembers.TryGetValue(memberAccess.GetName(), out var proposedMethod)
        && context.SemanticModel.GetTypeInfo(memberAccess).ConvertedType is { } type
        && IsFloatingPointType(type)
            ? $"'{type.ToMinimalDisplayString(context.SemanticModel, memberAccess.SpanStart)}.{proposedMethod}()'"
            : null;

    private static string ProposedMessageForIdentifier(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression) =>
        expression is IdentifierNameSyntax identifier
        && SpecialMembers.TryGetValue(identifier.GetName(), out var proposedMethod)
        && context.SemanticModel.GetSymbolInfo(identifier).Symbol is { ContainingType: { } type }
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

    private static bool IsIndirectInequality(SemanticModel semanticModel, BinaryExpressionSyntax binaryExpression, BinaryExpressionSyntax left, BinaryExpressionSyntax right) =>
        binaryExpression.IsKind(SyntaxKind.LogicalOrExpression)
        && IsOperatorPair(left, right, SyntaxKind.GreaterThanToken, SyntaxKind.LessThanToken)
        && HasFloatingType(semanticModel, right);

    private static bool IsIndirectEquality(SemanticModel semanticModel, BinaryExpressionSyntax binaryExpression, BinaryExpressionSyntax left, BinaryExpressionSyntax right) =>
        binaryExpression.IsKind(SyntaxKind.LogicalAndExpression)
        && IsOperatorPair(left, right, SyntaxKind.GreaterThanEqualsToken, SyntaxKind.LessThanEqualsToken)
        && HasFloatingType(semanticModel, right);

    private static bool HasFloatingType(SemanticModel semanticModel, BinaryExpressionSyntax binary) =>
        IsExpressionFloatingType(semanticModel, binary.Right) || IsExpressionFloatingType(semanticModel, binary.Left);

    private static bool IsExpressionFloatingType(SemanticModel semanticModel, ExpressionSyntax expression) =>
        IsFloatingPointType(semanticModel.GetTypeInfo(expression).Type);

    private static bool IsOperatorPair(BinaryExpressionSyntax left, BinaryExpressionSyntax right, SyntaxKind first, SyntaxKind second) =>
        (left.OperatorToken.IsKind(first) && right.OperatorToken.IsKind(second))
        || (left.OperatorToken.IsKind(second) && right.OperatorToken.IsKind(first));
}
