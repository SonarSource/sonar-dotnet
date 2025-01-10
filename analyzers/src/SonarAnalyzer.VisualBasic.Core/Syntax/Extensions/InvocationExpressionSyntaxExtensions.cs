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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;

public static class InvocationExpressionSyntaxExtensions
{
    public static bool IsMemberAccessOnKnownType(this InvocationExpressionSyntax invocation, string identifierName, KnownType knownType, SemanticModel semanticModel) =>
        invocation.Expression is MemberAccessExpressionSyntax memberAccess
        && memberAccess.IsMemberAccessOnKnownType(identifierName, knownType, semanticModel);

    public static IEnumerable<ISymbol> GetArgumentSymbolsOfKnownType(this InvocationExpressionSyntax invocation, KnownType knownType, SemanticModel semanticModel) =>
        invocation.ArgumentList.Arguments.GetSymbolsOfKnownType(knownType, semanticModel);

    public static bool HasExactlyNArguments(this InvocationExpressionSyntax invocation, int count) =>
        invocation?.ArgumentList is null
            ? count == 0
            : invocation.ArgumentList.Arguments.Count == count;

    public static bool TryGetOperands(this InvocationExpressionSyntax invocation, out SyntaxNode left, out SyntaxNode right)
    {
        left = right = null;

        if (invocation is { Expression: MemberAccessExpressionSyntax access })
        {
            left = access.Expression ?? invocation.GetParentConditionalAccessExpression()?.Expression;
            right = access.Name;
        }

        return left is not null && right is not null;
    }

    public static SyntaxToken? GetMethodCallIdentifier(this InvocationExpressionSyntax invocation) =>
        invocation?.Expression.GetIdentifier();

    public static bool IsMethodInvocation(this InvocationExpressionSyntax expression, KnownType type, string methodName, SemanticModel semanticModel) =>
        semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol methodSymbol &&
        methodSymbol.IsInType(type) &&
        // vbnet is case insensitive
        methodName.Equals(methodSymbol.Name, StringComparison.InvariantCultureIgnoreCase);
}
