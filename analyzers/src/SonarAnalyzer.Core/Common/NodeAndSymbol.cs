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

namespace SonarAnalyzer.Common;

public class NodeAndSymbol<TSyntaxNode, TSymbol>
    where TSyntaxNode : SyntaxNode
    where TSymbol : ISymbol
{
    public TSyntaxNode Node { get; }
    public TSymbol Symbol { get; }

    public NodeAndSymbol(TSyntaxNode node, TSymbol symbol)
    {
        Node = node;
        Symbol = symbol;
    }
}

public class NodeAndSymbol : NodeAndSymbol<SyntaxNode, ISymbol>
{
    public NodeAndSymbol(SyntaxNode node, ISymbol symbol) : base(node, symbol) { }
}
