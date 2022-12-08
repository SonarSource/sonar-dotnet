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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EqualityOnFloatingPoint : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1244";
        private const string MessageFormat = "Do not check floating point {0} with exact values, use a range instead.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<string> EqualityOperators = new HashSet<string> { "op_Equality", "op_Inequality" };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckEquality,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckLogicalExpression,
                SyntaxKind.LogicalAndExpression,
                SyntaxKind.LogicalOrExpression);
        }

        private static void CheckLogicalExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            var left = TryGetBinaryExpression(binaryExpression.Left);
            var right = TryGetBinaryExpression(binaryExpression.Right);

            if (right == null || left == null)
            {
                return;
            }

            var eqRight = CSharpEquivalenceChecker.AreEquivalent(right.Right, left.Right);
            var eqLeft = CSharpEquivalenceChecker.AreEquivalent(right.Left, left.Left);
            if (!eqRight || !eqLeft)
            {
                return;
            }

            var isEquality = IsIndirectEquality(context.SemanticModel, binaryExpression, left, right);

            if (isEquality || IsIndirectInequality(context.SemanticModel, binaryExpression, left, right))
            {
                var messageEqualityPart = GetMessageEqualityPart(isEquality);

                context.ReportIssue(Diagnostic.Create(rule, binaryExpression.GetLocation(), messageEqualityPart));
            }
        }

        private static string GetMessageEqualityPart(bool isEquality) =>
            isEquality ? "equality" : "inequality";

        private static void CheckEquality(SyntaxNodeAnalysisContext context)
        {
            var equals = (BinaryExpressionSyntax)context.Node;

            if (context.SemanticModel.GetSymbolInfo(equals).Symbol is IMethodSymbol { ContainingType: { } container, Name: { } equalitySymbolName }
                && IsFloatingPointNumberType(container)
                && EqualityOperators.Contains(equalitySymbolName))
            {
                var messageEqualityPart = GetMessageEqualityPart(equals.IsKind(SyntaxKind.EqualsExpression));

                context.ReportIssue(Diagnostic.Create(rule, equals.OperatorToken.GetLocation(), messageEqualityPart));
            }
        }

        // All floating point types that suffer from equivalence problems. These are all .net floating point types except decimal.
        // power 2 based types like double implement IFloatingPointIeee754 but power 10 based decimal not (implements IFloatingPoint).
        // Ieee754 also allows power 10 based representations but uses another layout than the .Net decimal type.
        // IFloatingPointIeee754 defines Epsilon which indicates problems with equivalence checking.
        private static bool IsFloatingPointNumberType(ITypeSymbol type) =>
            type.IsAny(KnownType.FloatingPointNumbers)
            || (type.Is(KnownType.System_Numerics_IEqualityOperators_TSelf_TOther_TResult) // The operator originates from a virtual static member
                && type is INamedTypeSymbol { TypeArguments: { } typeArguments }
                && typeArguments.OfType<ITypeParameterSymbol>().Any(IsFloatingPointNumberType))
            || (type is ITypeParameterSymbol { ConstraintTypes: { } constraintTypes }
                && constraintTypes.Any(constraint => constraint.DerivesOrImplements(KnownType.System_Numerics_IFloatingPointIeee754_TSelf)));

        private static BinaryExpressionSyntax TryGetBinaryExpression(ExpressionSyntax expression) =>
            expression.RemoveParentheses() as BinaryExpressionSyntax;

        private static bool IsIndirectInequality(SemanticModel semanticModel, BinaryExpressionSyntax binaryExpression, BinaryExpressionSyntax left, BinaryExpressionSyntax right) =>
            binaryExpression.IsKind(SyntaxKind.LogicalOrExpression)
                && HasAppropriateOperatorsForInequality(left, right)
                && HasFloatingType(semanticModel, right.Left, right.Right);

        private static bool IsIndirectEquality(SemanticModel semanticModel, BinaryExpressionSyntax binaryExpression, BinaryExpressionSyntax left, BinaryExpressionSyntax right) =>
            binaryExpression.IsKind(SyntaxKind.LogicalAndExpression)
                && HasAppropriateOperatorsForEquality(left, right)
                && HasFloatingType(semanticModel, right.Left, right.Right);

        private static bool HasFloatingType(SemanticModel semanticModel, ExpressionSyntax left, ExpressionSyntax right) =>
            IsExpressionFloatingType(semanticModel, right) || IsExpressionFloatingType(semanticModel, left);

        private static bool IsExpressionFloatingType(SemanticModel semanticModel, ExpressionSyntax expression) =>
            IsFloatingPointNumberType(semanticModel.GetTypeInfo(expression).Type);

        private static bool HasAppropriateOperatorsForEquality(BinaryExpressionSyntax left, BinaryExpressionSyntax right) =>
            (right.OperatorToken.Kind() is SyntaxKind.LessThanEqualsToken && left.OperatorToken.Kind() is SyntaxKind.GreaterThanEqualsToken)
            || (right.OperatorToken.Kind() is SyntaxKind.GreaterThanEqualsToken && left.OperatorToken.Kind() is SyntaxKind.LessThanEqualsToken);

        private static bool HasAppropriateOperatorsForInequality(BinaryExpressionSyntax left, BinaryExpressionSyntax right) =>
            (right.OperatorToken.Kind() is SyntaxKind.LessThanToken && left.OperatorToken.Kind() is SyntaxKind.GreaterThanToken)
            || (right.OperatorToken.Kind() is SyntaxKind.GreaterThanToken && left.OperatorToken.Kind() is SyntaxKind.LessThanToken);
    }
}
