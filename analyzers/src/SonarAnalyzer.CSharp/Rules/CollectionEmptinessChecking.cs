/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CollectionEmptinessChecking : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1155";
        private const string MessageFormat = "Use '.Any()' to test whether this 'IEnumerable<{0}>' is empty or not.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly CSharpExpressionNumericConverter ExpressionNumericConverter = new CSharpExpressionNumericConverter();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;

                    if (ExpressionNumericConverter.TryGetConstantIntValue(binary.Left, out var left))
                    {
                        CheckExpression(c, binary.Right, left, CSharpFacade.Instance.Syntax.ComparisonKind(binary).Mirror());
                    }
                    else if (ExpressionNumericConverter.TryGetConstantIntValue(binary.Right, out var right))
                    {
                        CheckExpression(c, binary.Left, right, CSharpFacade.Instance.Syntax.ComparisonKind(binary));
                    }
                },
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);

        private static void CheckExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, int constant, ComparisonKind comparison)
        {
            if (comparison.Compare(constant).IsEmptyOrNotEmpty()
                && TryGetCountCall(expression, context.SemanticModel, out var location, out var typeArgument))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, location, typeArgument));
            }
        }

        private static bool TryGetCountCall(ExpressionSyntax expression, SemanticModel semanticModel, out Location countLocation, out string typeArgument)
        {
            countLocation = null;
            typeArgument = null;
            var invocation = expression as InvocationExpressionSyntax;

            if (invocation?.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.ValueText == nameof(Enumerable.Count)
                && (semanticModel.GetSymbolInfo(memberAccess).Symbol is IMethodSymbol methodSymbol)
                && IsMethodCountExtension(methodSymbol)
                && methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                if (methodSymbol.IsGenericMethod)
                {
                    typeArgument = methodSymbol.TypeArguments.Single().ToDisplayString();
                }

                countLocation = memberAccess.Name.GetLocation();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsMethodCountExtension(IMethodSymbol methodSymbol) =>
            methodSymbol.Name == nameof(Enumerable.Count)
            && methodSymbol.IsExtensionMethod
            && methodSymbol.ReceiverType != null;
    }
}
