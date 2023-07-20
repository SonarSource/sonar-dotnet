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

namespace SonarAnalyzer.Rules
{
    public abstract class BooleanLiteralUnnecessaryBase<TBinaryExpression, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TBinaryExpression : SyntaxNode
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S1125";

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

        protected delegate bool IsBooleanLiteralKind(SyntaxNode node);

        protected abstract bool IsBooleanLiteral(SyntaxNode node);
        protected abstract SyntaxNode GetLeftNode(TBinaryExpression binary);
        protected abstract SyntaxNode GetRightNode(TBinaryExpression binary);
        protected abstract SyntaxToken GetOperatorToken(TBinaryExpression binary);
        protected abstract bool IsTrueLiteralKind(SyntaxNode syntaxNode);
        protected abstract bool IsFalseLiteralKind(SyntaxNode syntaxNode);
        // For C# 7 syntax
        protected virtual bool IsInsideTernaryWithThrowExpression(TBinaryExpression syntaxNode) => false;

        protected override string MessageFormat => "Remove the unnecessary Boolean literal(s).";

        protected BooleanLiteralUnnecessaryBase() : base(DiagnosticId) { }

        // LogicalAnd (C#) / AndAlso (VB)
        protected void CheckAndExpression(SonarSyntaxNodeReportingContext context)
        {
            var binary = (TBinaryExpression)context.Node;
            if (IsInsideTernaryWithThrowExpression(binary) || CheckForNullabilityAndBooleanConstantsReport(context, binary, reportOnTrue: true))
            {
                return;
            }

            // When we have 'EXPR And True', the true literal is the redundant part
            CheckForBooleanConstantOnLeft(context, binary, IsTrueLiteralKind, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnRight(context, binary, IsTrueLiteralKind, ErrorLocation.BoolLiteralAndOperator);

            // 'EXPR And False' is always False, thus EXPR is the redundant part
            CheckForBooleanConstantOnLeft(context, binary, IsFalseLiteralKind, ErrorLocation.NonBoolLiteralExpression);
            CheckForBooleanConstantOnRight(context, binary, IsFalseLiteralKind, ErrorLocation.NonBoolLiteralExpression);
        }

        // LogicalOr (C#) / OrElse (VB)
        protected void CheckOrExpression(SonarSyntaxNodeReportingContext context)
        {
            var binary = (TBinaryExpression)context.Node;
            if (IsInsideTernaryWithThrowExpression(binary) || CheckForNullabilityAndBooleanConstantsReport(context, binary, reportOnTrue: false))
            {
                return;
            }

            // When we have 'EXPR Or False', the false literal is the redundant part
            CheckForBooleanConstantOnLeft(context, binary, IsFalseLiteralKind, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnRight(context, binary, IsFalseLiteralKind, ErrorLocation.BoolLiteralAndOperator);

            // 'EXPR Or True' is always True, thus EXPR is the redundant part
            CheckForBooleanConstantOnLeft(context, binary, IsTrueLiteralKind, ErrorLocation.NonBoolLiteralExpression);
            CheckForBooleanConstantOnRight(context, binary, IsTrueLiteralKind, ErrorLocation.NonBoolLiteralExpression);
        }

        protected void CheckEquals(SonarSyntaxNodeReportingContext context)
        {
            var binary = (TBinaryExpression)context.Node;
            if (IsInsideTernaryWithThrowExpression(binary)
                || CheckForNullabilityAndBooleanConstantsReport(context, binary, reportOnTrue: true))
            {
                return;
            }

            CheckForBooleanConstantOnLeft(context, binary, IsTrueLiteralKind, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnLeft(context, binary, IsFalseLiteralKind, ErrorLocation.BoolLiteral);

            CheckForBooleanConstantOnRight(context, binary, IsTrueLiteralKind, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnRight(context, binary, IsFalseLiteralKind, ErrorLocation.BoolLiteral);
        }

        protected void CheckNotEquals(SonarSyntaxNodeReportingContext context)
        {
            var binary = (TBinaryExpression)context.Node;
            if (IsInsideTernaryWithThrowExpression(binary) || CheckForNullabilityAndBooleanConstantsReport(context, binary, reportOnTrue: false))
            {
                return;
            }

            CheckForBooleanConstantOnLeft(context, binary, IsFalseLiteralKind, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnLeft(context, binary, IsTrueLiteralKind, ErrorLocation.BoolLiteral);

            CheckForBooleanConstantOnRight(context, binary, IsFalseLiteralKind, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnRight(context, binary, IsTrueLiteralKind, ErrorLocation.BoolLiteral);
        }

        protected void CheckTernaryExpressionBranches(SonarSyntaxNodeReportingContext context, SyntaxTree ternaryTree, SyntaxNode thenBranch, SyntaxNode elseBranch)
        {
            var thenNoParantheses = Language.Syntax.RemoveParentheses(thenBranch);
            var elseNoParantheses = Language.Syntax.RemoveParentheses(elseBranch);

            var thenIsBooleanLiteral = IsBooleanLiteral(thenNoParantheses);
            var elseIsBooleanLiteral = IsBooleanLiteral(elseNoParantheses);

            var bothSideBool = thenIsBooleanLiteral && elseIsBooleanLiteral;
            var bothSideTrue = IsTrueLiteralKind(thenNoParantheses) && IsTrueLiteralKind(elseNoParantheses);
            var bothSideFalse = IsFalseLiteralKind(thenNoParantheses) && IsFalseLiteralKind(elseNoParantheses);

            if (bothSideBool && !bothSideFalse && !bothSideTrue)
            {
                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], thenBranch.CreateLocation(elseBranch)));
            }
            if (thenIsBooleanLiteral ^ elseIsBooleanLiteral)
            {
                // one side is boolean literal, the other is NOT boolean literal
                var booleanLiteralSide = thenIsBooleanLiteral ? thenBranch : elseBranch;

                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], booleanLiteralSide.GetLocation()));
            }
        }

        private bool CheckForNullabilityAndBooleanConstantsReport(SonarSyntaxNodeReportingContext context, TBinaryExpression binary, bool reportOnTrue)
        {
            var binaryExpressionLeft = Language.Syntax.RemoveParentheses(GetLeftNode(binary));
            var binaryExpressionRight = Language.Syntax.RemoveParentheses(GetRightNode(binary));

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
                    ? CalculateExtendedLocation(binary, false)
                    : CalculateExtendedLocation(binary, reportOnTrue == leftIsTrue);

                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], errorLocation));
                return true;
            }
            return false;
        }

        private void CheckForBooleanConstantOnLeft(SonarSyntaxNodeReportingContext context, TBinaryExpression binary, IsBooleanLiteralKind isBooleanLiteralKind, ErrorLocation errorLocation) =>
            CheckForBooleanConstant(context, binary, isBooleanLiteralKind, errorLocation, isLeftSide: true);

        private void CheckForBooleanConstantOnRight(SonarSyntaxNodeReportingContext context, TBinaryExpression binary, IsBooleanLiteralKind isBooleanLiteralKind, ErrorLocation errorLocation) =>
            CheckForBooleanConstant(context, binary, isBooleanLiteralKind, errorLocation, isLeftSide: false);

        private void CheckForBooleanConstant(SonarSyntaxNodeReportingContext context,
                                             TBinaryExpression binary,
                                             IsBooleanLiteralKind isBooleanLiteralKind,
                                             ErrorLocation errorLocation,
                                             bool isLeftSide)
        {
            var expression = isLeftSide
                ? Language.Syntax.RemoveParentheses(GetLeftNode(binary))
                : Language.Syntax.RemoveParentheses(GetRightNode(binary));

            if (!isBooleanLiteralKind(expression))
            {
                return;
            }

            context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], GetLocation()));

            Location GetLocation()
            {
                switch (errorLocation)
                {
                    case ErrorLocation.BoolLiteral:
                        return expression.GetLocation();

                    case ErrorLocation.BoolLiteralAndOperator:
                        return CalculateExtendedLocation(binary, isLeftSide);

                    case ErrorLocation.NonBoolLiteralExpression:
                        return CalculateExtendedLocation(binary, !isLeftSide);

                    default:
                        return null;
                }
            }
        }

        private Location CalculateExtendedLocation(TBinaryExpression binary, bool isLeftSide) =>
            isLeftSide
                ? binary.CreateLocation(GetOperatorToken(binary))
                : GetOperatorToken(binary).CreateLocation(binary);
    }
}
