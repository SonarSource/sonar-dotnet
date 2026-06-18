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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;

public static class InvocationExpressionSyntaxExtensions
{
    extension(InvocationExpressionSyntax invocation)
    {
        public bool IsMemberAccessOnKnownType(string identifierName, KnownType knownType, SemanticModel semanticModel) =>
            invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.IsMemberAccessOnKnownType(identifierName, knownType, semanticModel);

        public IEnumerable<ISymbol> GetArgumentSymbolsOfKnownType(KnownType knownType, SemanticModel semanticModel) =>
            invocation.ArgumentList.Arguments.GetSymbolsOfKnownType(knownType, semanticModel);

        public bool HasExactlyNArguments(int count) =>
            invocation?.ArgumentList is null
                ? count == 0
                : invocation.ArgumentList.Arguments.Count == count;

        public Pair<SyntaxNode, SyntaxNode> Operands =>
            invocation is { Expression: MemberAccessExpressionSyntax access }
                ? new(access.Expression ?? invocation.GetParentConditionalAccessExpression()?.Expression, access.Name)
                : default;

        public SyntaxToken? MethodCallIdentifier =>
            invocation?.Expression.GetIdentifier();

        public bool IsMethodInvocation(KnownType type, string methodName, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
            methodSymbol.IsInType(type) &&
            // vbnet is case insensitive
            methodName.Equals(methodSymbol.Name, StringComparison.InvariantCultureIgnoreCase);
    }
}
