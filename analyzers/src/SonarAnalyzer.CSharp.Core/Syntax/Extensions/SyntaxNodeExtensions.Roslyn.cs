// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.CSharp.Extensions;

[ExcludeFromCodeCoverage]
public static class SyntaxNodeExtensions
{
    /// <summary>
    /// Returns the left hand side of a conditional access expression. Returns c in case like a?.b?[0].c?.d.e?.f if d is passed.
    /// </summary>
    /// <remarks>Copied from <seealso href="https://github.com/dotnet/roslyn/blob/18f20b489/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L188">
    /// Roslyn SyntaxNodeExtensions</seealso></remarks>
    public static ConditionalAccessExpressionSyntax GetParentConditionalAccessExpression(this SyntaxNode node)
    {
        // Walk upwards based on the grammar/parser rules around ?. expressions (can be seen in
        // LanguageParser.ParseConsequenceSyntax).

        // These are the parts of the expression that the ?... expression can end with.  Specifically:
        //
        //  1.      x?.y.M()            // invocation
        //  2.      x?.y[...];          // element access
        //  3.      x?.y.z              // member access
        //  4.      x?.y                // member binding
        //  5.      x?[y]               // element binding
        var current = node;

        if ((current.IsParentKind(SyntaxKind.SimpleMemberAccessExpression, out MemberAccessExpressionSyntax memberAccess) && memberAccess.Name == current) ||
            (current.IsParentKind(SyntaxKind.MemberBindingExpression, out MemberBindingExpressionSyntax memberBinding) && memberBinding.Name == current))
        {
            current = current.Parent;
        }

        // Effectively, if we're on the RHS of the ? we have to walk up the RHS spine first until we hit the first
        // conditional access.
        while ((current.Kind() is SyntaxKind.InvocationExpression
            or SyntaxKind.ElementAccessExpression
            or SyntaxKind.SimpleMemberAccessExpression
            or SyntaxKind.MemberBindingExpression
            or SyntaxKind.ElementBindingExpression
            // Optional exclamations might follow the conditional operation. For example: a.b?.$$c!!!!()
            or SyntaxKindEx.SuppressNullableWarningExpression) &&
            current.Parent is not ConditionalAccessExpressionSyntax)
        {
            current = current.Parent;
        }

        // Two cases we have to care about:
        //
        //      1. a?.b.$$c.d        and
        //      2. a?.b.$$c.d?.e...
        //
        // Note that `a?.b.$$c.d?.e.f?.g.h.i` falls into the same bucket as two.  i.e. the parts after `.e` are
        // lower in the tree and are not seen as we walk upwards.
        //
        //
        // To get the root ?. (the one after the `a`) we have to potentially consume the first ?. on the RHS of the
        // right spine (i.e. the one after `d`).  Once we do this, we then see if that itself is on the RHS of a
        // another conditional, and if so we hten return the one on the left.  i.e. for '2' this goes in this direction:
        //
        //      a?.b.$$c.d?.e           // it will do:
        //           ----->
        //       <---------
        //
        // Note that this only one CAE consumption on both sides.  GetRootConditionalAccessExpression can be used to
        // get the root parent in a case like:
        //
        //      x?.y?.z?.a?.b.$$c.d?.e.f?.g.h.i         // it will do:
        //                    ----->
        //                <---------
        //             <---
        //          <---
        //       <---
        if (current.IsParentKind(SyntaxKind.ConditionalAccessExpression, out ConditionalAccessExpressionSyntax conditional) &&
            conditional.Expression == current)
        {
            current = conditional;
        }

        if (current.IsParentKind(SyntaxKind.ConditionalAccessExpression, out conditional) &&
            conditional.WhenNotNull == current)
        {
            current = conditional;
        }

        return current as ConditionalAccessExpressionSyntax;
    }

    /// <summary>
    /// Call on the `.y` part of a `x?.y` to get the entire `x?.y` conditional access expression.  This also works
    /// when there are multiple chained conditional accesses.  For example, calling this on '.y' or '.z' in
    /// `x?.y?.z` will both return the full `x?.y?.z` node.  This can be used to effectively get 'out' of the RHS of
    /// a conditional access, and commonly represents the full standalone expression that can be operated on
    /// atomically.
    /// </summary>
    /// <remarks>Copied from Roslyn SyntaxNodeExtensions.</remarks>
    public static ConditionalAccessExpressionSyntax GetRootConditionalAccessExpression(this SyntaxNode node)
    {
        // Once we've walked up the entire RHS, now we continually walk up the conditional accesses until we're at
        // the root. For example, if we have `a?.b` and we're on the `.b`, this will give `a?.b`.  Similarly with
        // `a?.b?.c` if we're on either `.b` or `.c` this will result in `a?.b?.c` (i.e. the root of this CAE
        // sequence).
        var current = node.GetParentConditionalAccessExpression();
        while (current.IsParentKind(SyntaxKind.ConditionalAccessExpression, out ConditionalAccessExpressionSyntax conditional) &&
            conditional.WhenNotNull == current)
        {
            current = conditional;
        }

        return current;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L347
    public static bool IsLeftSideOfAssignExpression(this SyntaxNode node)
        => node?.Parent is AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignment &&
           assignment.Left == node;

    // Copy of
    // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L43C1-L45C1
    public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind)
        => Microsoft.CodeAnalysis.CSharpExtensions.IsKind(node?.Parent, kind);

    // Copy of
    // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L46
    public static bool IsParentKind<T>(this SyntaxNode node, SyntaxKind kind, out T result) where T : SyntaxNode
    {
        if (node?.Parent?.IsKind(kind) is true && node.Parent is T t)
        {
            result = t;
            return true;
        }
        result = null;
        return false;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L351
    public static bool IsLeftSideOfAnyAssignExpression(this SyntaxNode node)
    {
        return node?.Parent != null &&
            node.Parent.IsAnyAssignExpression() &&
            ((AssignmentExpressionSyntax)node.Parent).Left == node;
    }

    // Copy of
    // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L323
    public static bool IsAnyAssignExpression(this SyntaxNode node)
        => SyntaxFacts.IsAssignmentExpression(node.Kind());
}
