/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Common
{
    public class SyntaxNodeWithSymbol<TSyntaxNode, TSymbol>
        where TSyntaxNode : SyntaxNode
        where TSymbol : ISymbol
    {
        public SyntaxNodeWithSymbol(TSyntaxNode syntax, TSymbol symbol)
        {
            Syntax = syntax;
            Symbol = symbol;
        }

        public TSyntaxNode Syntax { get; }
        public TSymbol Symbol { get; }
    }

    public static class SyntaxNodeWithSymbolHelper
    {
        public static SyntaxNodeWithSymbol<TSyntaxNode, TSymbol> ToSyntaxWithSymbol<TSyntaxNode, TSymbol>(
            this TSyntaxNode syntax, TSymbol symbol)
            where TSyntaxNode : SyntaxNode
            where TSymbol : ISymbol
        {
            return new SyntaxNodeWithSymbol<TSyntaxNode, TSymbol>(syntax, symbol);
        }

        public static SyntaxNodeWithSymbol<TSyntaxNode, TSymbol> ToSymbolWithSyntax<TSymbol, TSyntaxNode>(
            this TSymbol symbol, TSyntaxNode syntax)
            where TSyntaxNode : SyntaxNode
            where TSymbol : ISymbol
        {
            return new SyntaxNodeWithSymbol<TSyntaxNode, TSymbol>(syntax, symbol);
        }
    }
}
