/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

                    if (Language.ExpressionNumericConverter.ConstantIntValue(binaryLeft) is { } left)
                    {
                        CheckExpression(c, binaryRight, left, Language.Syntax.ComparisonKind(c.Node).Mirror());
                    }
                    else if (Language.ExpressionNumericConverter.ConstantIntValue(binaryRight) is { } right)
                    {
                        CheckExpression(c, binaryLeft, right, Language.Syntax.ComparisonKind(c.Node));
                    }
                },
                Language.SyntaxKind.ComparisonKinds);

        private void CheckExpression(SonarSyntaxNodeReportingContext context, SyntaxNode expression, int constant, ComparisonKind comparison)
        {
            if (comparison.Compare(constant).IsEmptyOrNotEmpty()
                && TryGetCountCall(expression, context.Model, out var location, out var typeArgument))
            {
                context.ReportIssue(Rule, location, typeArgument);
            }
        }

        private bool TryGetCountCall(SyntaxNode expression, SemanticModel model, out Location countLocation, out string typeArgument)
        {
            if (Language.Syntax.NodeIdentifier(expression) is { } identifier
                && identifier.ValueText == nameof(Enumerable.Count)
                && model.GetSymbolInfo(identifier.Parent.Parent).Symbol is IMethodSymbol methodSymbol
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
