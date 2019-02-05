/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class BooleanLiteralUnnecessary : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1125";
        private const string MessageFormat = "Remove the unnecessary Boolean literal(s).";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckEquals,
                SyntaxKind.EqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckLogicalAnd,
                SyntaxKind.LogicalAndExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckNotEquals,
                SyntaxKind.NotEqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckLogicalOr,
                SyntaxKind.LogicalOrExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckLogicalNot,
                SyntaxKind.LogicalNotExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckConditional,
                SyntaxKind.ConditionalExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckForLoopCondition,
                SyntaxKind.ForStatement);
        }

        private static void CheckForLoopCondition(SyntaxNodeAnalysisContext context)
        {
            var forLoop = (ForStatementSyntax)context.Node;

            if (forLoop.Condition != null &&
                CSharpEquivalenceChecker.AreEquivalent(forLoop.Condition.RemoveParentheses(), CSharpSyntaxHelper.TrueLiteralExpression))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, forLoop.Condition.GetLocation()));
            }
        }

        private static void CheckConditional(SyntaxNodeAnalysisContext context)
        {
            var conditional = (ConditionalExpressionSyntax)context.Node;
            var typeLeft = context.SemanticModel.GetTypeInfo(conditional.WhenTrue).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(conditional.WhenFalse).Type;
            if (ShouldNotReport(typeLeft, typeRight))
            {
                return;
            }

            var whenTrue = conditional.WhenTrue.RemoveParentheses();
            var whenFalse = conditional.WhenFalse.RemoveParentheses();

            var whenTrueIsTrue = whenTrue.IsKind(SyntaxKind.TrueLiteralExpression);
            var whenTrueIsFalse = whenTrue.IsKind(SyntaxKind.FalseLiteralExpression);
            var whenFalseIsTrue = whenFalse.IsKind(SyntaxKind.TrueLiteralExpression);
            var whenFalseIsFalse = whenFalse.IsKind(SyntaxKind.FalseLiteralExpression);

            var whenTrueIsBooleanConstant = whenTrueIsTrue || whenTrueIsFalse;
            var whenFalseIsBooleanConstant = whenFalseIsTrue || whenFalseIsFalse;

            if (whenTrueIsBooleanConstant ^ whenFalseIsBooleanConstant)
            {
                var side = whenTrueIsBooleanConstant
                    ? conditional.WhenTrue
                    : conditional.WhenFalse;

                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, side.GetLocation()));
                return;
            }

            var bothSideBool = whenTrueIsBooleanConstant && whenFalseIsBooleanConstant;
            var bothSideTrue = whenTrueIsTrue && whenFalseIsTrue;
            var bothSideFalse = whenTrueIsFalse && whenFalseIsFalse;

            if (bothSideBool && !bothSideFalse && !bothSideTrue)
            {
                var location = Location.Create(conditional.SyntaxTree,
                    new TextSpan(conditional.WhenTrue.SpanStart, conditional.WhenFalse.Span.End - conditional.WhenTrue.SpanStart));

                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
            }
        }

        private static void CheckLogicalNot(SyntaxNodeAnalysisContext context)
        {
            var logicalNot = (PrefixUnaryExpressionSyntax)context.Node;

            var logicalNotOperand = logicalNot.Operand.RemoveParentheses();

            if (IsBooleanLiteral(logicalNotOperand))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, logicalNot.Operand.GetLocation()));
            }
        }

        private static bool IsBooleanLiteral(SyntaxNode node) =>
            node.IsKind(SyntaxKind.TrueLiteralExpression) || node.IsKind(SyntaxKind.FalseLiteralExpression);

        private static void CheckLogicalOr(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: false))
            {
                return;
            }

            // When we have 'foo() || false', 'false' is the redundant part
            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, SyntaxKind.FalseLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, SyntaxKind.FalseLiteralExpression, context);

            // When we have 'foo() || true', 'foo()' is the redundant part
            CheckForBooleanConstantOnLeftReportOnInvertedLocation(binaryExpression, SyntaxKind.TrueLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnInvertedLocation(binaryExpression, SyntaxKind.TrueLiteralExpression, context);
        }

        private static void CheckNotEquals(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: false))
            {
                return;
            }

            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, SyntaxKind.FalseLiteralExpression, context);
            CheckForBooleanConstantOnLeftReportOnNormalLocation(binaryExpression, SyntaxKind.TrueLiteralExpression, context);

            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, SyntaxKind.FalseLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnNormalLocation(binaryExpression, SyntaxKind.TrueLiteralExpression, context);
        }

        private static void CheckLogicalAnd(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: true))
            {
                return;
            }

            // When we have 'foo() && true', 'true' is the redundant part
            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, SyntaxKind.TrueLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, SyntaxKind.TrueLiteralExpression, context);

            // When we have 'foo() && false', 'foo()' is the redundant part
            CheckForBooleanConstantOnLeftReportOnInvertedLocation(binaryExpression, SyntaxKind.FalseLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnInvertedLocation(binaryExpression, SyntaxKind.FalseLiteralExpression, context);
        }

        private static void CheckEquals(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: true))
            {
                return;
            }

            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, SyntaxKind.TrueLiteralExpression, context);
            CheckForBooleanConstantOnLeftReportOnNormalLocation(binaryExpression, SyntaxKind.FalseLiteralExpression, context);

            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, SyntaxKind.TrueLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnNormalLocation(binaryExpression, SyntaxKind.FalseLiteralExpression, context);
        }

        private static bool IsNullableBoolean(ITypeSymbol type) =>
            type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.Is(KnownType.System_Nullable_T) &&
            namedType.TypeArguments.Length == 1 &&
            namedType.TypeArguments[0].Is(KnownType.System_Boolean);

        private static bool CheckForNullabilityAndBooleanConstantsReport(BinaryExpressionSyntax binaryExpression,
            SyntaxNodeAnalysisContext context, bool reportOnTrue)
        {
            var typeLeft = context.SemanticModel.GetTypeInfo(binaryExpression.Left).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(binaryExpression.Right).Type;
            if (ShouldNotReport(typeLeft, typeRight))
            {
                return true;
            }

            var binaryExpressionLeft = binaryExpression.Left.RemoveParentheses();
            var binaryExpressionRight = binaryExpression.Right.RemoveParentheses();

            var leftIsTrue = binaryExpressionLeft.IsKind(SyntaxKind.TrueLiteralExpression);
            var leftIsFalse = binaryExpressionLeft.IsKind(SyntaxKind.FalseLiteralExpression);
            var rightIsTrue = binaryExpressionRight.IsKind(SyntaxKind.TrueLiteralExpression);
            var rightIsFalse = binaryExpressionRight.IsKind(SyntaxKind.FalseLiteralExpression);

            var leftIsBoolean = leftIsTrue || leftIsFalse;
            var rightIsBoolean = rightIsTrue || rightIsFalse;

            if (leftIsBoolean && rightIsBoolean)
            {
                var bothAreSame = (leftIsTrue && rightIsTrue) || (leftIsFalse && rightIsFalse);
                var errorLocation = bothAreSame
                    ? CalculateExtendedLocation(binaryExpression, false)
                    : CalculateExtendedLocation(binaryExpression, reportOnTrue == leftIsTrue);

                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, errorLocation));
                return true;
            }
            return false;
        }

        private static bool ShouldNotReport(ITypeSymbol typeLeft, ITypeSymbol typeRight)
        {
            return typeLeft == null
                || typeRight == null
                || IsNullableBoolean(typeLeft)
                || IsNullableBoolean(typeRight);
        }

        private static void CheckForBooleanConstantOnLeftReportOnExtendedLocation(BinaryExpressionSyntax binaryExpression,
            SyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Extended, context, leftSide: true);
        }

        private static void CheckForBooleanConstantOnRightReportOnExtendedLocation(BinaryExpressionSyntax binaryExpression,
            SyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Extended, context, leftSide: false);
        }

        private static void CheckForBooleanConstantOnLeftReportOnInvertedLocation(BinaryExpressionSyntax binaryExpression,
            SyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Inverted, context, leftSide: true);
        }

        private static void CheckForBooleanConstantOnRightReportOnInvertedLocation(BinaryExpressionSyntax binaryExpression,
            SyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Inverted, context, leftSide: false);
        }
        private static void CheckForBooleanConstantOnLeftReportOnNormalLocation(BinaryExpressionSyntax binaryExpression,
            SyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Normal, context, leftSide: true);
        }
        private static void CheckForBooleanConstantOnRightReportOnNormalLocation(BinaryExpressionSyntax binaryExpression,
            SyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Normal, context, leftSide: false);
        }


        private enum ErrorLocation
        {
            Normal,
            Extended,
            Inverted
        }

        private static void CheckForBooleanConstant(BinaryExpressionSyntax binaryExpression, SyntaxKind booleanSyntaxKind,
            ErrorLocation errorLocation, SyntaxNodeAnalysisContext context, bool leftSide)
        {
            var expression = leftSide
                ? binaryExpression.Left
                : binaryExpression.Right;

            if (!expression.RemoveParentheses().IsKind(booleanSyntaxKind))
            {
                return;
            }

            Location location;
            switch (errorLocation)
            {
                case ErrorLocation.Normal:
                    location = expression.GetLocation();
                    break;

                case ErrorLocation.Extended:
                    location = CalculateExtendedLocation(binaryExpression, leftSide);
                    break;

                case ErrorLocation.Inverted:
                    location = CalculateExtendedLocation(binaryExpression, !leftSide);
                    break;

                default:
                    location = null;
                    break;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
        }

        private static Location CalculateExtendedLocation(BinaryExpressionSyntax binaryExpression, bool leftSide)
        {
            return leftSide
                ? Location.Create(binaryExpression.SyntaxTree,
                        new TextSpan(binaryExpression.SpanStart,
                            binaryExpression.OperatorToken.Span.End - binaryExpression.SpanStart))
                : Location.Create(binaryExpression.SyntaxTree,
                        new TextSpan(binaryExpression.OperatorToken.SpanStart,
                            binaryExpression.Span.End - binaryExpression.OperatorToken.SpanStart));
        }
    }
}
