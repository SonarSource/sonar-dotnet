/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
    public sealed class ReturnValueIgnored : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2201";
        private const string MessageFormat = "Use the return value of method '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var expressionStatement = (ExpressionStatementSyntax)c.Node;
                    CheckExpressionForPureMethod(c, expressionStatement.Expression);
                },
                SyntaxKind.ExpressionStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var lambda = (LambdaExpressionSyntax)c.Node;

                    if (!(c.SemanticModel.GetSymbolInfo(lambda).Symbol is IMethodSymbol symbol) ||
                        !symbol.ReturnsVoid)
                    {
                        return;
                    }

                    var expression = lambda.Body as ExpressionSyntax;
                    CheckExpressionForPureMethod(c, expression);
                },
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.SimpleLambdaExpression);
        }

        private static void CheckExpressionForPureMethod(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            if (!(expression is InvocationExpressionSyntax invocation))
            {
                return;
            }

            if (!(context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethodSymbol) ||
                invokedMethodSymbol.ReturnsVoid)
            {
                return;
            }

            if (invokedMethodSymbol.Parameters.All(p => p.RefKind == RefKind.None) &&
                IsSideEffectFreeOrPure(invokedMethodSymbol))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, expression.GetLocation(), invokedMethodSymbol.Name));
            }
        }

        private static bool IsSideEffectFreeOrPure(IMethodSymbol invokedMethodSymbol)
        {
            var constructedFrom = invokedMethodSymbol.ContainingType.ConstructedFrom;

            return IsLinqMethod(invokedMethodSymbol) ||
                HasOnlySideEffectFreeMethods(constructedFrom) ||
                IsPureMethod(invokedMethodSymbol, constructedFrom);
        }

        private static bool IsPureMethod(IMethodSymbol invokedMethodSymbol, INamedTypeSymbol containingType) =>
            invokedMethodSymbol.HasAttribute(KnownType.System_Diagnostics_Contracts_PureAttribute) ||
            containingType.HasAttribute(KnownType.System_Diagnostics_Contracts_PureAttribute);

        private static bool HasOnlySideEffectFreeMethods(INamedTypeSymbol containingType)
        {
            return containingType.IsAny(ImmutableKnownTypes);
        }

        private static readonly ImmutableArray<KnownType> ImmutableKnownTypes =
            ImmutableArray.Create(
                KnownType.System_Object,
                KnownType.System_Int16,
                KnownType.System_Int32,
                KnownType.System_Int64,
                KnownType.System_UInt16,
                KnownType.System_UInt32,
                KnownType.System_UInt64,
                KnownType.System_Char,
                KnownType.System_Byte,
                KnownType.System_SByte,
                KnownType.System_Single,
                KnownType.System_Double,
                KnownType.System_Decimal,
                KnownType.System_Boolean,
                KnownType.System_String,

                KnownType.System_Collections_Immutable_ImmutableArray,
                KnownType.System_Collections_Immutable_ImmutableArray_T,
                KnownType.System_Collections_Immutable_ImmutableDictionary,
                KnownType.System_Collections_Immutable_ImmutableDictionary_TKey_TValue,
                KnownType.System_Collections_Immutable_ImmutableHashSet,
                KnownType.System_Collections_Immutable_ImmutableHashSet_T,
                KnownType.System_Collections_Immutable_ImmutableList,
                KnownType.System_Collections_Immutable_ImmutableList_T,
                KnownType.System_Collections_Immutable_ImmutableQueue,
                KnownType.System_Collections_Immutable_ImmutableQueue_T,
                KnownType.System_Collections_Immutable_ImmutableSortedDictionary,
                KnownType.System_Collections_Immutable_ImmutableSortedDictionary_TKey_TValue,
                KnownType.System_Collections_Immutable_ImmutableSortedSet,
                KnownType.System_Collections_Immutable_ImmutableSortedSet_T,
                KnownType.System_Collections_Immutable_ImmutableStack,
                KnownType.System_Collections_Immutable_ImmutableStack_T
            );

        private static bool IsLinqMethod(IMethodSymbol methodSymbol)
        {
            return methodSymbol.ContainingType.Is(KnownType.System_Linq_Enumerable) ||
                methodSymbol.ContainingType.Is(KnownType.System_Linq_ImmutableArrayExtensions);
        }
    }
}
