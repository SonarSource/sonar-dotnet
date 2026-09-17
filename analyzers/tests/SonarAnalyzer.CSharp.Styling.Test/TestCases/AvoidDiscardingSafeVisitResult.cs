using System;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Core.Common;

class SomeWalker : ISafeSyntaxWalker
{
    public SomeWalker Self => this;
    public bool SafeVisit(SyntaxNode syntaxNode) => true;
}

class SomeUnrelatedType
{
    // Not an ISafeSyntaxWalker, just happens to share the method name.
    public bool SafeVisit(SyntaxNode syntaxNode) => true;
}

class Sample
{
    void Standalone(SomeWalker walker, SyntaxNode node)
    {
        walker.SafeVisit(node); // Noncompliant {{Check the return value of 'SafeVisit': 'false' means the walk was aborted and the collected data can't be trusted.}}
    //  ^^^^^^^^^^^^^^^^^^^^^^
    }

    void StandaloneOnNewInstance(SyntaxNode node)
    {
        new SomeWalker().SafeVisit(node); // Noncompliant
    }

    void StandaloneThroughInterface(ISafeSyntaxWalker walker, SyntaxNode node)
    {
        walker.SafeVisit(node); // Noncompliant
    }

    void UsedInCondition(SomeWalker walker, SyntaxNode node)
    {
        if (walker.SafeVisit(node)) // Compliant, result is used
        {
        }
    }

    void UsedInBooleanExpression(SomeWalker walker, SyntaxNode node)
    {
        _ = walker.SafeVisit(node) && true; // Compliant
    }

    bool Assigned(SomeWalker walker, SyntaxNode node)
    {
        var result = walker.SafeVisit(node); // Compliant, assigned to a variable
        result = walker.SafeVisit(node); // Compliant, plain reassignment
        _ = walker.SafeVisit(node); // Compliant, assigned to a discard
        return result;
    }

    bool Returned(SomeWalker walker, SyntaxNode node) =>
        walker.SafeVisit(node); // Compliant

    void UnrelatedSafeVisit(SomeUnrelatedType other, SyntaxNode node)
    {
        other.SafeVisit(node); // Compliant, not an ISafeSyntaxWalker
    }

    void LambdaConvertedToVoidDelegate(SomeWalker walker, SyntaxNode node)
    {
        Action<SyntaxNode> action = n => walker.SafeVisit(n); // Noncompliant, the lambda converts to a void-returning delegate
        Func<SyntaxNode, bool> func = n => walker.SafeVisit(n); // Compliant, the lambda converts to a bool-returning delegate
    }

    void ExpressionBodiedVoidMember(SomeWalker walker, SyntaxNode node) =>
        walker.SafeVisit(node); // Noncompliant, the containing member returns void

    void ConditionalAccess(SomeWalker walker, SyntaxNode node)
    {
        walker?.SafeVisit(node); // Noncompliant, the conditional-access statement discards the result
    }

    void ConditionalAccessWithMemberChain(SomeWalker walker, SyntaxNode node)
    {
        // SafeVisit is the outermost node of the postfix chain, so its direct Parent is still the
        // ConditionalAccessExpressionSyntax itself, regardless of the '.Self' hop in between.
        walker?.Self.SafeVisit(node); // Noncompliant, the whole conditional-access statement discards the result
    }

    void ConditionalAccessResultUsed(SomeWalker walker, SyntaxNode node)
    {
        _ = walker?.SafeVisit(node); // Compliant, the conditional-access result is assigned
    }

    void ConditionalAccessAfterPlainMemberAccess(SomeWalker walker, SyntaxNode node)
    {
        // The '?.' sits right before SafeVisit here (after a plain, non-conditional '.Self' hop), so the
        // invocation is still exactly the WhenNotNull of the ConditionalAccessExpressionSyntax - same shape
        // as 'walker?.SafeVisit(node)', just with an extra unconditional member access before the '?.'.
        walker.Self?.SafeVisit(node); // Noncompliant, the conditional-access statement discards the result
    }
}
