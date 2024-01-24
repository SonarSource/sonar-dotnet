/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
    public abstract class UseShortCircuitingOperatorBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind> where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S2178";
        protected override string MessageFormat => "Correct this '{0}' to '{1}'{2}.";
        protected UseShortCircuitingOperatorBase() : base(DiagnosticId) { }

        protected static bool IsBool(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node == null)
            {
                return false;
            }

            var type = semanticModel.GetTypeInfo(node).Type;
            return type.Is(KnownType.System_Boolean);
        }
    }

    public abstract class UseShortCircuitingOperatorBase<TSyntaxKind, TBinaryExpression> : UseShortCircuitingOperatorBase<TSyntaxKind>
        where TSyntaxKind : struct
        where TBinaryExpression : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    var node = (TBinaryExpression)c.Node;

                    if (GetOperands(node) is ({ } left, { } right) && IsBool(left, c.SemanticModel) && IsBool(right, c.SemanticModel))
                    {
                        var extractText = c.SemanticModel.GetConstantValue(right) is { HasValue: true }
                            || c.SemanticModel.GetSymbolInfo(right).Symbol is ILocalSymbol or IFieldSymbol or IPropertySymbol or IParameterSymbol
                                ? string.Empty
                                : " and extract the right operand to a variable if it should always be evaluated";
                        c.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], GetOperator(node).GetLocation(),
                            GetCurrentOpName(node), GetSuggestedOpName(node), extractText));
                    }
                },
                SyntaxKindsOfInterest.ToArray());

        protected abstract string GetSuggestedOpName(TBinaryExpression node);

        protected abstract string GetCurrentOpName(TBinaryExpression node);

        protected abstract Operands GetOperands(TBinaryExpression expression);

        protected abstract SyntaxToken GetOperator(TBinaryExpression expression);

        protected abstract ImmutableArray<TSyntaxKind> SyntaxKindsOfInterest { get; }

        protected readonly record struct Operands(SyntaxNode Left, SyntaxNode Right);
    }
}
