/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
        protected abstract DiagnosticDescriptor Rule { get; }
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        internal abstract ISet<KnownType> AlgorithmTypes { get; }
        protected abstract ISet<string> AlgorithmParameterlessFactoryMethods { get; }
        protected abstract ISet<string> AlgorithmParameteredFactoryMethods { get; }
        protected abstract ISet<string> FactoryParameterNames { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckObjectCreation(c),
                SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckInvocation(c),
                SyntaxKind.InvocationExpression);
        }

        private void CheckInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            if (methodSymbol.ReturnType.DerivesFromAny(AlgorithmTypes) ||
                IsInsecureBaseAlgorithmCreationFactoryCall(methodSymbol, invocation.ArgumentList))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
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
                context.ReportDiagnostic(Diagnostic.Create(Rule, objectCreation.Type.GetLocation()));
            }
        }
    }
}
