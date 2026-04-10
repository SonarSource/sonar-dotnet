/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules;

public abstract class BooleanCheckInvertedBase<TBinaryExpression> : SonarDiagnosticAnalyzer where TBinaryExpression : SyntaxNode
{
    internal const string DiagnosticId = "S1940";
    protected const string MessageFormat = "Use the opposite operator ('{0}') instead.";

    protected abstract SyntaxNode LogicalNotNode(TBinaryExpression expression);

    protected abstract string SuggestedReplacement(TBinaryExpression expression);

    protected abstract bool IsUnsafeInversionOperation(TBinaryExpression expression, SemanticModel model);

    protected Action<SonarSyntaxNodeReportingContext> AnalysisAction(DiagnosticDescriptor rule) =>
        c =>
        {
            var expression = (TBinaryExpression)c.Node;

            if (IsUserDefinedOperator(expression, c.Model) || IsUnsafeInversionOperation(expression, c.Model))
            {
                return;
            }

            if (LogicalNotNode(expression) is { } logicalNot)
            {
                c.ReportIssue(rule, logicalNot, SuggestedReplacement(expression));
            }
        };

    protected static bool IsNullable(SyntaxNode expression, SemanticModel model) =>
        model.GetSymbolInfo(expression).Symbol.GetSymbolType() is INamedTypeSymbol symbolType
        && symbolType.ConstructedFrom.Is(KnownType.System_Nullable_T);

    protected static bool IsFloatingPoint(SyntaxNode expression, SemanticModel model) =>
        model.GetTypeInfo(expression).Type.IsAny(KnownType.FloatingPointNumbers);

    private static bool IsUserDefinedOperator(TBinaryExpression expression, SemanticModel model) =>
        model.GetEnclosingSymbol(expression.SpanStart) is IMethodSymbol enclosingSymbol
        && enclosingSymbol.MethodKind == MethodKind.UserDefinedOperator;
}
