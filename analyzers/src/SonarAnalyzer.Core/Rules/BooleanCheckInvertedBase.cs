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

namespace SonarAnalyzer.Rules
{
    public abstract class BooleanCheckInvertedBase<TBinaryExpression> : SonarDiagnosticAnalyzer
        where TBinaryExpression : SyntaxNode
    {
        internal const string DiagnosticId = "S1940";
        protected const string MessageFormat = "Use the opposite operator ('{0}') instead.";

        protected abstract bool IsLogicalNot(TBinaryExpression expression, out SyntaxNode logicalNot);

        protected abstract string GetSuggestedReplacement(TBinaryExpression expression);

        protected abstract bool IsIgnoredNullableOperation(TBinaryExpression expression, SemanticModel semanticModel);

        protected Action<SonarSyntaxNodeReportingContext> GetAnalysisAction(DiagnosticDescriptor rule)
        {
            return c =>
            {
                var expression = (TBinaryExpression)c.Node;

                if (IsUserDefinedOperator(expression, c.SemanticModel) ||
                    IsIgnoredNullableOperation(expression, c.SemanticModel))
                {
                    return;
                }

                if (IsLogicalNot(expression, out var logicalNot))
                {
                    c.ReportIssue(rule, logicalNot, GetSuggestedReplacement(expression));
                }
            };
        }

        private static bool IsUserDefinedOperator(TBinaryExpression expression, SemanticModel semanticModel) =>
            semanticModel.GetEnclosingSymbol(expression.SpanStart) is IMethodSymbol enclosingSymbol &&
            enclosingSymbol?.MethodKind == MethodKind.UserDefinedOperator;

        protected static bool IsNullable(SyntaxNode expression, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(expression).Symbol.GetSymbolType() is INamedTypeSymbol symbolType &&
            symbolType.ConstructedFrom.Is(KnownType.System_Nullable_T);
    }
}
