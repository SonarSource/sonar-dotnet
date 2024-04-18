// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Shared.Extensions;

[ExcludeFromCodeCoverage]
internal static class SyntaxNodeExtensions
{
    /// <summary>
    /// Returns true if is a given token is a child token of a certain type of parent node.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent node.</typeparam>
    /// <param name="node">The node that we are testing.</param>
    /// <param name="childGetter">A function that, when given the parent node, returns the child token we are interested in.</param>
    /// <remarks>
    /// Copied from <seealso href="https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/SyntaxNodeExtensions.cs#L142"/>
    /// </remarks>
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
