﻿/*
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

namespace SonarAnalyzer.Helpers;

internal class VisualBasicMethodParameterLookup : MethodParameterLookupBase<ArgumentSyntax>
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
