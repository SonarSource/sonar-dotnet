/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public abstract class OptionalParameterNotPassedToBaseCallBase<TInvocationExpressionSyntax>
        : SonarDiagnosticAnalyzer
        where TInvocationExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S3466";
        protected const string MessageFormat = "Pass the missing user-supplied parameter value{0} to this 'base' call.";

        protected void ReportOptionalParameterNotPassedToBase(SonarSyntaxNodeReportingContext c, TInvocationExpressionSyntax invocation)
        {
            if (!(c.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol calledMethod))
            {
                return;
            }

            int difference = calledMethod.Parameters.Length - GetArgumentCount(invocation);

            if (!calledMethod.IsVirtual ||
                difference == 0 ||
                !IsCallInsideOverride(invocation, calledMethod, c.Model))
            {
                return;
            }

            var pluralize = difference > 1
                ? "s"
                : string.Empty;
            c.ReportIssue(Rule, invocation, pluralize);
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
