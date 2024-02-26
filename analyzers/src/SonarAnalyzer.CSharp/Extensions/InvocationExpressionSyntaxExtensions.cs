/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Extensions
{
    internal static class InvocationExpressionSyntaxExtensions
    {
        internal static bool IsMemberAccessOnKnownType(this InvocationExpressionSyntax invocation, string identifierName, KnownType knownType, SemanticModel semanticModel) =>
            invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.IsMemberAccessOnKnownType(identifierName, knownType, semanticModel);

        internal static IEnumerable<ISymbol> GetArgumentSymbolsOfKnownType(this InvocationExpressionSyntax invocation, KnownType knownType, SemanticModel semanticModel) =>
            invocation.ArgumentList.Arguments.GetSymbolsOfKnownType(knownType, semanticModel);

        internal static bool HasExactlyNArguments(this InvocationExpressionSyntax invocation, int count) =>
            invocation?.ArgumentList != null && invocation.ArgumentList.Arguments.Count == count;

        internal static bool IsGetTypeCall(this InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation != null
            && (semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol && methodSymbol.IsGetTypeCall());

        internal static bool IsOnBase(this InvocationExpressionSyntax invocation) =>
            (invocation.Expression as MemberAccessExpressionSyntax)?.Expression is BaseExpressionSyntax;

        internal static bool IsEqualTo(this InvocationExpressionSyntax first, InvocationExpressionSyntax second, SemanticModel model) =>
            model.GetSymbolInfo(first).Symbol.Equals(model.GetSymbolInfo(second).Symbol);

        internal static bool TryGetOperands(this InvocationExpressionSyntax invocation, out SyntaxNode left, out SyntaxNode right)
        {
            (left, right) = invocation switch
            {
                { Expression: MemberAccessExpressionSyntax access } => (access.Expression, access.Name),
                { Expression: MemberBindingExpressionSyntax binding } => (invocation.GetParentConditionalAccessExpression()?.Expression, binding.Name),
                _ => (null, null)
            };

            return left is not null && right is not null;
        }

        internal static SyntaxToken? GetMethodCallIdentifier(this InvocationExpressionSyntax invocation) =>
            invocation?.Expression.GetIdentifier();
    }
}
