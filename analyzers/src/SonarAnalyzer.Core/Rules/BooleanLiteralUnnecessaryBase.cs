/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules
{
    public abstract class BooleanLiteralUnnecessaryBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S1125";

        private ImmutableArray<INamedTypeSymbol> systemBooleanInterfaces;

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

        protected abstract SyntaxNode GetLeftNode(SyntaxNode node);
        protected abstract SyntaxNode GetRightNode(SyntaxNode node);
        protected abstract SyntaxToken? GetOperatorToken(SyntaxNode node);
        protected abstract bool IsTrue(SyntaxNode syntaxNode);
        protected abstract bool IsFalse(SyntaxNode syntaxNode);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(x =>
            {
                systemBooleanInterfaces = x.Compilation.GetTypeByMetadataName(KnownType.System_Boolean).Interfaces;
            });

        // For C# 7 syntax
        protected virtual bool IsInsideTernaryWithThrowExpression(SyntaxNode syntaxNode) => false;

        protected override string MessageFormat => "Remove the unnecessary Boolean literal(s).";

        protected BooleanLiteralUnnecessaryBase() : base(DiagnosticId) { }

        // LogicalAnd (C#) / AndAlso (VB)
        protected void CheckAndExpression(SonarSyntaxNodeReportingContext context)
        {
            if (IsInsideTernaryWithThrowExpression(context.Node) || CheckForNullabilityAndBooleanConstantsReport(context, context.Node, reportOnTrue: true))
            {
                return;
            }

            // When we have 'EXPR And True', the true literal is the redundant part
            CheckForBooleanConstantOnLeft(context, context.Node, IsTrue, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnRight(context, context.Node, IsTrue, ErrorLocation.BoolLiteralAndOperator);

            // 'EXPR And False' is always False, thus EXPR is the redundant part
            CheckForBooleanConstantOnLeft(context, context.Node, IsFalse, ErrorLocation.NonBoolLiteralExpression);
            CheckForBooleanConstantOnRight(context, context.Node, IsFalse, ErrorLocation.NonBoolLiteralExpression);
        }

        // LogicalOr (C#) / OrElse (VB)
        protected void CheckOrExpression(SonarSyntaxNodeReportingContext context)
        {
            if (IsInsideTernaryWithThrowExpression(context.Node) || CheckForNullabilityAndBooleanConstantsReport(context, context.Node, reportOnTrue: false))
            {
                return;
            }

            // When we have 'EXPR Or False', the false literal is the redundant part
            CheckForBooleanConstantOnLeft(context, context.Node, IsFalse, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnRight(context, context.Node, IsFalse, ErrorLocation.BoolLiteralAndOperator);

            // 'EXPR Or True' is always True, thus EXPR is the redundant part
            CheckForBooleanConstantOnLeft(context, context.Node, IsTrue, ErrorLocation.NonBoolLiteralExpression);
            CheckForBooleanConstantOnRight(context, context.Node, IsTrue, ErrorLocation.NonBoolLiteralExpression);
        }

        protected void CheckEquals(SonarSyntaxNodeReportingContext context)
        {
            if (IsInsideTernaryWithThrowExpression(context.Node) || CheckForNullabilityAndBooleanConstantsReport(context, context.Node, reportOnTrue: true))
            {
                return;
            }

            CheckForBooleanConstantOnLeft(context, context.Node, IsTrue, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnLeft(context, context.Node, IsFalse, ErrorLocation.BoolLiteralAndOperator);

            CheckForBooleanConstantOnRight(context, context.Node, IsTrue, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnRight(context, context.Node, IsFalse, ErrorLocation.BoolLiteralAndOperator);
        }

        protected void CheckNotEquals(SonarSyntaxNodeReportingContext context)
        {
            if (IsInsideTernaryWithThrowExpression(context.Node) || CheckForNullabilityAndBooleanConstantsReport(context, context.Node, reportOnTrue: false))
            {
                return;
            }

            CheckForBooleanConstantOnLeft(context, context.Node, IsFalse, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnLeft(context, context.Node, IsTrue, ErrorLocation.BoolLiteral);

            CheckForBooleanConstantOnRight(context, context.Node, IsFalse, ErrorLocation.BoolLiteralAndOperator);
            CheckForBooleanConstantOnRight(context, context.Node, IsTrue, ErrorLocation.BoolLiteral);
        }

        protected void CheckTernaryExpressionBranches(SonarSyntaxNodeReportingContext context, SyntaxNode thenBranch, SyntaxNode elseBranch)
        {
            var thenNoParantheses = Language.Syntax.RemoveParentheses(thenBranch);
            var elseNoParantheses = Language.Syntax.RemoveParentheses(elseBranch);

            var thenIsBooleanLiteral = IsTrue(thenNoParantheses) || IsFalse(thenNoParantheses);
            var elseIsBooleanLiteral = IsTrue(elseNoParantheses) || IsFalse(elseNoParantheses);

            var bothSideBool = thenIsBooleanLiteral && elseIsBooleanLiteral;
            var bothSideTrue = IsTrue(thenNoParantheses) && IsTrue(elseNoParantheses);
            var bothSideFalse = IsFalse(thenNoParantheses) && IsFalse(elseNoParantheses);

            if (bothSideBool && !bothSideFalse && !bothSideTrue)
            {
                context.ReportIssue(SupportedDiagnostics[0], thenBranch.CreateLocation(elseBranch));
            }
            if (thenIsBooleanLiteral ^ elseIsBooleanLiteral)
            {
                // one side is boolean literal, the other is NOT boolean literal
                var booleanLiteralSide = thenIsBooleanLiteral ? thenBranch : elseBranch;

                context.ReportIssue(SupportedDiagnostics[0], booleanLiteralSide);
            }
        }

        protected bool CheckForNullabilityAndBooleanConstantsReport(SonarSyntaxNodeReportingContext context, SyntaxNode node, bool reportOnTrue)
        {
            var left = Language.Syntax.RemoveParentheses(GetLeftNode(node));
            var right = Language.Syntax.RemoveParentheses(GetRightNode(node));

            if (right is null // Avoids DeclarationPattern or RecursivePattern
                || TypeShouldBeIgnored(left, context.Model)
                || TypeShouldBeIgnored(right, context.Model))
            {
                return true;
            }

            var leftIsTrue = IsTrue(left);
            var leftIsFalse = IsFalse(left);
            var rightIsTrue = IsTrue(right);
            var rightIsFalse = IsFalse(right);

            if ((leftIsTrue || leftIsFalse) && (rightIsTrue || rightIsFalse))
            {
                var bothAreSame = (leftIsTrue && rightIsTrue) || (leftIsFalse && rightIsFalse);
                var errorLocation = bothAreSame
                    ? CalculateExtendedLocation(node, false)
                    : CalculateExtendedLocation(node, reportOnTrue == leftIsTrue);

                context.ReportIssue(SupportedDiagnostics[0], errorLocation);
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
            if (isBooleanLiteralKind(node) && GetLocation() is { } location)
            {
                context.ReportIssue(SupportedDiagnostics[0], location);
            }

            Location GetLocation() =>
                errorLocation switch
                {
                    ErrorLocation.BoolLiteral => node.GetLocation(),
                    ErrorLocation.BoolLiteralAndOperator => CalculateExtendedLocation(node.Parent, isLeftSide),
                    ErrorLocation.NonBoolLiteralExpression => CalculateExtendedLocation(node.Parent, !isLeftSide),
                    _ => null,
                };
        }

        private void CheckForBooleanConstantOnLeft(SonarSyntaxNodeReportingContext context, SyntaxNode node, IsBooleanLiteralKind isBooleanLiteralKind, ErrorLocation errorLocation) =>
            CheckForBooleanConstant(context, GetLeftNode(node), isBooleanLiteralKind, errorLocation, isLeftSide: true);

        private void CheckForBooleanConstantOnRight(SonarSyntaxNodeReportingContext context, SyntaxNode node, IsBooleanLiteralKind isBooleanLiteralKind, ErrorLocation errorLocation) =>
            CheckForBooleanConstant(context, GetRightNode(node), isBooleanLiteralKind, errorLocation, isLeftSide: false);

        private Location CalculateExtendedLocation(SyntaxNode parent, bool isLeftSide)
        {
            if (GetOperatorToken(parent) is not { } token)
            {
                return null;
            }

            return isLeftSide ? parent.CreateLocation(token) : token.CreateLocation(parent);
        }

        private bool TypeShouldBeIgnored(SyntaxNode node, SemanticModel model)
        {
            var type = model.GetTypeInfo(node).Type;
            return type.IsNullableBoolean()
                || type is ITypeParameterSymbol
                || type.Is(KnownType.System_Object)
                || systemBooleanInterfaces.Any(x => x.Equals(type));
        }
    }
}
