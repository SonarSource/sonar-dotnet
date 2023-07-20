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
    public abstract class IndexOfCheckAgainstZeroBase<TSyntaxKind, TBinaryExpressionSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TBinaryExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2692";

        private static readonly string[] TrackedMethods =
            {
                "IndexOf",
                "IndexOfAny",
                "LastIndexOf",
                "LastIndexOfAny"
            };

        private static readonly ImmutableArray<KnownType> CheckedTypes =
            ImmutableArray.Create(
                KnownType.System_Array,
                KnownType.System_Collections_Generic_IList_T,
                KnownType.System_String,
                KnownType.System_Collections_IList);

        protected abstract TSyntaxKind LessThanExpression { get; }
        protected abstract TSyntaxKind GreaterThanExpression { get; }

        protected abstract SyntaxNode Left(TBinaryExpressionSyntax binaryExpression);
        protected abstract SyntaxNode Right(TBinaryExpressionSyntax binaryExpression);
        protected abstract SyntaxToken OperatorToken(TBinaryExpressionSyntax binaryExpression);

        protected override string MessageFormat => "0 is a valid index, but this check ignores it.";

        protected IndexOfCheckAgainstZeroBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    var lessThan = (TBinaryExpressionSyntax)c.Node;
                    if (IsInvalidComparison(Left(lessThan), Right(lessThan), c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, Left(lessThan).CreateLocation(OperatorToken(lessThan))));
                    }
                },
                LessThanExpression);

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    var greaterThan = (TBinaryExpressionSyntax)c.Node;
                    if (IsInvalidComparison(Right(greaterThan), Left(greaterThan), c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, OperatorToken(greaterThan).CreateLocation(Right(greaterThan))));
                    }
                },
                GreaterThanExpression);
        }

        private bool IsInvalidComparison(SyntaxNode constantExpression, SyntaxNode methodInvocationExpression, SemanticModel semanticModel) =>
            Language.ExpressionNumericConverter.TryGetConstantIntValue(constantExpression, out var constValue)
            && constValue == 0
            && semanticModel.GetSymbolInfo(methodInvocationExpression).Symbol is IMethodSymbol indexOfSymbol
            && TrackedMethods.Any(x => x.Equals(indexOfSymbol.Name, Language.NameComparison))
            && indexOfSymbol.ContainingType.DerivesOrImplementsAny(CheckedTypes);
    }
}
