﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
    public class BooleanLiteralUnnecessary : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1125";
        private const string MessageFormat = "Remove the unnecessary Boolean literal(s).";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckEquals(c),
                SyntaxKind.EqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckLogicalAnd(c),
                SyntaxKind.LogicalAndExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckNotEquals(c),
                SyntaxKind.NotEqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckLogicalOr(c),
                SyntaxKind.LogicalOrExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckLogicalNot(c),
                SyntaxKind.LogicalNotExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckConditional(c),
                SyntaxKind.ConditionalExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForLoopCondition(c),
                SyntaxKind.ForStatement);
        }

        private static void CheckForLoopCondition(SyntaxNodeAnalysisContext context)
        {
            var forLoop = (ForStatementSyntax)context.Node;

            if (forLoop.Condition != null &&
                EquivalenceChecker.AreEquivalent(forLoop.Condition.RemoveParentheses(), CSharpSyntaxHelper.TrueLiteralExpression))
            {
                Diagnostic.Create(rule, forLoop.Condition.GetLocation()).ReportFor(context);
            }
        }

        private static void CheckConditional(SyntaxNodeAnalysisContext context)
        {
            var conditional = (ConditionalExpressionSyntax)context.Node;
            if (IsOnNullableBoolean(conditional, context.SemanticModel))
            {
                return;
            }

            var whenTrue = conditional.WhenTrue.RemoveParentheses();
            var whenFalse = conditional.WhenFalse.RemoveParentheses();

            var whenTrueIsTrue = EquivalenceChecker.AreEquivalent(whenTrue, CSharpSyntaxHelper.TrueLiteralExpression);
            var whenTrueIsFalse = EquivalenceChecker.AreEquivalent(whenTrue, CSharpSyntaxHelper.FalseLiteralExpression);
            var whenFalseIsTrue = EquivalenceChecker.AreEquivalent(whenFalse, CSharpSyntaxHelper.TrueLiteralExpression);
            var whenFalseIsFalse = EquivalenceChecker.AreEquivalent(whenFalse, CSharpSyntaxHelper.FalseLiteralExpression);

            var whenTrueIsBooleanConstant = whenTrueIsTrue || whenTrueIsFalse;
            var whenFalseIsBooleanConstant = whenFalseIsTrue || whenFalseIsFalse;

            if (whenTrueIsBooleanConstant ^ whenFalseIsBooleanConstant)
            {
                var side = whenTrueIsBooleanConstant
                    ? conditional.WhenTrue
                    : conditional.WhenFalse;

                Diagnostic.Create(rule, side.GetLocation()).ReportFor(context);
                return;
            }

            var bothSideBool = whenTrueIsBooleanConstant && whenFalseIsBooleanConstant;
            var bothSideTrue = whenTrueIsTrue && whenFalseIsTrue;
            var bothSideFalse = whenTrueIsFalse && whenFalseIsFalse;

            if (bothSideBool && !bothSideFalse && !bothSideTrue)
            {
                var location = Location.Create(conditional.SyntaxTree,
                    new TextSpan(conditional.WhenTrue.SpanStart, conditional.WhenFalse.Span.End - conditional.WhenTrue.SpanStart));

                Diagnostic.Create(rule, location).ReportFor(context);
            }
        }

        private static void CheckLogicalNot(SyntaxNodeAnalysisContext context)
        {
            var logicalNot = (PrefixUnaryExpressionSyntax)context.Node;

            var logicalNotOperand = logicalNot.Operand.RemoveParentheses();

            if (EquivalenceChecker.AreEquivalent(logicalNotOperand, CSharpSyntaxHelper.TrueLiteralExpression) ||
                EquivalenceChecker.AreEquivalent(logicalNotOperand, CSharpSyntaxHelper.FalseLiteralExpression))
            {
                Diagnostic.Create(rule, logicalNot.Operand.GetLocation()).ReportFor(context);
            }
        }

        private static void CheckLogicalOr(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: false))
            {
                return;
            }

            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, CSharpSyntaxHelper.FalseLiteralExpression, context);
            CheckForBooleanConstantOnLeftReportOnInvertedLocation(binaryExpression, CSharpSyntaxHelper.TrueLiteralExpression, context);

            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, CSharpSyntaxHelper.FalseLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnInvertedLocation(binaryExpression, CSharpSyntaxHelper.TrueLiteralExpression, context);
        }

        private static void CheckNotEquals(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: false))
            {
                return;
            }

            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, CSharpSyntaxHelper.FalseLiteralExpression, context);
            CheckForBooleanConstantOnLeftReportOnNormalLocation(binaryExpression, CSharpSyntaxHelper.TrueLiteralExpression, context);

            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, CSharpSyntaxHelper.FalseLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnNormalLocation(binaryExpression, CSharpSyntaxHelper.TrueLiteralExpression, context);
        }

        private static void CheckLogicalAnd(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: true))
            {
                return;
            }

            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, CSharpSyntaxHelper.TrueLiteralExpression, context);
            CheckForBooleanConstantOnLeftReportOnInvertedLocation(binaryExpression, CSharpSyntaxHelper.FalseLiteralExpression, context);

            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, CSharpSyntaxHelper.TrueLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnInvertedLocation(binaryExpression, CSharpSyntaxHelper.FalseLiteralExpression, context);
        }

        private static void CheckEquals(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: true))
            {
                return;
            }

            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, CSharpSyntaxHelper.TrueLiteralExpression, context);
            CheckForBooleanConstantOnLeftReportOnNormalLocation(binaryExpression, CSharpSyntaxHelper.FalseLiteralExpression, context);

            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, CSharpSyntaxHelper.TrueLiteralExpression, context);
            CheckForBooleanConstantOnRightReportOnNormalLocation(binaryExpression, CSharpSyntaxHelper.FalseLiteralExpression, context);
        }

        private static bool IsOnNullableBoolean(ConditionalExpressionSyntax conditionalExpression, SemanticModel semanticModel)
        {
            var typeLeft = semanticModel.GetTypeInfo(conditionalExpression.WhenTrue).Type;
            var typeRight = semanticModel.GetTypeInfo(conditionalExpression.WhenFalse).Type;
            return IsOnNullableBoolean(typeLeft, typeRight);
        }

        private static bool IsOnNullableBoolean(ITypeSymbol typeLeft, ITypeSymbol typeRight)
        {
            if (typeLeft == null || typeRight == null)
            {
                return false;
            }

            return IsNullableBoolean(typeLeft) || IsNullableBoolean(typeRight);
        }

        private static bool IsNullableBoolean(ITypeSymbol type)
        {
            if (!type.OriginalDefinition.Is(KnownType.System_Nullable_T))
            {
                return false;
            }

            var namedType = (INamedTypeSymbol)type;

            return namedType.TypeArguments.Length == 1 &&
                namedType.TypeArguments[0].Is(KnownType.System_Boolean);
        }

        private static bool CheckForNullabilityAndBooleanConstantsReport(BinaryExpressionSyntax binaryExpression,
            SyntaxNodeAnalysisContext context, bool reportOnTrue)
        {
            var typeLeft = context.SemanticModel.GetTypeInfo(binaryExpression.Left).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(binaryExpression.Right).Type;
            var shouldNotReport = IsOnNullableBoolean(typeLeft, typeRight);
            if (shouldNotReport)
            {
                return true;
            }

            var binaryExpressionLeft = binaryExpression.Left.RemoveParentheses();
            var binaryExpressionRight = binaryExpression.Right.RemoveParentheses();

            var leftIsTrue = EquivalenceChecker.AreEquivalent(binaryExpressionLeft, CSharpSyntaxHelper.TrueLiteralExpression);
            var leftIsFalse = EquivalenceChecker.AreEquivalent(binaryExpressionLeft, CSharpSyntaxHelper.FalseLiteralExpression);
            var rightIsTrue = EquivalenceChecker.AreEquivalent(binaryExpressionRight, CSharpSyntaxHelper.TrueLiteralExpression);
            var rightIsFalse = EquivalenceChecker.AreEquivalent(binaryExpressionRight, CSharpSyntaxHelper.FalseLiteralExpression);

            var leftIsBoolean = leftIsTrue || leftIsFalse;
            var rightIsBoolean = rightIsTrue || rightIsFalse;

            if (leftIsBoolean && rightIsBoolean)
            {
                var bothAreSame = (leftIsTrue && rightIsTrue) || (leftIsFalse && rightIsFalse);
                var errorLocation = bothAreSame
                    ? CalculateExtendedLocation(binaryExpression, false)
                    : CalculateExtendedLocation(binaryExpression, reportOnTrue == leftIsTrue);

                Diagnostic.Create(rule, errorLocation).ReportFor(context);
                return true;
            }
            return false;
        }

        private static void CheckForBooleanConstantOnLeftReportOnInvertedLocation(BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax booleanContantExpression, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanContantExpression, ErrorLocation.Inverted, context, leftSide: true);
        }

        private static void CheckForBooleanConstantOnRightReportOnInvertedLocation(BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax booleanContantExpression, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanContantExpression, ErrorLocation.Inverted, context, leftSide: false);
        }

        private static void CheckForBooleanConstantOnLeftReportOnExtendedLocation(BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax booleanContantExpression, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanContantExpression, ErrorLocation.Extended, context, leftSide: true);
        }
        private static void CheckForBooleanConstantOnRightReportOnExtendedLocation(BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax booleanContantExpression, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanContantExpression, ErrorLocation.Extended, context, leftSide: false);
        }
        private static void CheckForBooleanConstantOnLeftReportOnNormalLocation(BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax booleanContantExpression, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanContantExpression, ErrorLocation.Normal, context, leftSide: true);
        }
        private static void CheckForBooleanConstantOnRightReportOnNormalLocation(BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax booleanContantExpression, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanContantExpression, ErrorLocation.Normal, context, leftSide: false);
        }

        private enum ErrorLocation
        {
            Normal,
            Extended,
            Inverted
        }

        private static void CheckForBooleanConstant(BinaryExpressionSyntax binaryExpression, ExpressionSyntax booleanContantExpression,
            ErrorLocation errorLocation, SyntaxNodeAnalysisContext context, bool leftSide)
        {
            var expression = leftSide
                ? binaryExpression.Left
                : binaryExpression.Right;

            if (!EquivalenceChecker.AreEquivalent(expression.RemoveParentheses(), booleanContantExpression))
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

            Diagnostic.Create(rule, location).ReportFor(context);
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
