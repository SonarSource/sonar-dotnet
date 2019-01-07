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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ReversedOperatorsBase<TUnaryExpressionSyntax> : SonarDiagnosticAnalyzer
        where TUnaryExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2757";
        protected const string MessageFormat = "Was '{0}' meant instead?";

        protected abstract SyntaxToken GetOperatorToken(TUnaryExpressionSyntax e);

        protected abstract bool IsEqualsToken(SyntaxToken token);

        protected abstract bool IsMinusToken(SyntaxToken token);

        protected static bool TiedTogether(FileLinePositionSpan left, FileLinePositionSpan right)
        {
            return left.EndLinePosition == right.StartLinePosition;
        }

        public Action<SyntaxNodeAnalysisContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
            c =>
            {
                var unaryExpression = (TUnaryExpressionSyntax)c.Node;

                var operatorToken = GetOperatorToken(unaryExpression);
                var equalsToken = operatorToken.GetPreviousToken();
                if (!IsEqualsToken(equalsToken))
                {
                    return;
                }

                var operandToken = operatorToken.GetNextToken();
                var operatorLocation = operatorToken.GetLocation();

                var operatorSpan = operatorLocation.GetLineSpan();
                var equalsSignSpan = equalsToken.GetLocation().GetLineSpan();
                var operandSpan = operandToken.GetLocation().GetLineSpan();

                if (TiedTogether(equalsSignSpan, operatorSpan) &&
                    !(IsMinusToken(operatorToken) && TiedTogether(operatorSpan, operandSpan)))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, operatorLocation, $"{operatorToken.Text}="));
                }
            };
    }
}
