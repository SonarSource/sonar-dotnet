/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class InvocationExpressionSyntaxExtensions
{
    extension(InvocationExpressionSyntax invocation)
    {
        public bool IsMemberAccessOnKnownType(string identifierName, KnownType knownType, SemanticModel model) =>
            invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.IsMemberAccessOnKnownType(identifierName, knownType, model);

        public IEnumerable<ISymbol> GetArgumentSymbolsOfKnownType(KnownType knownType, SemanticModel model) =>
            invocation.ArgumentList.Arguments.GetSymbolsOfKnownType(knownType, model);

        public bool HasExactlyNArguments(int count) =>
            invocation is not null && invocation.ArgumentList.Arguments.Count == count;

        public bool IsGetTypeCall(SemanticModel model) =>
            invocation is not null
            && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { IsGetTypeCall: true };

        public bool IsOnBase =>
            (invocation.Expression as MemberAccessExpressionSyntax)?.Expression is BaseExpressionSyntax;

        public bool IsEqualTo(InvocationExpressionSyntax second, SemanticModel model) =>
            Pair.From(model.GetSymbolInfo(invocation).Symbol, model.GetSymbolInfo(second).Symbol) switch
            {
                // the nameof(someVariable) is considered an Invocation Expression, but it is not a method call, and GetSymbolInfo returns null for it.
                (null, null) when invocation.GetName() == "nameof" && second.GetName() == "nameof" => model.GetConstantValue(invocation).Equals(model.GetConstantValue(second)),
                ({ } firstSymbol, { } secondSymbol) => firstSymbol.Equals(secondSymbol),
                _ => false
            };

        public Pair<SyntaxNode, SyntaxNode> Operands =>
            invocation switch
            {
                { Expression: MemberAccessExpressionSyntax access } => new Pair<SyntaxNode, SyntaxNode>(access.Expression, access.Name),
                { Expression: MemberBindingExpressionSyntax binding } => new(invocation.GetParentConditionalAccessExpression()?.Expression, binding.Name),
                _ => default
            };

        public SyntaxToken? MethodCallIdentifier =>
            invocation?.Expression.GetIdentifier();

        public bool IsNameof(SemanticModel semanticModel) =>
            invocation is not null &&
            invocation.Expression is IdentifierNameSyntax identifierNameSyntax &&
            identifierNameSyntax.Identifier.ValueText == Common.SyntaxConstants.NameOfKeywordText &&
            semanticModel.GetSymbolInfo(invocation).Symbol?.Kind != SymbolKind.Method;

        public bool IsMethodInvocation(KnownType type, string methodName, SemanticModel semanticModel) =>
            invocation.Expression.NameIs(methodName) &&
            semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
            methodSymbol.IsInType(type);

        public bool IsMethodInvocation(ImmutableArray<KnownType> types, string methodName, SemanticModel semanticModel) =>
            invocation.Expression.NameIs(methodName) &&
            semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
            methodSymbol.IsInType(types);
    }
}
