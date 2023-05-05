/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

        internal static bool TryGetOperands(this InvocationExpressionSyntax invocation, out SyntaxNode leftOperand, out SyntaxNode rightOperand)
        {
            leftOperand = rightOperand = null;

            if (invocation is { Expression: MemberAccessExpressionSyntax access })
            {
                leftOperand = access.Expression ?? invocation.GetParentConditionalAccessExpression()?.Expression;
                rightOperand = access.Name;
            }

            return leftOperand is not null && rightOperand is not null;
        }
    }
}
