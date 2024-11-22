/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Helpers;

internal class AttributeSyntaxSymbolMapping
{
    public AttributeSyntax SyntaxNode { get; }
    public IMethodSymbol Symbol { get; }

    private AttributeSyntaxSymbolMapping(AttributeSyntax syntaxNode, IMethodSymbol symbol)
    {
        SyntaxNode = syntaxNode;
        Symbol = symbol;
    }

    public static IEnumerable<AttributeSyntaxSymbolMapping> GetAttributesForParameter(ParameterSyntax parameter, SemanticModel semanticModel) =>
        parameter.AttributeLists
                 .SelectMany(al => al.Attributes)
                 .Select(attr => new AttributeSyntaxSymbolMapping(attr, semanticModel.GetSymbolInfo(attr).Symbol as IMethodSymbol))
                 .Where(attr => attr.Symbol != null);
}
