/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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
    public abstract class ReversedOperatorsBase<TUnaryExpressionSyntax> : SonarDiagnosticAnalyzer
        where TUnaryExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2757";
        protected const string MessageFormat = "Was '{0}' meant instead?";

        protected abstract SyntaxToken GetOperatorToken(TUnaryExpressionSyntax e);

        protected abstract bool IsEqualsToken(SyntaxToken token);

        protected abstract bool IsMinusToken(SyntaxToken token);

        private static bool TiedTogether(FileLinePositionSpan left, FileLinePositionSpan right) =>
            left.EndLinePosition == right.StartLinePosition;

        protected Action<SonarSyntaxNodeReportingContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
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
                    c.ReportIssue(rule, operatorLocation, $"{operatorToken.Text}=");
                }
            };
    }
}
