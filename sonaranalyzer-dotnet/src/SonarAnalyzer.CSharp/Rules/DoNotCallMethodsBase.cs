/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotCallMethodsBase : SonarDiagnosticAnalyzer
    {
        internal abstract IEnumerable<MethodSignature> CheckedMethods { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        protected virtual bool ShouldReportOnMethodCall(InvocationExpressionSyntax invocation,
            SemanticModel semanticModel, MethodSignature methodSignature) => true;

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext analysisContext)
        {
            var invocation = (InvocationExpressionSyntax)analysisContext.Node;

            if (!IsInValidContext(invocation, analysisContext.SemanticModel))
            {
                return;
            }

            var identifier = GetMethodCallIdentifier(invocation);
            if (identifier == null)
            {
                return;
            }

            var methodCallSymbol = analysisContext.SemanticModel.GetSymbolInfo(identifier.Value.Parent).Symbol;
            if (methodCallSymbol == null)
            {
                return;
            }

            var disallowedMethodSignature = FindDisallowedMethodSignature(identifier.Value, methodCallSymbol);
            if (disallowedMethodSignature == null)
            {
                return;
            }

            if (ShouldReportOnMethodCall(invocation, analysisContext.SemanticModel, disallowedMethodSignature))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], identifier.Value.GetLocation(),
                    disallowedMethodSignature.ToShortName()));
            }
        }

        protected virtual bool IsInValidContext(InvocationExpressionSyntax invocationSyntax,
            SemanticModel semanticModel) => true;

        protected SyntaxToken? GetMethodCallIdentifier(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax directMethodCall)
            {
                return directMethodCall.Identifier;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccessCall)
            {
                return memberAccessCall.Name.Identifier;
            }

            return null;
        }

        private MethodSignature FindDisallowedMethodSignature(SyntaxToken identifier, ISymbol methodCallSymbol)
        {
            return CheckedMethods
                .Where(method => method.Name.Equals(identifier.ValueText))
                .FirstOrDefault(m => methodCallSymbol.ContainingType.ConstructedFrom.Is(m.ContainingType));
        }
    }
}
