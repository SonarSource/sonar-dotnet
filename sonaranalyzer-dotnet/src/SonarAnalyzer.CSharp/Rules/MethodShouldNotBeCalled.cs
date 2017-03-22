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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class MethodShouldNotBeCalled : SonarDiagnosticAnalyzer
    {
        internal abstract IEnumerable<MethodSignature> InvalidMethods { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(ReportIfInvalidMethodIsCalled, SyntaxKind.InvocationExpression);
        }

        private void ReportIfInvalidMethodIsCalled(SyntaxNodeAnalysisContext analysisContext)
        {
            var invocation = (InvocationExpressionSyntax)analysisContext.Node;

            var identifier = GetMethodCallIdentifier(invocation);
            if (identifier == null)
            {
                return;
            }

            var methodName = InvalidMethods.FirstOrDefault(method => method.Name.Equals(identifier.Value.ValueText));
            if (methodName == null)
            {
                return;
            }

            var methodCallSymbol = analysisContext.SemanticModel.GetSymbolInfo(identifier.Value.Parent);
            if (methodCallSymbol.Symbol == null)
            {
                return;
            }

            if (!methodCallSymbol.Symbol.ContainingType.ConstructedFrom.Is(methodName.ContainingType))
            {
                return;
            }

            analysisContext.ReportDiagnostic(Diagnostic.Create(Rule, identifier.Value.GetLocation(),
                GetMethodTypeName(methodName)));
        }

        private SyntaxToken? GetMethodCallIdentifier(InvocationExpressionSyntax invocation)
        {
            var directMethodCall = invocation.Expression as IdentifierNameSyntax;
            if (directMethodCall != null)
            {
                return directMethodCall.Identifier;
            }

            var memberAccessCall = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccessCall != null)
            {
                return memberAccessCall.Name.Identifier;
            }

            return null;
        }

        private string GetMethodTypeName(MethodSignature method)
        {
            var containingTypeName = method.ContainingType.TypeName.Split('.').Last();
            return string.Concat(containingTypeName, ".", method.Name);
        }
    }
}
