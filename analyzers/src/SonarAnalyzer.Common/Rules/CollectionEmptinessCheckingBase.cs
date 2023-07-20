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
    public abstract class CollectionEmptinessCheckingBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S1155";

        protected CollectionEmptinessCheckingBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var binaryLeft = Language.Syntax.BinaryExpressionLeft(c.Node);
                    var binaryRight = Language.Syntax.BinaryExpressionRight(c.Node);

                    if (Language.ExpressionNumericConverter.TryGetConstantIntValue(binaryLeft, out var left))
                    {
                        CheckExpression(c, binaryRight, left, Language.Syntax.ComparisonKind(c.Node).Mirror());
                    }
                    else if (Language.ExpressionNumericConverter.TryGetConstantIntValue(binaryRight, out var right))
                    {
                        CheckExpression(c, binaryLeft, right, Language.Syntax.ComparisonKind(c.Node));
                    }
                },
                Language.SyntaxKind.ComparisonKinds);

        private void CheckExpression(SonarSyntaxNodeReportingContext context, SyntaxNode expression, int constant, ComparisonKind comparison)
        {
            if (comparison.Compare(constant).IsEmptyOrNotEmpty()
                && TryGetCountCall(expression, context.SemanticModel, out var location, out var typeArgument))
            {
                context.ReportIssue(CreateDiagnostic(Rule, location, typeArgument));
            }
        }

        private bool TryGetCountCall(SyntaxNode expression, SemanticModel semanticModel, out Location countLocation, out string typeArgument)
        {
            if (Language.Syntax.NodeIdentifier(expression) is { } identifier
                && identifier.ValueText == nameof(Enumerable.Count)
                && semanticModel.GetSymbolInfo(identifier.Parent.Parent).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
                && methodSymbol.ReceiverType is INamedTypeSymbol receiverType)
            {
                countLocation = identifier.Parent.GetLocation();
                typeArgument = (methodSymbol.TypeArguments.Any()
                    ? methodSymbol.TypeArguments
                    : receiverType.TypeArguments).Single().ToDisplayString();
                return true;
            }
            else
            {
                countLocation = null;
                typeArgument = null;
                return false;
            }
        }
    }
}
