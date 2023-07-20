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
    public abstract class RedundantNullCheckBase<TBinaryExpression> : SonarDiagnosticAnalyzer
        where TBinaryExpression : SyntaxNode
    {
        internal const string DiagnosticId = "S4201";

        protected abstract SyntaxNode GetLeftNode(TBinaryExpression binaryExpression);

        protected abstract SyntaxNode GetRightNode(TBinaryExpression binaryExpression);

        protected abstract SyntaxNode GetNullCheckVariable(SyntaxNode node);

        protected abstract SyntaxNode GetNonNullCheckVariable(SyntaxNode node);

        protected abstract SyntaxNode GetIsOperatorCheckVariable(SyntaxNode node);

        protected abstract SyntaxNode GetInvertedIsOperatorCheckVariable(SyntaxNode node);

        protected abstract bool AreEquivalent(SyntaxNode node1, SyntaxNode node2);

        // LogicalAnd (C#) / AndAlso (VB)
        protected void CheckAndExpression(SonarSyntaxNodeReportingContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            var binaryExpressionLeft = GetLeftNode(binaryExpression);
            var binaryExpressionRight = GetRightNode(binaryExpression);

            if (GetNonNullCheckVariable(binaryExpressionLeft) is SyntaxNode nonNullCheckVariable1
                && GetIsOperatorCheckVariable(binaryExpressionRight) is  SyntaxNode isCheckVariable1
                && AreEquivalent(nonNullCheckVariable1, isCheckVariable1))
            {
                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], binaryExpressionLeft.GetLocation()));
            }
            if (GetNonNullCheckVariable(binaryExpressionRight) is SyntaxNode nonNullCheckVariable2
                && GetIsOperatorCheckVariable(binaryExpressionLeft) is SyntaxNode isCheckVariable2
                && AreEquivalent(nonNullCheckVariable2, isCheckVariable2))
            {
                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], binaryExpressionRight.GetLocation()));
            }
        }

        // LogicalOr (C#) / OrElse (VB)
        protected void CheckOrExpression(SonarSyntaxNodeReportingContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            var binaryExpressionLeft = GetLeftNode(binaryExpression);
            var binaryExpressionRight = GetRightNode(binaryExpression);

            if (GetNullCheckVariable(binaryExpressionLeft) is SyntaxNode nullCheckVariable1
                && GetInvertedIsOperatorCheckVariable(binaryExpressionRight) is SyntaxNode invertedIsCheckVariable1
                && AreEquivalent(nullCheckVariable1, invertedIsCheckVariable1))
            {
                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], binaryExpressionLeft.GetLocation()));
            }
            if (GetNullCheckVariable(binaryExpressionRight) is SyntaxNode nullCheckVariable2
                && GetInvertedIsOperatorCheckVariable(binaryExpressionLeft) is SyntaxNode invertedIsCheckVariable2
                && AreEquivalent(nullCheckVariable2, invertedIsCheckVariable2))
            {
                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], binaryExpressionRight.GetLocation()));
            }
        }
    }
}
