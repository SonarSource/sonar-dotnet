/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.CodeDom.Compiler;

namespace SonarAnalyzer.Helpers;

[GeneratedCode("Copied from Roslyn", "5a1cc5f83e4baba57f0355a685a5d1f487bfac66")]
internal static partial class SyntaxNodeExtensions
{
    /// <summary>
    /// Returns true if is a given token is a child token of a certain type of parent node.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent node.</typeparam>
    /// <param name="node">The node that we are testing.</param>
    /// <param name="childGetter">A function that, when given the parent node, returns the child token we are interested in.</param>
    // Copy of
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/SyntaxNodeExtensions.cs#L142
    public static bool IsChildNode<TParent>(this SyntaxNode node, Func<TParent, SyntaxNode> childGetter) where TParent : SyntaxNode
    {
        var ancestor = node.GetAncestor<TParent>();
        if (ancestor == null)
        {
            return false;
        }

        var ancestorNode = childGetter(ancestor);

        return node == ancestorNode;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/SyntaxNodeExtensions.cs#L56
    public static TNode GetAncestor<TNode>(this SyntaxNode node) where TNode : SyntaxNode
    {
        var current = node.Parent;
        while (current != null)
        {
            if (current is TNode tNode)
            {
                return tNode;
            }

            current = current.GetParent(ascendOutOfTrivia: true);
        }

        return null;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/SyntaxNodeExtensions.cs#L811
    public static SyntaxNode GetParent(this SyntaxNode node, bool ascendOutOfTrivia)
    {
        var parent = node.Parent;
        if (parent == null && ascendOutOfTrivia)
        {
            if (node is IStructuredTriviaSyntax structuredTrivia)
            {
                parent = structuredTrivia.ParentTrivia.Token.Parent;
            }
        }

        return parent;
    }
}
