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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class BooleanLiteralUnnecessaryBase<TBinaryExpression, TSyntaxKind> : SonarDiagnosticAnalyzer
        where TBinaryExpression : SyntaxNode
        where TSyntaxKind : struct
    {
        protected enum ErrorLocation
        {
            // The BooleanLiteral node is highlighted
            Normal,

            // The BooleanLiteral node and the preceding operator are highlighted together
            Extended,

            // Inside a binary expression, the non-boolean literal is highlighted
            // e.g. for 'a && True', 'a' will be highlighted
            Inverted
        }

        protected internal const string DiagnosticId = "S1125";
        protected const string MessageFormat = "Remove the unnecessary Boolean literal(s).";

        protected abstract DiagnosticDescriptor Rule { get; }
        protected abstract TSyntaxKind TrueLiteral { get; }
        protected abstract TSyntaxKind FalseLiteral { get; }

        protected abstract SyntaxNode Left(TBinaryExpression binaryExpression);

        protected abstract SyntaxNode Right(TBinaryExpression binaryExpression);

        protected abstract SyntaxToken OperatorToken(TBinaryExpression binaryExpression);

        protected abstract bool IsKind(SyntaxNode syntaxNode, TSyntaxKind syntaxKind);

        // LogicalAnd (C#) / AndAlso (VB)
        protected void CheckAndExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: true))
            {
                return;
            }

            // When we have 'foo() && true', 'true' is the redundant part
            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, TrueLiteral, context);
            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, TrueLiteral, context);

            // When we have 'foo() && false', 'foo()' is the redundant part
            CheckForBooleanConstantOnLeftReportOnInvertedLocation(binaryExpression, FalseLiteral, context);
            CheckForBooleanConstantOnRightReportOnInvertedLocation(binaryExpression, FalseLiteral, context);
        }

        // LogicalOr (C#) / OrElse (VB)
        protected void CheckOrExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: false))
            {
                return;
            }

            // When we have 'foo() || false', 'false' is the redundant part
            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, FalseLiteral, context);
            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, FalseLiteral, context);

            // When we have 'foo() || true', 'foo()' is the redundant part
            CheckForBooleanConstantOnLeftReportOnInvertedLocation(binaryExpression, TrueLiteral, context);
            CheckForBooleanConstantOnRightReportOnInvertedLocation(binaryExpression, TrueLiteral, context);
        }

        protected void CheckEquals(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: true))
            {
                return;
            }

            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, TrueLiteral, context);
            CheckForBooleanConstantOnLeftReportOnNormalLocation(binaryExpression, FalseLiteral, context);

            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, TrueLiteral, context);
            CheckForBooleanConstantOnRightReportOnNormalLocation(binaryExpression, FalseLiteral, context);
        }

        protected void CheckNotEquals(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: false))
            {
                return;
            }

            CheckForBooleanConstantOnLeftReportOnExtendedLocation(binaryExpression, FalseLiteral, context);
            CheckForBooleanConstantOnLeftReportOnNormalLocation(binaryExpression, TrueLiteral, context);

            CheckForBooleanConstantOnRightReportOnExtendedLocation(binaryExpression, FalseLiteral, context);
            CheckForBooleanConstantOnRightReportOnNormalLocation(binaryExpression, TrueLiteral, context);
        }

        protected bool CheckForNullabilityAndBooleanConstantsReport(TBinaryExpression binaryExpression,
            SyntaxNodeAnalysisContext context, bool reportOnTrue)
        {
            var binaryExpressionLeft = Left(binaryExpression);
            var binaryExpressionRight = Right(binaryExpression);

            var typeLeft = context.SemanticModel.GetTypeInfo(binaryExpressionLeft).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(binaryExpressionRight).Type;
            if (ShouldNotReport(typeLeft, typeRight))
            {
                return true;
            }

            var leftIsTrue = IsKind(binaryExpressionLeft, TrueLiteral);
            var leftIsFalse = IsKind(binaryExpressionLeft, FalseLiteral);
            var rightIsTrue = IsKind(binaryExpressionRight, TrueLiteral);
            var rightIsFalse = IsKind(binaryExpressionRight, FalseLiteral);

            var leftIsBoolean = leftIsTrue || leftIsFalse;
            var rightIsBoolean = rightIsTrue || rightIsFalse;

            if (leftIsBoolean && rightIsBoolean)
            {
                var bothAreSame = (leftIsTrue && rightIsTrue) || (leftIsFalse && rightIsFalse);
                var errorLocation = bothAreSame
                    ? CalculateExtendedLocation(binaryExpression, false)
                    : CalculateExtendedLocation(binaryExpression, reportOnTrue == leftIsTrue);

                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, errorLocation));
                return true;
            }
            return false;
        }

        protected static bool ShouldNotReport(ITypeSymbol typeLeft, ITypeSymbol typeRight)
        {
            return typeLeft == null
                || typeRight == null
                || IsNullableBoolean(typeLeft)
                || IsNullableBoolean(typeRight);
        }

        protected static bool IsNullableBoolean(ITypeSymbol type) =>
            type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.Is(KnownType.System_Nullable_T) &&
            namedType.TypeArguments.Length == 1 &&
            namedType.TypeArguments[0].Is(KnownType.System_Boolean);

        protected void CheckForBooleanConstantOnLeftReportOnExtendedLocation(TBinaryExpression binaryExpression,
            TSyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Extended, context, leftSide: true);
        }

        protected void CheckForBooleanConstantOnRightReportOnExtendedLocation(TBinaryExpression binaryExpression,
            TSyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Extended, context, leftSide: false);
        }

        protected void CheckForBooleanConstantOnLeftReportOnInvertedLocation(TBinaryExpression binaryExpression,
            TSyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Inverted, context, leftSide: true);
        }

        protected void CheckForBooleanConstantOnRightReportOnInvertedLocation(TBinaryExpression binaryExpression,
            TSyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Inverted, context, leftSide: false);
        }

        protected void CheckForBooleanConstantOnLeftReportOnNormalLocation(TBinaryExpression binaryExpression,
            TSyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Normal, context, leftSide: true);
        }

        protected void CheckForBooleanConstantOnRightReportOnNormalLocation(TBinaryExpression binaryExpression,
            TSyntaxKind booleanSyntaxKind, SyntaxNodeAnalysisContext context)
        {
            CheckForBooleanConstant(binaryExpression, booleanSyntaxKind, ErrorLocation.Normal, context, leftSide: false);
        }

        protected void CheckForBooleanConstant(TBinaryExpression binaryExpression, TSyntaxKind booleanSyntaxKind,
            ErrorLocation errorLocation, SyntaxNodeAnalysisContext context, bool leftSide)
        {
            var expression = leftSide ? Left(binaryExpression) : Right(binaryExpression);

            if (!IsKind(expression, booleanSyntaxKind))
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

            context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, location));
        }

        protected Location CalculateExtendedLocation(TBinaryExpression binaryExpression, bool leftSide)
        {
            return leftSide
                ? Location.Create(binaryExpression.SyntaxTree,
                        new TextSpan(binaryExpression.SpanStart,
                            OperatorToken(binaryExpression).Span.End - binaryExpression.SpanStart))
                : Location.Create(binaryExpression.SyntaxTree,
                        new TextSpan(OperatorToken(binaryExpression).SpanStart,
                            binaryExpression.Span.End - OperatorToken(binaryExpression).SpanStart));
        }
    }
}
