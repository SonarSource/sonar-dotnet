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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotCallInsecureSecurityAlgorithmBase<TSyntaxKind, TInvocationExpressionSyntax, TObjectCreationExpressionSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
        where TObjectCreationExpressionSyntax : SyntaxNode
    {
        protected abstract ISet<string> AlgorithmParameterlessFactoryMethods { get; }
        protected abstract ISet<string> AlgorithmParameterizedFactoryMethods { get; }
        protected abstract ISet<string> FactoryParameterNames { get; }
        protected abstract TSyntaxKind ObjectCreation { get; }
        protected abstract TSyntaxKind Invocation { get; }
        protected abstract TSyntaxKind StringLiteral { get; }
        protected abstract ILanguageFacade LanguageFacade { get; }
        private protected abstract ImmutableArray<KnownType> AlgorithmTypes { get; }

        protected abstract SyntaxNode InvocationExpression(TInvocationExpressionSyntax invocation);
        protected abstract bool IsInsecureBaseAlgorithmCreationFactoryCall(IMethodSymbol methodSymbol, TInvocationExpressionSyntax invocationExpression);
        protected abstract Location Location(TObjectCreationExpressionSyntax objectCreation);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(LanguageFacade.GeneratedCodeRecognizer, CheckObjectCreation, ObjectCreation);
            context.RegisterSyntaxNodeActionInNonGenerated(LanguageFacade.GeneratedCodeRecognizer, CheckInvocation, Invocation);
        }

        private void CheckInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (TInvocationExpressionSyntax)context.Node;

            if (!(context.SemanticModel.GetSymbolInfo(InvocationExpression(invocation)).Symbol is IMethodSymbol methodSymbol))
            {
                return;
            }

            if (methodSymbol.ReturnType.DerivesFromAny(AlgorithmTypes)
                || IsInsecureBaseAlgorithmCreationFactoryCall(methodSymbol, invocation))
            {
                ReportAllDiagnostics(context, invocation.GetLocation());
            }
        }

        private void CheckObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (TObjectCreationExpressionSyntax)context.Node;

            var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
            if (typeInfo.Type == null || typeInfo.Type is IErrorTypeSymbol)
            {
                return;
            }

            if (typeInfo.Type.DerivesFromAny(AlgorithmTypes))
            {
                ReportAllDiagnostics(context, Location(objectCreation));
            }
        }

        private void ReportAllDiagnostics(SyntaxNodeAnalysisContext context, Location location)
        {
            foreach (var supportedDiagnostic in SupportedDiagnostics)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(supportedDiagnostic, location));
            }
        }
    }
}
