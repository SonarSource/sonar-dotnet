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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class BooleanLiteralUnnecessaryBase<TBinaryExpression> : SonarDiagnosticAnalyzer
        where TBinaryExpression : SyntaxNode
    {
        internal const string DiagnosticId = "S1125";
        protected const string MessageFormat = "Remove the unnecessary Boolean literal(s).";

        protected delegate bool IsBooleanLiteralKind(SyntaxNode node);

        protected abstract bool IsBooleanLiteral(SyntaxNode node);

        protected abstract SyntaxNode GetLeftNode(TBinaryExpression binaryExpression);

        protected abstract SyntaxNode GetRightNode(TBinaryExpression binaryExpression);

        protected abstract SyntaxToken GetOperatorToken(TBinaryExpression binaryExpression);

        protected abstract bool IsTrueLiteralKind(SyntaxNode syntaxNode);

        protected abstract bool IsFalseLiteralKind(SyntaxNode syntaxNode);

        protected abstract SyntaxNode RemoveParentheses(SyntaxNode syntaxNode);

        // LogicalAnd (C#) / AndAlso (VB)
        protected void CheckAndExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: true))
            {
                return;
            }

            // When we have 'EXPR And True', the true literal is the redundant part
            CheckForBooleanConstantOnLeft(binaryExpression, IsTrueLiteralKind,
                ErrorLocation.BoolLiteralAndOperator, context);
            CheckForBooleanConstantOnRight(binaryExpression, IsTrueLiteralKind,
                ErrorLocation.BoolLiteralAndOperator, context);

            // 'EXPR And False' is always False, thus EXPR is the redundant part
            CheckForBooleanConstantOnLeft(binaryExpression, IsFalseLiteralKind,
                ErrorLocation.NonBoolLiteralExpression, context);
            CheckForBooleanConstantOnRight(binaryExpression, IsFalseLiteralKind,
                ErrorLocation.NonBoolLiteralExpression, context);
        }

        // LogicalOr (C#) / OrElse (VB)
        protected void CheckOrExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: false))
            {
                return;
            }

            // When we have 'EXPR Or False', the false literal is the redundant part
            CheckForBooleanConstantOnLeft(binaryExpression, IsFalseLiteralKind,
                ErrorLocation.BoolLiteralAndOperator, context);
            CheckForBooleanConstantOnRight(binaryExpression, IsFalseLiteralKind,
                ErrorLocation.BoolLiteralAndOperator, context);

            // 'EXPR Or True' is always True, thus EXPR is the redundant part
            CheckForBooleanConstantOnLeft(binaryExpression, IsTrueLiteralKind,
                ErrorLocation.NonBoolLiteralExpression, context);
            CheckForBooleanConstantOnRight(binaryExpression, IsTrueLiteralKind,
                ErrorLocation.NonBoolLiteralExpression, context);
        }

        protected void CheckEquals(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: true))
            {
                return;
            }

            CheckForBooleanConstantOnLeft(binaryExpression, IsTrueLiteralKind,
                ErrorLocation.BoolLiteralAndOperator, context);
            CheckForBooleanConstantOnLeft(binaryExpression, IsFalseLiteralKind,
                ErrorLocation.BoolLiteral, context);

            CheckForBooleanConstantOnRight(binaryExpression, IsTrueLiteralKind,
                ErrorLocation.BoolLiteralAndOperator, context);
            CheckForBooleanConstantOnRight(binaryExpression, IsFalseLiteralKind,
                ErrorLocation.BoolLiteral, context);
        }

        protected void CheckNotEquals(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            if (CheckForNullabilityAndBooleanConstantsReport(binaryExpression, context, reportOnTrue: false))
            {
                return;
            }

            CheckForBooleanConstantOnLeft(binaryExpression, IsFalseLiteralKind,
                ErrorLocation.BoolLiteralAndOperator, context);
            CheckForBooleanConstantOnLeft(binaryExpression, IsTrueLiteralKind,
                ErrorLocation.BoolLiteral, context);

            CheckForBooleanConstantOnRight(binaryExpression, IsFalseLiteralKind,
                ErrorLocation.BoolLiteralAndOperator, context);
            CheckForBooleanConstantOnRight(binaryExpression, IsTrueLiteralKind,
                ErrorLocation.BoolLiteral, context);
        }

        protected void CheckTernaryExpressionBranches(SyntaxNodeAnalysisContext context,
            SyntaxTree ternaryTree, SyntaxNode thenBranch, SyntaxNode elseBranch)
        {
            var thenNoParantheses = RemoveParentheses(thenBranch);
            var elseNoParantheses = RemoveParentheses(elseBranch);

            var thenIsBooleanLiteral = IsBooleanLiteral(thenNoParantheses);
            var elseIsBooleanLiteral = IsBooleanLiteral(elseNoParantheses);

            var bothSideBool = thenIsBooleanLiteral && elseIsBooleanLiteral;
            var bothSideTrue = IsTrueLiteralKind(thenNoParantheses) && IsTrueLiteralKind(elseNoParantheses);
            var bothSideFalse = IsFalseLiteralKind(thenNoParantheses) && IsFalseLiteralKind(elseNoParantheses);

            if (bothSideBool && !bothSideFalse && !bothSideTrue)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], thenBranch.CreateLocation(elseBranch)));
            }
            if (thenIsBooleanLiteral ^ elseIsBooleanLiteral)
            {
                // one side is boolean literal, the other is NOT boolean literal
                var booleanLiteralSide = thenIsBooleanLiteral ? thenBranch : elseBranch;

                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], booleanLiteralSide.GetLocation()));
            }
        }

        protected bool CheckForNullabilityAndBooleanConstantsReport(TBinaryExpression binaryExpression,
            SyntaxNodeAnalysisContext context, bool reportOnTrue)
        {
            var binaryExpressionLeft = RemoveParentheses(GetLeftNode(binaryExpression));
            var binaryExpressionRight = RemoveParentheses(GetRightNode(binaryExpression));

            var typeLeft = context.SemanticModel.GetTypeInfo(binaryExpressionLeft).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(binaryExpressionRight).Type;
            if (typeLeft.IsNullableBoolean() || typeRight.IsNullableBoolean())
            {
                return true;
            }

            var leftIsTrue = IsTrueLiteralKind(binaryExpressionLeft);
            var leftIsFalse = IsFalseLiteralKind(binaryExpressionLeft);
            var rightIsTrue = IsTrueLiteralKind(binaryExpressionRight);
            var rightIsFalse = IsFalseLiteralKind(binaryExpressionRight);

            var leftIsBoolean = leftIsTrue || leftIsFalse;
            var rightIsBoolean = rightIsTrue || rightIsFalse;

            if (leftIsBoolean && rightIsBoolean)
            {
                var bothAreSame = (leftIsTrue && rightIsTrue) || (leftIsFalse && rightIsFalse);
                var errorLocation = bothAreSame
                    ? CalculateExtendedLocation(binaryExpression, false)
                    : CalculateExtendedLocation(binaryExpression, reportOnTrue == leftIsTrue);

                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], errorLocation));
                return true;
            }
            return false;
        }

        protected void CheckForBooleanConstantOnLeft(TBinaryExpression binaryExpression,
            IsBooleanLiteralKind isBooleanLiteralKind, ErrorLocation errorLocation, SyntaxNodeAnalysisContext context) =>
            CheckForBooleanConstant(binaryExpression, isBooleanLiteralKind, errorLocation, context, isLeftSide: true);

        protected void CheckForBooleanConstantOnRight(TBinaryExpression binaryExpression,
            IsBooleanLiteralKind isBooleanLiteralKind, ErrorLocation errorLocation, SyntaxNodeAnalysisContext context) =>
            CheckForBooleanConstant(binaryExpression, isBooleanLiteralKind, errorLocation, context, isLeftSide: false);

        protected void CheckForBooleanConstant(TBinaryExpression binaryExpression, IsBooleanLiteralKind isBooleanLiteralKind,
            ErrorLocation errorLocation, SyntaxNodeAnalysisContext context, bool isLeftSide)
        {
            var expression = isLeftSide
                ? RemoveParentheses(GetLeftNode(binaryExpression))
                : RemoveParentheses(GetRightNode(binaryExpression));

            if (!isBooleanLiteralKind(expression))
            {
                return;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], GetLocation()));

            Location GetLocation()
            {
                switch (errorLocation)
                {
                    case ErrorLocation.BoolLiteral:
                        return expression.GetLocation();

                    case ErrorLocation.BoolLiteralAndOperator:
                        return CalculateExtendedLocation(binaryExpression, isLeftSide);

                    case ErrorLocation.NonBoolLiteralExpression:
                        return CalculateExtendedLocation(binaryExpression, !isLeftSide);

                    default:
                        return null;
                }
            }
        }

        protected Location CalculateExtendedLocation(TBinaryExpression binaryExpression, bool isLeftSide)
        {
            return isLeftSide
                ? binaryExpression.CreateLocation(GetOperatorToken(binaryExpression))
                : GetOperatorToken(binaryExpression).CreateLocation(binaryExpression);
        }

        protected enum ErrorLocation
        {
            // The BooleanLiteral node is highlighted
            BoolLiteral,

            // The BooleanLiteral node and the operator are highlighted together
            BoolLiteralAndOperator,

            // Inside a binary expression, the expression that is not a boolean literal is highlighted
            // e.g. for 'foo() && True', 'foo()' will be highlighted
            NonBoolLiteralExpression
        }
    }
}
