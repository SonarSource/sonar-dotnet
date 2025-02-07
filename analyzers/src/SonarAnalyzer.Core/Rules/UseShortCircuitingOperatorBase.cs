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
    public abstract class UseShortCircuitingOperatorBase<TSyntaxKind, TBinaryExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TBinaryExpression : SyntaxNode
    {
        internal const string DiagnosticId = "S2178";

        protected abstract string GetSuggestedOpName(TBinaryExpression node);
        protected abstract string GetCurrentOpName(TBinaryExpression node);
        protected abstract SyntaxToken GetOperator(TBinaryExpression expression);
        protected abstract ImmutableArray<TSyntaxKind> SyntaxKindsOfInterest { get; }

        protected override string MessageFormat => "Correct this '{0}' to '{1}'{2}.";

        protected UseShortCircuitingOperatorBase() : base(DiagnosticId) { }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    if (c.Node is TBinaryExpression node
                        && Language.Syntax.BinaryExpressionLeft(node) is { } left
                        && Language.Syntax.BinaryExpressionRight(node) is { } right
                        && IsBool(left, c.Model)
                        && IsBool(right, c.Model))
                    {
                        var extractText = c.Model.GetConstantValue(right) is { HasValue: true }
                            || c.Model.GetSymbolInfo(right).Symbol is ILocalSymbol or IFieldSymbol or IPropertySymbol or IParameterSymbol
                                ? string.Empty
                                : " and extract the right operand to a variable if it should always be evaluated";
                        c.ReportIssue(SupportedDiagnostics[0], GetOperator(node), GetCurrentOpName(node), GetSuggestedOpName(node), extractText);
                    }
                },
                SyntaxKindsOfInterest.ToArray());

        private static bool IsBool(SyntaxNode node, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(node).Type.Is(KnownType.System_Boolean);
    }
}
