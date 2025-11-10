/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Syntax.Utilities;

internal class AttributeSyntaxSymbolMapping
{
    public AttributeSyntax Node { get; }
    public IMethodSymbol Symbol { get; }

    private AttributeSyntaxSymbolMapping(AttributeSyntax node, IMethodSymbol symbol)
    {
        Node = node;
        Symbol = symbol;
    }

    public static IEnumerable<AttributeSyntaxSymbolMapping> GetAttributesForParameter(ParameterSyntax parameter, SemanticModel model) =>
        parameter.AttributeLists
            .SelectMany(x => x.Attributes)
            .Select(x => new AttributeSyntaxSymbolMapping(x, model.GetSymbolInfo(x).Symbol as IMethodSymbol))
            .Where(x => x.Symbol is not null);
}
