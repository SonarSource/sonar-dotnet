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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class OptionalParameterNotPassedToBaseCallBase<TInvocationExpressionSyntax>
        : SonarDiagnosticAnalyzer
        where TInvocationExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S3466";
        protected const string MessageFormat = "Pass the missing user-supplied parameter value{0} to this 'base' call.";

        protected void ReportOptionalParameterNotPassedToBase(SyntaxNodeAnalysisContext c, TInvocationExpressionSyntax invocation)
        {
            if (!(c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol calledMethod))
            {
                return;
            }

            int difference = calledMethod.Parameters.Length - GetArgumentCount(invocation);

            if (!calledMethod.IsVirtual ||
                difference == 0 ||
                !IsCallInsideOverride(invocation, calledMethod, c.SemanticModel))
            {
                return;
            }

            var pluralize = difference > 1
                ? "s"
                : string.Empty;
            c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, invocation.GetLocation(), pluralize));
        }

        protected abstract int GetArgumentCount(TInvocationExpressionSyntax invocation);
        protected abstract DiagnosticDescriptor Rule { get; }

        protected static bool IsCallInsideOverride(SyntaxNode invocation, IMethodSymbol calledMethod,
            SemanticModel semanticModel)
        {
            return semanticModel.GetEnclosingSymbol(invocation.SpanStart) is IMethodSymbol enclosingSymbol &&
                enclosingSymbol.IsOverride &&
                object.Equals(enclosingSymbol.OverriddenMethod, calledMethod);
        }
    }
}
