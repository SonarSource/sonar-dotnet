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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class InvocationExpressionSyntaxExtensions
{
    public static bool IsMemberAccessOnKnownType(this InvocationExpressionSyntax invocation, string identifierName, KnownType knownType, SemanticModel model) =>
        invocation.Expression is MemberAccessExpressionSyntax memberAccess
        && memberAccess.IsMemberAccessOnKnownType(identifierName, knownType, model);

    public static IEnumerable<ISymbol> GetArgumentSymbolsOfKnownType(this InvocationExpressionSyntax invocation, KnownType knownType, SemanticModel model) =>
        invocation.ArgumentList.Arguments.GetSymbolsOfKnownType(knownType, model);

    public static bool HasExactlyNArguments(this InvocationExpressionSyntax invocation, int count) =>
        invocation is not null && invocation.ArgumentList.Arguments.Count == count;

    public static bool IsGetTypeCall(this InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation is not null
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol && methodSymbol.IsGetTypeCall();

    public static bool IsOnBase(this InvocationExpressionSyntax invocation) =>
        (invocation.Expression as MemberAccessExpressionSyntax)?.Expression is BaseExpressionSyntax;

    public static bool IsEqualTo(this InvocationExpressionSyntax first, InvocationExpressionSyntax second, SemanticModel model) =>
        (model.GetSymbolInfo(first).Symbol, model.GetSymbolInfo(second).Symbol) switch
        {
            // the nameof(someVariable) is considered an Invocation Expression, but it is not a method call, and GetSymbolInfo returns null for it.
            (null, null) when first.GetName() == "nameof" && second.GetName() == "nameof" => model.GetConstantValue(first).Equals(model.GetConstantValue(second)),
            ({ } firstSymbol, { } secondSymbol) => firstSymbol.Equals(secondSymbol),
            _ => false
        };

    public static bool TryGetOperands(this InvocationExpressionSyntax invocation, out SyntaxNode left, out SyntaxNode right)
    {
        (left, right) = invocation switch
        {
            { Expression: MemberAccessExpressionSyntax access } => (access.Expression, access.Name),
            { Expression: MemberBindingExpressionSyntax binding } => (invocation.GetParentConditionalAccessExpression()?.Expression, binding.Name),
            _ => (null, null)
        };

        return left is not null && right is not null;
    }

    public static SyntaxToken? GetMethodCallIdentifier(this InvocationExpressionSyntax invocation) =>
        invocation?.Expression.GetIdentifier();

    public static bool IsNameof(this InvocationExpressionSyntax expression, SemanticModel semanticModel) =>
        expression is not null &&
        expression.Expression is IdentifierNameSyntax identifierNameSyntax &&
        identifierNameSyntax.Identifier.ValueText == Common.SyntaxConstants.NameOfKeywordText &&
        semanticModel.GetSymbolInfo(expression).Symbol?.Kind != SymbolKind.Method;

    public static bool IsMethodInvocation(this InvocationExpressionSyntax invocation, KnownType type, string methodName, SemanticModel semanticModel) =>
        invocation.Expression.NameIs(methodName) &&
        semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
        methodSymbol.IsInType(type);

    public static bool IsMethodInvocation(this InvocationExpressionSyntax invocation, ImmutableArray<KnownType> types, string methodName, SemanticModel semanticModel) =>
        invocation.Expression.NameIs(methodName) &&
        semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
        methodSymbol.IsInType(types);
}
