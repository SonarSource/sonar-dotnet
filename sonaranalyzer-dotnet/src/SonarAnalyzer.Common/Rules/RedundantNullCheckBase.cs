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
        protected void CheckAndExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            var binaryExpressionLeft = GetLeftNode(binaryExpression);
            var binaryExpressionRight = GetRightNode(binaryExpression);

            if (GetNonNullCheckVariable(binaryExpressionLeft) is SyntaxNode node1
                && GetIsOperatorCheckVariable(binaryExpressionRight) is  SyntaxNode node2
                && AreEquivalent(node1, node2))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], binaryExpressionLeft.GetLocation()));
            }
            if (GetNonNullCheckVariable(binaryExpressionRight) is SyntaxNode node3
                && GetIsOperatorCheckVariable(binaryExpressionLeft) is SyntaxNode node4
                && AreEquivalent(node3, node4))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], binaryExpressionRight.GetLocation()));
            }
        }

        // LogicalOr (C#) / OrElse (VB)
        protected void CheckOrExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (TBinaryExpression)context.Node;
            var binaryExpressionLeft = GetLeftNode(binaryExpression);
            var binaryExpressionRight = GetRightNode(binaryExpression);

            if (GetNullCheckVariable(binaryExpressionLeft) is SyntaxNode node1
                && GetInvertedIsOperatorCheckVariable(binaryExpressionRight) is SyntaxNode node2
                && AreEquivalent(node1, node2))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], binaryExpressionLeft.GetLocation()));
            }
            if (GetNullCheckVariable(binaryExpressionRight) is SyntaxNode node3
                && GetInvertedIsOperatorCheckVariable(binaryExpressionLeft) is SyntaxNode node4
                && AreEquivalent(node3, node4))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], binaryExpressionRight.GetLocation()));
            }
        }
    }
}
