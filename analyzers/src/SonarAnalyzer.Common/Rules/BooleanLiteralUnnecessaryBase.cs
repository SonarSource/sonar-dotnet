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
        protected abstract SyntaxToken GetOperatorToken(SyntaxNode node);
        protected abstract bool IsTrueLiteralKind(SyntaxNode syntaxNode);
        protected abstract bool IsFalseLiteralKind(SyntaxNode syntaxNode);
        // For C# 7 syntax
        protected virtual bool IsInsideTernaryWithThrowExpression(SyntaxNode syntaxNode) => false;

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
            if (IsInsideTernaryWithThrowExpression(binary) || CheckForNullabilityAndBooleanConstantsReport(context, binary, reportOnTrue: true))
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
                context.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], thenBranch.CreateLocation(elseBranch)));
            }
            if (thenIsBooleanLiteral ^ elseIsBooleanLiteral)
            {
                // one side is boolean literal, the other is NOT boolean literal
                var booleanLiteralSide = thenIsBooleanLiteral ? thenBranch : elseBranch;

                context.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], booleanLiteralSide.GetLocation()));
            }
        }

        protected bool CheckForNullabilityAndBooleanConstantsReport(SonarSyntaxNodeReportingContext context, SyntaxNode left, SyntaxNode right, bool reportOnTrue)
        {
            var typeLeft = context.SemanticModel.GetTypeInfo(left).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(right).Type;
            if (typeLeft.IsNullableBoolean() || typeRight.IsNullableBoolean())
            {
                return true;
            }

            var leftIsTrue = IsTrueLiteralKind(left);
            var leftIsFalse = IsFalseLiteralKind(left);
            var rightIsTrue = IsTrueLiteralKind(right);
            var rightIsFalse = IsFalseLiteralKind(right);

            var leftIsBoolean = leftIsTrue || leftIsFalse;
            var rightIsBoolean = rightIsTrue || rightIsFalse;

            if (leftIsBoolean && rightIsBoolean)
            {
                var bothAreSame = (leftIsTrue && rightIsTrue) || (leftIsFalse && rightIsFalse);
                var errorLocation = bothAreSame
                    ? CalculateExtendedLocation(left.Parent, false)
                    : CalculateExtendedLocation(left.Parent, reportOnTrue == leftIsTrue);

                context.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], errorLocation));
                return true;
            }
            return false;
        }

        protected void CheckForBooleanConstant(SonarSyntaxNodeReportingContext context,
                                             SyntaxNode node,
                                             IsBooleanLiteralKind isBooleanLiteralKind,
                                             ErrorLocation errorLocation,
                                             bool isLeftSide)
        {
            if (!isBooleanLiteralKind(node))
            {
                return;
            }

            context.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], GetLocation()));

            Location GetLocation() =>
                errorLocation switch
                {
                    ErrorLocation.BoolLiteral => node.GetLocation(),
                    ErrorLocation.BoolLiteralAndOperator => CalculateExtendedLocation(node.Parent, isLeftSide),
                    ErrorLocation.NonBoolLiteralExpression => CalculateExtendedLocation(node.Parent, !isLeftSide),
                    _ => null,
                };
        }

        private bool CheckForNullabilityAndBooleanConstantsReport(SonarSyntaxNodeReportingContext context, TBinaryExpression binary, bool reportOnTrue) =>
            CheckForNullabilityAndBooleanConstantsReport(context,
                                                         Language.Syntax.RemoveParentheses(Language.Syntax.BinaryExpressionLeft(binary)),
                                                         Language.Syntax.RemoveParentheses(Language.Syntax.BinaryExpressionRight(binary)),
                                                         reportOnTrue);

        private void CheckForBooleanConstantOnLeft(SonarSyntaxNodeReportingContext context, TBinaryExpression binary, IsBooleanLiteralKind isBooleanLiteralKind, ErrorLocation errorLocation) =>
            CheckForBooleanConstant(context, Language.Syntax.RemoveParentheses(Language.Syntax.BinaryExpressionLeft(binary)), isBooleanLiteralKind, errorLocation, isLeftSide: true);

        private void CheckForBooleanConstantOnRight(SonarSyntaxNodeReportingContext context, TBinaryExpression binary, IsBooleanLiteralKind isBooleanLiteralKind, ErrorLocation errorLocation) =>
            CheckForBooleanConstant(context, Language.Syntax.RemoveParentheses(Language.Syntax.BinaryExpressionRight(binary)), isBooleanLiteralKind, errorLocation, isLeftSide: false);

        private Location CalculateExtendedLocation(SyntaxNode parent, bool isLeftSide) =>
            isLeftSide
                ? parent.CreateLocation(GetOperatorToken(parent))
                : GetOperatorToken(parent).CreateLocation(parent);
    }
}
