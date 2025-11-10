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

namespace SonarAnalyzer.VisualBasic.Core.Extensions;

public static class ISymbolExtensions
{
    public static IEnumerable<SyntaxNode> GetLocationNodes(this ISymbol symbol, SyntaxNode node) =>
        symbol.Locations.SelectMany(location => GetDescendantNodes(location, node));

    public static IEnumerable<SyntaxNode> GetDescendantNodes(Location location, SyntaxNode node) =>
        location.SourceTree?.GetRoot() is { } locationRootNode
        && locationRootNode == node.SyntaxTree.GetRoot()
        && locationRootNode.FindNode(location.SourceSpan)
                           .FirstAncestorOrSelf<VariableDeclaratorSyntax>() is { } declaration
            ? declaration.DescendantNodes()
            : Enumerable.Empty<SyntaxNode>();
}
