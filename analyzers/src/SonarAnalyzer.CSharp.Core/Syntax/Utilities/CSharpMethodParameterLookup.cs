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

namespace SonarAnalyzer.CSharp.Core.Syntax.Utilities;

public class CSharpMethodParameterLookup : MethodParameterLookupBase<ArgumentSyntax>
{
    public CSharpMethodParameterLookup(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        : this(invocation.ArgumentList, semanticModel) { }

    public CSharpMethodParameterLookup(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
        : this(invocation.ArgumentList, methodSymbol) { }

    public CSharpMethodParameterLookup(BaseArgumentListSyntax argumentList, SemanticModel semanticModel)
        : base(argumentList.Arguments, semanticModel.GetSymbolInfo(argumentList.Parent)) { }

    public CSharpMethodParameterLookup(BaseArgumentListSyntax argumentList, IMethodSymbol methodSymbol)
        : base(argumentList.Arguments, methodSymbol) { }

    protected override SyntaxNode Expression(ArgumentSyntax argument) =>
        argument.Expression;

    protected override SyntaxToken? GetNameColonIdentifier(ArgumentSyntax argument) =>
        argument.NameColon?.Name.Identifier;

    protected override SyntaxToken? GetNameEqualsIdentifier(ArgumentSyntax argument) =>
        null;
}
