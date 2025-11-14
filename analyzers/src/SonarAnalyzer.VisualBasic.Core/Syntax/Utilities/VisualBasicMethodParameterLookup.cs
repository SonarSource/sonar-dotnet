/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;

public class VisualBasicMethodParameterLookup : MethodParameterLookupBase<ArgumentSyntax>
{
    public VisualBasicMethodParameterLookup(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        : this(invocation.ArgumentList, semanticModel) { }

    public VisualBasicMethodParameterLookup(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
        : this(invocation.ArgumentList, methodSymbol) { }

    public VisualBasicMethodParameterLookup(ArgumentListSyntax argumentList, SemanticModel semanticModel)
        : base(argumentList.Arguments, semanticModel.GetSymbolInfo(argumentList.Parent)) { }

    public VisualBasicMethodParameterLookup(ArgumentListSyntax argumentList, IMethodSymbol methodSymbol)
        : base(argumentList.Arguments, methodSymbol) { }

    protected override SyntaxToken? GetNameColonIdentifier(ArgumentSyntax argument) =>
        (argument as SimpleArgumentSyntax)?.NameColonEquals?.Name.Identifier;

    protected override SyntaxToken? GetNameEqualsIdentifier(ArgumentSyntax argument) =>
       null;

    protected override SyntaxNode Expression(ArgumentSyntax argument) =>
        argument.GetExpression();
}
