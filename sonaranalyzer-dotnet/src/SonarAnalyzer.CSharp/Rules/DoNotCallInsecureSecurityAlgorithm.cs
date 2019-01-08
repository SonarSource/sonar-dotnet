/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    public abstract class DoNotCallInsecureSecurityAlgorithm : SonarDiagnosticAnalyzer
    {
        internal abstract ImmutableArray<KnownType> AlgorithmTypes { get; }
        protected abstract ISet<string> AlgorithmParameterlessFactoryMethods { get; }
        protected abstract ISet<string> AlgorithmParameteredFactoryMethods { get; }
        protected abstract ISet<string> FactoryParameterNames { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckObjectCreation,
                SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckInvocation,
                SyntaxKind.InvocationExpression);
        }

        private void CheckInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!(context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol methodSymbol))
            {
                return;
            }

            if (methodSymbol.ReturnType.DerivesFromAny(AlgorithmTypes) ||
                IsInsecureBaseAlgorithmCreationFactoryCall(methodSymbol, invocation.ArgumentList))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], invocation.GetLocation()));
            }
        }

        private bool IsInsecureBaseAlgorithmCreationFactoryCall(IMethodSymbol methodSymbol,
            ArgumentListSyntax argumentList)
        {
            if (argumentList == null ||
                methodSymbol?.ContainingType == null ||
                methodSymbol.Name == null)
            {
                return false;
            }

            var methodFullName = $"{methodSymbol.ContainingType}.{methodSymbol.Name}";

            if (argumentList.Arguments.Count == 0)
            {
                return AlgorithmParameterlessFactoryMethods.Contains(methodFullName);
            }

            if (argumentList.Arguments.Count > 1 ||
                !argumentList.Arguments.First().Expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            if (!AlgorithmParameteredFactoryMethods.Contains(methodFullName))
            {
                return false;
            }

            var literalExpressionSyntax = (LiteralExpressionSyntax)argumentList.Arguments.First().Expression;
            return FactoryParameterNames.Any(alg => alg.Equals(literalExpressionSyntax.Token.ValueText, StringComparison.Ordinal));
        }

        private void CheckObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

            var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
            if (typeInfo.ConvertedType == null || typeInfo.ConvertedType is IErrorTypeSymbol)
            {
                return;
            }

            if (typeInfo.ConvertedType.DerivesFromAny(AlgorithmTypes))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], objectCreation.Type.GetLocation()));
            }
        }
    }
}
