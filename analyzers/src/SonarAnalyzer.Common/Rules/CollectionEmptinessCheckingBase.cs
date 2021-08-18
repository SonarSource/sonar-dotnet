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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class CollectionEmptinessCheckingBase<TExpression, TMemberAccess, TSyntaxKind>
        : SonarDiagnosticAnalyzer
        where TExpression : SyntaxNode
        where TMemberAccess : SyntaxNode
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S1155";
        private const string MessageFormat = "Use '.Any()' to test whether this 'IEnumerable<{0}>' is empty or not.";

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected CollectionEmptinessCheckingBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var binary = c.Node;
                    var binaryLeft = Left(binary);
                    var binaryRight = Right(binary);

                    if (Language.ExpressionNumericConverter.TryGetConstantIntValue(binaryLeft, out var left))
                    {
                        CheckExpression(c, binaryRight, left, Language.Syntax.ComparisonKind(binary).Mirror());
                    }
                    else if (Language.ExpressionNumericConverter.TryGetConstantIntValue(binaryRight, out var right))
                    {
                        CheckExpression(c, binaryLeft, right, Language.Syntax.ComparisonKind(binary));
                    }
                },
                Language.SyntaxKind.ComparisonKinds);

        protected abstract TExpression Left(SyntaxNode binary);
        protected abstract TExpression Right(SyntaxNode binary);
        protected abstract TMemberAccess GetMemberAccess(TExpression expression);
        protected abstract string GetIdentifierName(TMemberAccess memberAccess);
        protected abstract Location GetLocation(TMemberAccess memberAccess);

        private void CheckExpression(SyntaxNodeAnalysisContext context, TExpression expression, int constant, ComparisonKind comparison)
        {
            if (comparison.Compare(constant).IsEmptyOrNotEmpty()
                && TryGetCountCall(expression, context.SemanticModel, out var location, out var typeArgument))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, typeArgument));
            }
        }

        private bool TryGetCountCall(TExpression expression, SemanticModel semanticModel, out Location countLocation, out string typeArgument)
        {
            countLocation = null;
            typeArgument = null;

            if (GetMemberAccess(expression) is TMemberAccess memberAccess
                && GetIdentifierName(memberAccess) == nameof(Enumerable.Count)
                && (semanticModel.GetSymbolInfo(memberAccess).Symbol is IMethodSymbol methodSymbol)
                && IsMethodCountExtension(methodSymbol)
                && methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T))
            {
                if (methodSymbol.IsGenericMethod)
                {
                    typeArgument = methodSymbol.TypeArguments.Single().ToDisplayString();
                }

                countLocation = GetLocation(memberAccess);
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
